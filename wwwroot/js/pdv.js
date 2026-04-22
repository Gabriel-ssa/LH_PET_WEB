let carrinho = [];

function adicionarAoCarrinho(id, nome, preco, estoqueMaximo) {
    let itemExistente = carrinho.find(i => i.id === id);
    
    if (itemExistente) {
        if (itemExistente.quantidade < estoqueMaximo) {
            itemExistente.quantidade++;
        } else {
            alert("Atenção: Estoque insuficiente para adicionar mais deste item!");
        }
    } else {
        carrinho.push({ id, nome, preco, quantidade: 1 });
    }
    atualizarTelaCarrinho();
}

function alterarQuantidade(id, delta) {
    let item = carrinho.find(i => i.id === id);
    if (!item) return;

    item.quantidade += delta;
    
    if (item.quantidade <= 0) {
        carrinho = carrinho.filter(i => i.id !== id);
    }
    atualizarTelaCarrinho();
}

function removerItem(id) {
    carrinho = carrinho.filter(i => i.id !== id);
    atualizarTelaCarrinho();
}

function atualizarTelaCarrinho() {
    const lista = document.getElementById('listaCarrinho');
    const totalEl = document.getElementById('valorTotal');
    const btnFinalizar = document.getElementById('btnFinalizar');
    
    lista.innerHTML = '';
    let total = 0;

    if (carrinho.length === 0) {
        lista.innerHTML = '<tr><td class="text-center text-muted py-5">O carrinho está vazio.</td></tr>';
        totalEl.innerText = 'R$ 0,00';
        btnFinalizar.disabled = true;
        return;
    }

    carrinho.forEach(item => {
        let subtotal = item.preco * item.quantidade;
        total += subtotal;

        lista.innerHTML += `
            <tr>
                <td class="px-3">
                    <div class="fw-bold text-truncate" style="max-width: 150px;">${item.nome}</div>
                    <small class="text-success fw-bold">R$ ${item.preco.toFixed(2).replace('.', ',')}</small>
                </td>
                <td class="text-center">
                    <div class="input-group input-group-sm" style="width: 90px;">
                        <button class="btn btn-outline-secondary" onclick="alterarQuantidade(${item.id}, -1)">-</button>
                        <input type="text" class="form-control text-center fw-bold px-1" value="${item.quantidade}" readonly>
                        <button class="btn btn-outline-secondary" onclick="alterarQuantidade(${item.id}, 1)">+</button>
                    </div>
                </td>
                <td class="text-end fw-bold text-primary px-3">
                    R$ ${subtotal.toFixed(2).replace('.', ',')}
                </td>
                <td>
                    <button class="btn btn-sm text-danger border-0" onclick="removerItem(${item.id})"><i class="fa-solid fa-trash"></i></button>
                </td>
            </tr>
        `;
    });

    totalEl.innerText = `R$ ${total.toFixed(2).replace('.', ',')}`;
    btnFinalizar.disabled = false;
}

async function finalizarVenda() {
    if (carrinho.length === 0) return;

    const btnFinalizar = document.getElementById('btnFinalizar');
    btnFinalizar.disabled = true;
    btnFinalizar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2"></i> Processando...';

    const payload = {
        formaPagamento: document.getElementById('formaPagamento').value,
        itens: carrinho.map(i => ({ produtoId: i.id, quantidade: i.quantidade }))
    };

    try {
        const response = await fetch('/Vendas/Finalizar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (response.ok) {
            alert(data.mensagem);
            window.location.reload(); 
        } else {
            alert("Erro: " + data.mensagem);
            btnFinalizar.disabled = false;
            btnFinalizar.innerHTML = '<i class="fa-solid fa-check me-2"></i> Finalizar Venda';
        }
    } catch (erro) {
        alert("Erro de comunicação com o servidor.");
        btnFinalizar.disabled = false;
        btnFinalizar.innerHTML = '<i class="fa-solid fa-check me-2"></i> Finalizar Venda';
    }
}