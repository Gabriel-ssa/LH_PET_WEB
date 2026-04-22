using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using LH_PET_WEB.Services;

namespace LH_PET_WEB.Controllers
{
    [Authorize]
    public class VendasController : Controller
    {
        private readonly ContextoBanco _contexto;
        private readonly IEmailService _emailService;

        public VendasController(ContextoBanco contexto, IEmailService emailService)
        {
            _contexto = contexto;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> PDV()
        {
            var produtos = await _contexto.Produtos
                .Where(p => p.Estoque > 0)
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return View(produtos);
        }
        [HttpPost]
        public async Task<IActionResult> Finalizar([FromBody] CheckoutRequest request)
        {
            if (request == null || !request.Itens.Any()) return BadRequest("O carrinho está vazio.");
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdClaim)) return Unauthorized("Usuário não identificado.");
            int usuarioId = int.Parse(usuarioIdClaim);

            using var transaction = await _contexto.Database.BeginTransactionAsync();

            try
            {
                var novaVenda = new Venda
                {
                    DataVenda = DateTime.Now,
                    FormaPagamento = request.FormaPagamento,
<<<<<<< HEAD
                    UsuarioId = usuarioId,
=======
                    usuarioId = usuarioId,
>>>>>>> 2cfbdbae417f3303a31065a259960f3d4ea7ea98
                    Total = 0    
                };

                _contexto.Vendas.Add(novaVenda);
                await _contexto.SaveChangesAsync();

                decimal totalVenda = 0;

                foreach (var item in request.Itens)
                {
                    var produtoBanco = await _contexto.Produtos.FindAsync(item.ProdutoId);

                    if (produtoBanco == null) throw new Exception($"Produto com ID {item.ProdutoId} não encontrado.");
                    if (produtoBanco.Estoque < item.Quantidade) throw new Exception($"Estoque insuficiente para  {produtoBanco.Nome}. Restam apenas {produtoBanco.Estoque}");

                    produtoBanco.Estoque -= item.Quantidade;
                    _contexto.Produtos.Update(produtoBanco);

                    var novoItem = new ItemVenda
                    {
                        VendaId = novaVenda.Id,
                        ProdutoId = item.ProdutoId,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = produtoBanco.Preco
                    };

                    totalVenda += (item.Quantidade * produtoBanco.Preco);
<<<<<<< HEAD
                    _contexto.ItemVenda.Add(novoItem);
=======
                    _contexto.ItensVenda.Add(novoItem);
>>>>>>> 2cfbdbae417f3303a31065a259960f3d4ea7ea98
                }
                novaVenda.Total = totalVenda;
                _contexto.Vendas.Update(novaVenda);

                await _contexto.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Mensagem = "Venda finalizada com sucesso!", VendaId = novaVenda.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { Mensagem = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Historico()
        {
            var vendas = await _contexto.Vendas
                .Include(v => v.Usuario)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
            
            
            return View(vendas);
        }
        [HttpGet]
        public async Task<IActionResult> Recibos(int id)
        {
            var venda = await _contexto.Vendas
                .Include(v => v.Usuario)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null) return NotFound();

            return View(venda);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarPorEmail(int id, string emailCliente)
        {
            var venda = await _contexto.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null) return NotFound();

            if(string.IsNullOrEmpty(emailCliente))
            {
                TempData["Erro"] = "Digite um e-mail válido.";
                return RedirectToAction(nameof(Recibos), new { id = venda.Id });
            }

            string itensHtml ="";
            foreach(var item in venda.Itens)
            {
                itensHtml += $"<li>{item.Quantidade} x {item.Produto!.Nome} - R${item.PrecoUnitario:N2} </li>";
            }

            string corpoEmail = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
                    <h2 style='color: #0d6efd; text-align: center;'>VetPlus Care - Recibo Eletrônico</h2>
                    <p>Olá! Agradecemos a sua preferência. Abaixo estão os detalhes da sua compra:</p>
                    <p><strong>Recino Nº:</strong> {venda.Id.ToString("D6")}</p>
                    <p><strong>Data da Compra:</strong> {venda.DataVenda.ToString("dd/MM/yyyy HH:mm")}</p>
                    <p><strong>Pagamento:</strong> {venda.FormaPagamento}</p>
                    <hr style='border: 1px solid #eee;' />
                    <ul style='list-style: none; padding: 0;'>{itensHtml}</ul>
                    <hr style='border: 1px solid #eee;' />
                    <h3 style='text-align: right; color: #198754;'>Total: R${venda.Total:N2}</h3>
                    <p style='text-align: center; color: #888; font-size: 12px; margin-top: 30px;'>VetPlus Care System - Documento auxiliar sem valor fiscal.</p>
                </div>
            ";
            bool enviado = await _emailService.EnviarEmailAsync(emailCliente, $"Recibo de Compra #{venda.Id.ToString("D6")} - VetPlus Care", corpoEmail);

            if(enviado) TempData["Sucesso"] = "Recibo enviado por e-mail com sucesso!";
            else TempData["Erro"] = "Falha ao enviar o e-mail. Verifique a conexão.";

            return RedirectToAction(nameof(Recibos), new { id = venda.Id });
        }
    }
    public class CheckoutRequest
    {
        public string FormaPagamento { get; set; } = string.Empty;
        public List<CarrinhoItem> Itens { get; set; } = new List<CarrinhoItem>();
    }

    public class CarrinhoItem
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}