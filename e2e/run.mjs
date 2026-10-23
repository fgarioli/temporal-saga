// Testes ponta a ponta do cadastro com saga no Temporal. Node 18+, sem dependências.
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
  return { status: res.status, body: json };
}

function assert(cond, msg) { if (!cond) throw new Error(msg); }
function igual(a, b, msg) { assert(JSON.stringify(a) === JSON.stringify(b), `${msg}\n  esperado: ${JSON.stringify(b)}\n  obtido:   ${JSON.stringify(a)}`); }

async function aguardar(fn, descricao, timeoutMs = 30000) {
  const fim = Date.now() + timeoutMs;
  while (Date.now() < fim) { const r = await fn(); if (r) return r; await sleep(300); }
  throw new Error(`tempo esgotado esperando: ${descricao}`);
}

async function cadastrar(username, email, nome = 'Ana Souza') {
  const r = await http('POST', `${URL_.cadastro}/cadastros`, { username, email, nome });
  igual(r.status, 202, 'POST /cadastros deveria responder 202');
  return r.body.cadastroId;
}

const aguardarFim = (cadastroId) => aguardar(async () => {
  const r = await http('GET', `${URL_.cadastro}/cadastros/${cadastroId}`);
  return ['Concluido', 'Cancelado'].includes(r.body?.estado) ? r.body : null;
}, `fim do cadastro ${cadastroId}`);

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

cenario('caminho feliz: os 4 passos até Concluido, com o usuário no Keycloak e o e-mail no Mailpit', async () => {
  const username = `ana-${id()}`; const email = `${username}@exemplo.com`;
  const cadastroId = await cadastrar(username, email);
  const fim = await aguardarFim(cadastroId);
  igual(fim.historico, ['CriandoUsuario', 'CriandoIdentidade', 'AtivandoUsuario', 'EnviandoBoasVindas', 'Concluido'], 'histórico');
  const usuario = (await http('GET', `${URL_.cadastro}/usuarios/${cadastroId}`)).body;
  igual(usuario.status, 'Ativo', 'status do usuário');
  const noKeycloak = await usuariosNoKeycloak(username);
  igual(noKeycloak.length, 1, 'usuário no Keycloak');
  igual(noKeycloak[0].id, usuario.keycloakId, 'keycloakId gravado no usuário');
  await aguardar(async () => {
    const r = await http('GET', `${URL_.mailpit}/api/v1/messages`);
    return r.body.messages?.some((m) => m.To?.some((t) => t.Address === email));
  }, 'e-mail de boas-vindas');
  primeiro = { cadastroId, username, keycloakId: usuario.keycloakId };
});

cenario('perfil padrão: o Keycloak dá o papel "cliente" a todo usuário novo, sem passo na saga', async () => {
  const papeis = (await adminKeycloak('GET', `/users/${primeiro.keycloakId}/role-mappings/realm/composite`)).body;
  assert(papeis.some((p) => p.name === 'cliente'), `papéis: ${JSON.stringify(papeis.map((p) => p.name))}`);
});

cenario('Keycloak recusa (username já existe lá): compensa rejeitando o usuário', async () => {
  const username = `bia-${id()}`;
  const criado = await adminKeycloak('POST', '/users', { username, email: `${username}@outro-sistema.com`, firstName: 'Bia', lastName: 'Outro', enabled: true });
  igual(criado.status, 201, 'usuário criado direto no Keycloak, por outro sistema');
  const cadastroId = await cadastrar(username, `${username}@exemplo.com`, 'Bia Lima');
  const fim = await aguardarFim(cadastroId);
  igual(fim.historico, ['CriandoUsuario', 'CriandoIdentidade', 'RejeitandoUsuario', 'Cancelado'], 'histórico');
  assert(fim.motivo?.includes('Keycloak'), `motivo: ${fim.motivo}`);
  igual((await http('GET', `${URL_.cadastro}/usuarios/${cadastroId}`)).body.status, 'Rejeitado', 'usuário rejeitado');
});

cenario('username já usado no sistema: T1 recusa e a saga termina sem nada para compensar', async () => {
  const cadastroId = await cadastrar(primeiro.username, `repetido-${id()}@exemplo.com`);
  const fim = await aguardarFim(cadastroId);
  igual(fim.historico, ['CriandoUsuario', 'Cancelado'], 'histórico');
});

cenario('falha temporária do Keycloak: resolvida com nova tentativa, sem compensar', async () => {
  igual((await http('POST', `${URL_.cadastro}/simulations/failures`, { quantity: 2 })).status, 204, 'simulação');
  const username = `caio-${id()}`;
  const cadastroId = await cadastrar(username, `${username}@exemplo.com`, 'Caio Reis');
  igual((await aguardarFim(cadastroId)).estado, 'Concluido', 'estado');
  igual((await usuariosNoKeycloak(username)).length, 1, 'um usuário só no Keycloak');
});

cenario('semantic lock: o usuário pendente não pode ser excluído (409)', async () => {
  await http('POST', `${URL_.cadastro}/simulations/failures`, { quantity: 2 }); // deixa o cadastro mais lento
  const username = `davi-${id()}`;
  const cadastroId = await cadastrar(username, `${username}@exemplo.com`, 'Davi Melo');
  await aguardar(async () => (await http('GET', `${URL_.cadastro}/usuarios/${cadastroId}`)).status === 200, 'usuário pendente');
  igual((await http('DELETE', `${URL_.cadastro}/usuarios/${cadastroId}`)).status, 409, 'exclusão durante a saga');
  await aguardarFim(cadastroId);
  igual((await http('DELETE', `${URL_.cadastro}/usuarios/${cadastroId}`)).status, 204, 'exclusão depois da saga');
});

cenario('o mesmo cadastro não roda duas vezes: o id do workflow é o id do cadastro', async () => {
  igual((await http('GET', `${URL_.cadastro}/cadastros/${primeiro.cadastroId}`)).body.estado, 'Concluido', 'estado consultado depois de terminado');
  igual((await usuariosNoKeycloak(primeiro.username)).length, 1, 'um usuário só no Keycloak');
  igual((await http('GET', `${URL_.cadastro}/cadastros/nao-existe`)).status, 404, 'cadastro desconhecido');
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
