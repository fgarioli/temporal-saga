// Testes ponta a ponta do provisionamento de usuário com saga no Temporal. Node 18+, sem dependências.
// Dentro do compose: docker compose run --rm e2e. Do seu computador: node e2e/run.mjs
const URL_ = {
  cadastro: process.env.CADASTRO_URL ?? 'http://localhost:8090',
  keycloak: process.env.KEYCLOAK_URL ?? 'http://localhost:8180',
  mailpit: process.env.MAILPIT_URL ?? 'http://localhost:8025',
};
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const id = () => Math.random().toString(36).slice(2, 8);

async function http(method, url, body, headers = {}) {
  const res = await fetch(url, {
    method, headers: { 'content-type': 'application/json', ...headers },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const text = await res.text();
  let json; try { json = text ? JSON.parse(text) : undefined; } catch { json = text; }
  return { status: res.status, body: json, location: res.headers.get('location') };
}

function assert(cond, msg) { if (!cond) throw new Error(msg); }
function igual(a, b, msg) { assert(JSON.stringify(a) === JSON.stringify(b), `${msg}\n  esperado: ${JSON.stringify(b)}\n  obtido:   ${JSON.stringify(a)}`); }

async function aguardar(fn, descricao, timeoutMs = 30000) {
  const fim = Date.now() + timeoutMs;
  while (Date.now() < fim) { const r = await fn(); if (r) return r; await sleep(300); }
  throw new Error(`tempo esgotado esperando: ${descricao}`);
}

async function criarUsuario(username, email, nome = 'Ana Souza') {
  const r = await http('POST', `${URL_.cadastro}/usuarios`, { username, email, nome });
  igual(r.status, 202, 'POST /usuarios deveria responder 202');
  igual(r.location, `/usuarios/${r.body.id}`, 'Location do POST');
  igual([r.body.etapa, r.body.habilitado, r.body.historico], ['Pendente', false, []], 'o usuário nasce Pendente e desabilitado');
  return r.body.id;
}

const consultar = async (usuarioId) => (await http('GET', `${URL_.cadastro}/usuarios/${usuarioId}`)).body;

const aguardarEtapa = (usuarioId, etapa) => aguardar(async () => {
  const u = await consultar(usuarioId);
  return u?.etapa === etapa ? u : null;
}, `etapa ${etapa} do usuário ${usuarioId}`);

const aguardarFim = (usuarioId) => aguardar(async () => {
  const u = await consultar(usuarioId);
  return ['Concluido', 'Cancelado'].includes(u?.etapa) ? u : null;
}, `fim do provisionamento do usuário ${usuarioId}`);

async function adminKeycloak(method, caminho, body) {
  const res = await fetch(`${URL_.keycloak}/realms/master/protocol/openid-connect/token`, {
    method: 'POST', headers: { 'content-type': 'application/x-www-form-urlencoded' },
    body: 'grant_type=password&client_id=admin-cli&username=admin&password=admin',
  });
  const { access_token } = await res.json();
  return http(method, `${URL_.keycloak}/admin/realms/saga${caminho}`, body, { authorization: `Bearer ${access_token}` });
}
const usuariosNoKeycloak = async (username) => (await adminKeycloak('GET', `/users?exact=true&username=${encodeURIComponent(username)}`)).body;

const cenarios = [];
const cenario = (nome, fn) => cenarios.push({ nome, fn });
let primeiro;

cenario('caminho feliz: o usuário nasce desabilitado no POST e a saga o leva até Concluido, habilitado, com a identidade habilitada no Keycloak e o e-mail no Mailpit', async () => {
  const username = `ana-${id()}`; const email = `${username}@exemplo.com`;
  const usuarioId = await criarUsuario(username, email);
  const fim = await aguardarFim(usuarioId);
  igual(fim.historico, ['CriandoIdentidade', 'HabilitandoUsuario', 'LiberandoAcesso', 'EnviandoBoasVindas', 'Concluido'], 'histórico');
  igual([fim.etapa, fim.habilitado], ['Concluido', true], 'etapa e habilitado do usuário');
  const noKeycloak = await usuariosNoKeycloak(username);
  igual(noKeycloak.length, 1, 'usuário no Keycloak');
  igual(noKeycloak[0].id, fim.keycloakId, 'keycloakId gravado no usuário');
  igual(noKeycloak[0].enabled, true, 'identidade habilitada no Keycloak');
  await aguardar(async () => {
    const r = await http('GET', `${URL_.mailpit}/api/v1/messages`);
    return r.body.messages?.some((m) => m.To?.some((t) => t.Address === email));
  }, 'e-mail de boas-vindas');
  primeiro = { usuarioId, username, keycloakId: fim.keycloakId };
});

cenario('perfil padrão: o Keycloak dá o papel "cliente" a todo usuário novo, sem passo na saga', async () => {
  const papeis = (await adminKeycloak('GET', `/users/${primeiro.keycloakId}/role-mappings/realm/composite`)).body;
  assert(papeis.some((p) => p.name === 'cliente'), `papéis: ${JSON.stringify(papeis.map((p) => p.name))}`);
});

cenario('Keycloak recusa (username já existe lá): compensa rejeitando o usuário, que fica desabilitado e Cancelado', async () => {
  const username = `bia-${id()}`;
  const criado = await adminKeycloak('POST', '/users', { username, email: `${username}@outro-sistema.com`, firstName: 'Bia', lastName: 'Outro', enabled: true });
  igual(criado.status, 201, 'usuário criado direto no Keycloak, por outro sistema');
  const usuarioId = await criarUsuario(username, `${username}@exemplo.com`, 'Bia Lima');
  const fim = await aguardarFim(usuarioId);
  igual(fim.historico, ['CriandoIdentidade', 'RejeitandoUsuario', 'Cancelado'], 'histórico');
  assert(fim.motivo?.includes('Keycloak'), `motivo: ${fim.motivo}`);
  igual([fim.etapa, fim.habilitado], ['Cancelado', false], 'etapa e habilitado do usuário rejeitado');
});

cenario('username já usado no sistema: o POST responde 409 e nenhuma saga é iniciada', async () => {
  const r = await http('POST', `${URL_.cadastro}/usuarios`, { username: primeiro.username, email: `repetido-${id()}@exemplo.com`, nome: 'Ana Souza' });
  igual(r.status, 409, 'POST /usuarios com username em uso deveria responder 409');
  igual(r.body, { erro: 'username_em_uso', mensagem: `O username ${primeiro.username} já está em uso` }, 'corpo da recusa');
});

cenario('falha temporária do Keycloak ao criar: resolvida com nova tentativa, sem compensar', async () => {
  igual((await http('POST', `${URL_.cadastro}/simulations/failures`, { quantity: 2 })).status, 204, 'simulação sem alvo vale keycloak-create');
  const username = `caio-${id()}`;
  const usuarioId = await criarUsuario(username, `${username}@exemplo.com`, 'Caio Reis');
  igual((await aguardarFim(usuarioId)).etapa, 'Concluido', 'etapa');
  igual((await usuariosNoKeycloak(username)).length, 1, 'um usuário só no Keycloak');
});

cenario('usuário bloqueado durante a saga: a identidade existe desabilitada até o passo LiberarAcesso terminar', async () => {
  igual((await http('POST', `${URL_.cadastro}/simulations/failures`, { quantity: 4, alvo: 'keycloak-enable' })).status, 204, 'simulação keycloak-enable');
  igual((await http('POST', `${URL_.cadastro}/simulations/failures`, { quantity: 1, alvo: 'keycloak-delete' })).status, 400, 'alvo desconhecido');
  const username = `elisa-${id()}`;
  const usuarioId = await criarUsuario(username, `${username}@exemplo.com`, 'Elisa Prado');
  const durante = await aguardarEtapa(usuarioId, 'LiberandoAcesso');
  const noKeycloak = await usuariosNoKeycloak(username);
  igual(noKeycloak.length, 1, 'a identidade já existe no Keycloak');
  igual(noKeycloak[0].enabled, false, 'identidade ainda desabilitada enquanto LiberandoAcesso repete');
  igual(noKeycloak[0].id, durante.keycloakId, 'keycloakId já gravado no usuário');
  const fim = await aguardarFim(usuarioId);
  igual([fim.etapa, fim.habilitado], ['Concluido', true], 'etapa e habilitado no fim');
  igual((await usuariosNoKeycloak(username))[0].enabled, true, 'identidade habilitada no fim');
});

cenario('semantic lock: o usuário não pode ser excluído enquanto a saga roda (409)', async () => {
  await http('POST', `${URL_.cadastro}/simulations/failures`, { quantity: 2 }); // deixa a saga mais lenta
  const username = `davi-${id()}`;
  const usuarioId = await criarUsuario(username, `${username}@exemplo.com`, 'Davi Melo');
  const durante = await http('DELETE', `${URL_.cadastro}/usuarios/${usuarioId}`);
  igual(durante.status, 409, 'exclusão durante a saga');
  igual(durante.body.erro, 'provisionamento_em_andamento', 'erro da exclusão');
  await aguardarFim(usuarioId);
  igual((await http('DELETE', `${URL_.cadastro}/usuarios/${usuarioId}`)).status, 204, 'exclusão depois da saga');
});

cenario('o mesmo usuário não é provisionado duas vezes: o id do workflow é o id do usuário', async () => {
  igual((await consultar(primeiro.usuarioId)).etapa, 'Concluido', 'etapa consultada depois de terminado');
  igual((await usuariosNoKeycloak(primeiro.username)).length, 1, 'um usuário só no Keycloak');
  igual((await http('GET', `${URL_.cadastro}/usuarios/nao-existe`)).status, 404, 'usuário desconhecido');
  igual((await http('GET', `${URL_.cadastro}/cadastros/${primeiro.usuarioId}`)).status, 404, 'a rota /cadastros não existe');
});

console.log('Aguardando o serviço...');
await aguardar(async () => { try { return (await fetch(`${URL_.cadastro}/health`)).ok; } catch { return false; } }, 'cadastro-web pronto e banco migrado', 180000);
let falhas = 0;
for (const { nome, fn } of cenarios) {
  const inicio = Date.now();
  try { await fn(); console.log(`  ✔ ${nome} (${Date.now() - inicio} ms)`); }
  catch (e) { falhas++; console.log(`  ✘ ${nome}\n    ${e.message}`); }
}
console.log(falhas ? `\n${falhas} cenário(s) falharam` : '\nTodos os cenários passaram');
process.exit(falhas ? 1 : 0);
