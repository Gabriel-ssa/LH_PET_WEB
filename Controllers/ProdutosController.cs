using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using System.IO;
using Microsoft.Build.Experimental.ProjectCache;

namespace LH_PET_WEB.Controllers
{
 [Authorize]
 public class ProdutosController : Controller
    {
        private readonly ContextoBanco _contexto;
        private readonly IWebHostEnvironment HostEnvironment;

        public ProdutosController(ContextoBanco contexto, IWebHostEnvironment hostEnvironment)
        {
            _contexto = contexto;
            HostEnvironment = hostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var produtos = await _contexto.Produtos.OrderBy(p => p.Nome).ToListAsync();
            return View(produtos);
        }
        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Produto model, IFormFile? foto)
        {
           if (!ModelState.IsValid)
            {
           
            var produtoExiste = await _contexto.Produtos.AnyAsync(p => p.Nome.ToLower() == model.Nome.ToLower());
            if (produtoExiste)
            {
                ModelState.AddModelError("Nome", "Já existe um produto cadastrado com esse nome no estoque.");
                return View(model);
            }

            if (foto != null && foto.Length > 0)
            {
                string pastaDestino = Path.Combine(HostEnvironment.WebRootPath, "uploads", "produtos");
                Directory.CreateDirectory(pastaDestino);

                string nomeArquivoUnico = Guid.NewGuid().ToString() + " " + foto.FileName;
                string caminhoCompleto = Path.Combine(pastaDestino, nomeArquivoUnico);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                model.ImagemUrl = "/uploads/produtos/" + nomeArquivoUnico;
            }
            _contexto.Produtos.Add(model);
            await _contexto.SaveChangesAsync();
            TempData["Sucesso"] = "Produto cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var produto = await _contexto.Produtos.FindAsync(id);
            if (produto == null) return NotFound();
            return View(produto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Produto model, IFormFile? foto)
        {
           if(id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    var conflitoNome = await _contexto.Produtos.AnyAsync(p => p.Nome.ToLower() == model.Nome.ToLower() && p.Id != id);
                    if (conflitoNome)  {
                        ModelState.AddModelError("Nome", "Outro produto já usa este nome.");
                        return View(model);
                    }
                    if (foto != null && foto.Length > 0)
                    {
                        string pastaDestino = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "produtos");
                        Directory.CreateDirectory(pastaDestino);

                        string nomeArquivoUnico = Guid.NewGuid().ToString() + " " + foto.FileName;
                        string caminhoCompleto = Path.Combine(pastaDestino, nomeArquivoUnico);

                        using(var stream = new FileStream(caminhoCompleto, FileMode.Create))
                        {
                            await foto.CopyToAsync(stream);
                        }

                        if (!string.IsNullOrEmpty(model.ImagemUrl))
                        {
                            string caminhoAntigo = Path.Combine(_hostEnvironment.WebRootPath, model.ImagemUrl.TrimStart('/'));
                            if (System.IO.File.Exists(caminhoAntigo)) System.IO.File.Delete(caminhoAntigo);
                        }
                        model.ImagemUrl = "/uploads/produtos/" + nomeArquivoUnico;
                    }
                    else
                    {
                        var produtoBanco = await _contexto.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (produtoBanco != null) model.ImagemUrl = produtoBanco.ImagemUrl;
                    }

                    contexto.Produtos.Update(model);
                    await contexto.SaveChangesAsync();
                    TempData["Sucesso"] = "Produto atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdutoExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
         }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            var produto = await _contexto.Produtos.FindAsync(id);
            if (produto != null)
            {
            if (!string.IsNullOrEmpty(produto.ImagemUrl))
            {
                string caminhoImagem = Path.Combine(_hostEnvironment.WebRootPath, produto.ImagemUrl.TrimStart('/'));
                if (System.IO.File.Exists(caminhoImagem)) System.IO.File.Delete(caminhoImagem);
            }
            _contexto.Produtos.Remove(produto);
            await _contexto.SaveChangesAsync();
            TempData["Sucesso"] = "Produto removido do estoque.";
            }
            return RedirectToAction(nameof(Index));
        }
        private bool ProdutoExists(int id) => _contexto.Produtos.Any(e => e.Id == id);
        
    }
}