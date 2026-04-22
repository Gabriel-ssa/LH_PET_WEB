using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models.ViewModels;



namespace LH_PET_WEB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RelatoriosController : Controller
    {
        private readonly ContextoBanco _contexto;

        public RelatoriosController(ContextoBanco contexto)
        {
            _contexto = contexto;
        }

        public async Task<IActionResult> Index()
        {
            var hoje = DateTime.Today;
            var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);

            var vendasMes = await _contexto.Vendas
                .Where(v => v.DataVenda >= inicioDoMes)
                .ToListAsync();

            var vendasHoje = vendasMes
                .Where(v => v.DataVenda.Date == hoje)
                .ToList();

            var faturamentoPagamento = vendasMes
                .GroupBy(v => v.FormaPagamento)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));
            
<<<<<<< HEAD
            var topProdutos = await _contexto.ItemVenda
=======
            var topProdutos = await _contexto.ItensVenda
>>>>>>> 2cfbdbae417f3303a31065a259960f3d4ea7ea98
                .Include(i => i.Produto)
                .Include(i => i.Venda)
                .Where(i => i.Venda!.DataVenda >= inicioDoMes)
                .GroupBy(i => new { i.ProdutoId, i.Produto!.Nome })
                .Select(g => new TopProdutoViewModel
                {
                    NomeProduto = g.Key.Nome,
                    QuantidadeVendida = g.Sum(i => i.Quantidade),
                    ValorTotalGerado = g.Sum(i => i.Quantidade * i.PrecoUnitario)
                })
                .OrderByDescending(p => p.QuantidadeVendida)
                .Take(5)
                .ToListAsync();

<<<<<<< HEAD
            var viewModel = new RelatorioDashboardViewModel
=======
            var viewModel = new RelatorioViewModel
>>>>>>> 2cfbdbae417f3303a31065a259960f3d4ea7ea98
            {
                FaturamentoHoje = vendasHoje.Sum(v => v.Total),
                FaturamentoMes = vendasMes.Sum(v => v.Total),
                QuantidadeVendasMes = vendasMes.Count,
                FaturamentoPorPagamento = faturamentoPagamento,
                ProdutosMaisVendidos = topProdutos
            };

            return View(viewModel);
        }
    }
}