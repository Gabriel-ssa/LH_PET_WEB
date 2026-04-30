const URL_BASE = '/api';

const racasPorEspecie = {
    "Cachorro": ["SRD (Vira-lata)", "Shih Tzu", "Poodle", "Bulldog Francês", "Golden Retriever", "Labrador", "Pinscher", "Spitz Alemão", "Pug", "Outro"],
    "Gato": ["SRD (Sem Raça Definida)", "Persa", "Siamês", "Maine Coon", "Angorá", "Sphynx", "Bengal", "Outro"],
    "Ave": ["Calopsita", "Papagaio", "Periquito", "Canário", "Cacatua", "Outro"],
    "Roedor": ["Hamster", "Porquinho da Índia", "Coelho", "Chinchila", "Outro"],
    "Exótico": ["Ferret", "Iguana", "Jabuti", "Mini Pig", "Outro"]
};

// Variável para guardar os pets carregados e facilitar a edição
let listaPetsGlobal = [];

function mascaraCpf(input) {
    let v = input.value;
    v = v.replace(/\D/g, "");
    v = v.replace(/(\d{3})(\d)/, "$1.$2");
    v = v.replace(/(\d{3})(\d)/, "$1.$2");
    v = v.replace(/(\d{3})(\d{1,2})$/, "$1-$2");
    input.value = v;
}

function mascaraTelefone(input) {
    let v = input.value;
    v = v.replace(/\D/g, "");
    v = v.replace(/^(\d{2})(\d)/g, "($1) $2");
    v = v.replace(/(\d)(\d{4})$/, "$1-$2");
    input.value = v;
}

function avaliarForcaSenha(senha) {
    let forca = 0;
    const barra = document.getElementById('barraForca');
    const texto = document.getElementById('textoForca');

    if (senha.length >= 8) forca += 1;
    if (/[A-Z]/.test(senha)) forca += 1;
    if (/[a-z]/.test(senha)) forca += 1;
    if (/\d/.test(senha)) forca += 1;
    if (/[\W_]/.test(senha)) forca += 1;

    barra.className = "progress-bar transition-all";
    if (senha.length === 0) {
        barra.style.width = "0%";
        texto.innerText = "";
    } else if (forca <= 2) {
        barra.style.width = "33%";
        barra.classList.add("bg-danger");
        texto.innerText = "Fraca";
        texto.className = "small fw-bold text-danger";
    } else if (forca >= 3 && forca < 5) {
        barra.style.width = "66%";
        barra.classList.add("bg-warning");
        texto.innerText = "Média";
        texto.className = "small fw-bold text-warning";
    } else if (forca === 5) {
        barra.style.width = "100%";
        barra.classList.add("bg-success");
        texto.innerText = "Forte";
        texto.className = "small fw-bold text-success";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    verificarAutenticacao();
    const hoje = new Date().toISOString().split('T')[0];
    const inputData = document.getElementById('agendaData');
    if(inputData) inputData.setAttribute('min', hoje);

    const selectTipo = document.getElementById('agendaTipo');
    if(selectTipo) selectTipo.addEventListener('change', buscarHorariosLivres);
});

function toggleForms(e) {
    e.preventDefault();
    document.getElementById('formLogin').parentElement.classList.toggle('hidden');
    document.getElementById('cardRegistro').classList.toggle('hidden');
}

function mostrarMensagem(elementoId, texto, tipo) {
    const el = document.getElementById(elementoId);
    el.className = `alert alert-${tipo}`;
    el.innerText = texto;
    el.classList.remove('hidden');
    setTimeout(() => el.classList.add('hidden'), 4000);
}

// Atualizada para funcionar tanto no formulário de Criação quanto no de Edição
function atualizarRacas(idEspecie, idRaca) {
    const especieSelecionada = document.getElementById(idEspecie).value;
    const selectRaca = document.getElementById(idRaca);
    
    selectRaca.innerHTML = '<option value="">Selecione...</option>';
    
    if (especieSelecionada && racasPorEspecie[especieSelecionada]) {
        racasPorEspecie[especieSelecionada].forEach(raca => {
            selectRaca.innerHTML += `<option value="${raca}">${raca}</option>`;
        });
    }
}

function getHeadersAuth() {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
    };
}

// ==========================================
// AUTENTICAÇÃO E REGISTRO
// ==========================================
async function fazerLogin(e) {
    e.preventDefault();
    const dados = {
        email: document.getElementById('loginEmail').value,
        senha: document.getElementById('loginSenha').value
    };

    try {
        const response = await fetch(`${URL_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dados)
        });
        const resData = await response.json();
        if (response.ok) {
            localStorage.setItem('jwtToken', resData.token);
            verificarAutenticacao();
        } else {
            mostrarMensagem('msgAuth', resData.mensagem || "Erro ao fazer login", 'danger');
        }
    } catch (erro) {
        mostrarMensagem('msgAuth', "Erro de conexão.", 'danger');
    }
}

async function fazerRegistro(e) {
    e.preventDefault();
    const senha = document.getElementById('regSenha').value;
    const regexSenhaSegura = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;
    if (!regexSenhaSegura.test(senha)) {
        alert("A senha precisa estar 'Forte' para prosseguir.");
        return; 
    }

    const dados = {
        nome: document.getElementById('regNome').value,
        cpf: document.getElementById('regCpf').value,
        telefone: document.getElementById('regTelefone').value,
        email: document.getElementById('regEmail').value,
        senha: senha
    };

    try {
        const response = await fetch(`${URL_BASE}/auth/registrar`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dados)
        });
        const isJson = response.headers.get("content-type")?.includes("application/json");

        if (response.ok) {
            alert("Conta criada com sucesso! Faça login para continuar.");
            toggleForms(new Event('click'));
        } else {
            if (isJson) {
                const resData = await response.json();
                if(resData.errors) {
                    let errs = "";
                    for (let campo in resData.errors) errs += resData.errors[campo][0] + "\n";
                    alert("Atenção:\n" + errs);
                } else {
                    alert(resData.mensagem || "Erro ao registrar.");
                }
            } else {
                alert("Erro interno (500).");
            }
        }
    } catch (erro) {
        alert("Erro de conexão.");
    }
}

function fazerLogout() {
    localStorage.removeItem('jwtToken');
    verificarAutenticacao();
}

function verificarAutenticacao() {
    const token = localStorage.getItem('jwtToken');
    if (token) {
        document.getElementById('areaAuth').classList.add('hidden');
        document.getElementById('areaLogada').classList.remove('hidden');
        document.getElementById('btnSair').classList.remove('hidden');
        carregarDashboard();
    } else {
        document.getElementById('areaAuth').classList.remove('hidden');
        document.getElementById('areaLogada').classList.add('hidden');
        document.getElementById('btnSair').classList.add('hidden');
    }
}

// ==========================================
// GESTÃO DE PERFIL (CRUD)
// ==========================================
async function carregarDashboard() {
    await carregarPerfil();
    await carregarPets();
    await carregarAgendamentos();
}

async function carregarPerfil() {
    const response = await fetch(`${URL_BASE}/cliente/perfil`, { headers: getHeadersAuth() });
    if (response.status === 401) { fazerLogout(); return; }
    const dados = await response.json();
    document.getElementById('lblNomeCliente').innerText = dados.nome;
    
    // Deixa os dados pré-carregados ocultos para o Modal
    document.getElementById('editNome').value = dados.nome;
    document.getElementById('editTelefone').value = dados.telefone;
    document.getElementById('editEmail').value = dados.email;
}

function abrirModalPerfil() {
    document.getElementById('msgModalPerfil').classList.add('hidden');
    new bootstrap.Modal(document.getElementById('modalPerfil')).show();
}

async function salvarPerfil(e) {
    e.preventDefault();
    const dados = {
        nome: document.getElementById('editNome').value,
        telefone: document.getElementById('editTelefone').value,
        email: document.getElementById('editEmail').value
    };

    const response = await fetch(`${URL_BASE}/cliente/perfil`, {
        method: 'PUT',
        headers: getHeadersAuth(),
        body: JSON.stringify(dados)
    });

    if (response.ok) {
        alert("Perfil atualizado com sucesso!");
        bootstrap.Modal.getInstance(document.getElementById('modalPerfil')).hide();
        carregarPerfil(); // Recarrega o nome na tela
    } else {
        const resData = await response.json();
        mostrarMensagem('msgModalPerfil', resData.mensagem || "Erro ao atualizar perfil.", 'danger');
    }
}

// ==========================================
// GESTÃO DE PETS (CRUD COMPLETO)
// ==========================================
async function carregarPets() {
    const response = await fetch(`${URL_BASE}/cliente/pets`, { headers: getHeadersAuth() });
    listaPetsGlobal = await response.json(); // Salva globalmente
    
    const lista = document.getElementById('listaPets');
    const selectAgenda = document.getElementById('agendaPetId');
    
    lista.innerHTML = '';
    selectAgenda.innerHTML = '<option value="">Escolha seu Pet...</option>';

    if (listaPetsGlobal.length === 0) {
        lista.innerHTML = '<li class="list-group-item text-muted text-center py-3">Nenhum pet cadastrado.</li>';
        return;
    }

    listaPetsGlobal.forEach(pet => {
        // Agora exibe a Idade que o C# calculou dinamicamente
        lista.innerHTML += `
            <li class="list-group-item d-flex justify-content-between align-items-center flex-wrap">
                <div>
                    <i class="fa-solid fa-bone text-secondary me-2"></i> 
                    <strong>${pet.nome}</strong> <small class="text-muted">(${pet.raca})</small>
                    <span class="badge bg-light text-dark border ms-2">${pet.idade} anos</span>
                </div>
                <div>
                    <button class="btn btn-outline-info btn-acao-pet rounded-circle me-1" onclick="abrirModalPet(${pet.id})" title="Editar"><i class="fa-solid fa-pen"></i></button>
                    <button class="btn btn-outline-danger btn-acao-pet rounded-circle" onclick="excluirPet(${pet.id})" title="Excluir"><i class="fa-solid fa-trash"></i></button>
                </div>
            </li>`;
        
        selectAgenda.innerHTML += `<option value="${pet.id}">${pet.nome}</option>`;
    });
}

async function adicionarPet(e) {
    e.preventDefault();
    const dados = {
        nome: document.getElementById('petNome').value,
        dataNascimento: document.getElementById('petNascimento').value, // <-- ENVIANDO DATA
        especie: document.getElementById('petEspecie').value,
        raca: document.getElementById('petRaca').value
    };

    const response = await fetch(`${URL_BASE}/cliente/pets`, {
        method: 'POST',
        headers: getHeadersAuth(),
        body: JSON.stringify(dados)
    });

    if (response.ok) {
        document.getElementById('formPet').reset();
        document.getElementById('petRaca').innerHTML = '<option value="">Selecione a Espécie primeiro...</option>'; 
        carregarPets();
    } else {
        alert("Erro ao cadastrar pet.");
    }
}

function abrirModalPet(id) {
    document.getElementById('msgModalPet').classList.add('hidden');
    const pet = listaPetsGlobal.find(p => p.id === id);
    if (!pet) return;

    document.getElementById('editPetId').value = pet.id;
    document.getElementById('editPetNome').value = pet.nome;
    document.getElementById('editPetNascimento').value = pet.dataNascimento;
    
    // Atualiza os dropdowns em cascata
    document.getElementById('editPetEspecie').value = pet.especie;
    atualizarRacas('editPetEspecie', 'editPetRaca'); 
    document.getElementById('editPetRaca').value = pet.raca;

    new bootstrap.Modal(document.getElementById('modalPet')).show();
}

async function salvarEdicaoPet(e) {
    e.preventDefault();
    const id = document.getElementById('editPetId').value;
    const dados = {
        nome: document.getElementById('editPetNome').value,
        dataNascimento: document.getElementById('editPetNascimento').value,
        especie: document.getElementById('editPetEspecie').value,
        raca: document.getElementById('editPetRaca').value
    };

    const response = await fetch(`${URL_BASE}/cliente/pets/${id}`, {
        method: 'PUT',
        headers: getHeadersAuth(),
        body: JSON.stringify(dados)
    });

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('modalPet')).hide();
        carregarPets();
        carregarAgendamentos(); // Atualiza a tabela caso o nome do pet mude
    } else {
        mostrarMensagem('msgModalPet', "Erro ao editar pet.", 'danger');
    }
}

async function excluirPet(id) {
    if (!confirm("Atenção! Deseja realmente excluir este pet? Isso não poderá ser desfeito.")) return;

    const response = await fetch(`${URL_BASE}/cliente/pets/${id}`, {
        method: 'DELETE',
        headers: getHeadersAuth()
    });

    if (response.ok) {
        carregarPets();
    } else {
        const resData = await response.json();
        alert(resData.mensagem || "Erro ao excluir o pet.");
    }
}

// ==========================================
// GESTÃO DE AGENDAMENTOS (INTACTO)
// ==========================================
async function buscarHorariosLivres() {
    const dataEscolhida = document.getElementById('agendaData').value;
    const tipoServico = document.getElementById('agendaTipo').value; 
    const selectHora = document.getElementById('agendaHora');
    const btnAgendar = document.getElementById('btnAgendar');

    if (!dataEscolhida || !tipoServico) {
        selectHora.innerHTML = '<option value="">Escolha Serviço e Data...</option>';
        selectHora.disabled = true; btnAgendar.disabled = true; return;
    }

    selectHora.innerHTML = '<option value="">Calculando horários...</option>';
    selectHora.disabled = true;

    try {
        const response = await fetch(`${URL_BASE}/cliente/horarios-disponiveis?data=${dataEscolhida}&tipo=${tipoServico}`, { headers: getHeadersAuth() });
        if (response.ok) {
            const horarios = await response.json();
            selectHora.innerHTML = '<option value="">Selecione um horário...</option>';
            if (horarios.length === 0) {
                selectHora.innerHTML = '<option value="">Fechado ou Lotado</option>'; btnAgendar.disabled = true;
            } else {
                horarios.forEach(h => selectHora.innerHTML += `<option value="${h}">${h}</option>`);
                selectHora.disabled = false; btnAgendar.disabled = false;
            }
        }
    } catch (e) { selectHora.innerHTML = '<option value="">Erro</option>'; }
}

async function carregarAgendamentos() {
    const response = await fetch(`${URL_BASE}/cliente/agendamentos`, { headers: getHeadersAuth() });
    const agendamentos = await response.json();
    const tabela = document.getElementById('listaAgendamentos');
    tabela.innerHTML = '';
    if (agendamentos.length === 0) {
        tabela.innerHTML = '<tr><td colspan="4" class="text-muted py-3">Nenhum agendamento encontrado.</td></tr>'; return;
    }
    agendamentos.forEach(ag => {
        const dataFormatada = new Date(ag.dataHora).toLocaleString('pt-BR');
        let badgeStatus = ag.status === 'Concluído' ? 'bg-success' : (ag.status === 'Cancelado' ? 'bg-danger' : 'bg-warning text-dark');
        tabela.innerHTML += `<tr><td class="align-middle">${dataFormatada}</td><td class="align-middle fw-bold">${ag.petNome}</td><td class="align-middle">${ag.tipo}</td><td class="align-middle"><span class="badge ${badgeStatus}">${ag.status}</span></td></tr>`;
    });
}

async function criarAgendamento(e) {
    e.preventDefault();
    const dataHoraCompleta = `${document.getElementById('agendaData').value}T${document.getElementById('agendaHora').value}:00`;
    const dados = { petId: parseInt(document.getElementById('agendaPetId').value), tipo: document.getElementById('agendaTipo').value, dataHora: dataHoraCompleta };
    const response = await fetch(`${URL_BASE}/cliente/agendamentos`, { method: 'POST', headers: getHeadersAuth(), body: JSON.stringify(dados) });
    if (response.ok) {
        mostrarMensagem('msgAgenda', 'Agendado com sucesso!', 'success');
        document.getElementById('formAgenda').reset();
        document.getElementById('agendaHora').disabled = true; document.getElementById('btnAgendar').disabled = true;
        carregarAgendamentos();
    } else {
        const res = await response.json(); mostrarMensagem('msgAgenda', res.mensagem || 'Erro.', 'danger');
    }
}