using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;

namespace LH_PET_WEB.Controllers
{
    [Authorize]
    public class ConfiguracaoController : Controller
    {
        private readonly ContextoBanco contexto;

        public ConfiguracaoController(ContextoBanco contexto)
        {
            contexto = contexto;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var configuracao = await contexto.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);
            if (configuracao == null)
            {
                config = new ConfiguracaoClinica();
            }  

            return View(config); 
        }  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(ConfiguracaoClinica model, List<string> DiasSelecionados)
        {
            model.DiasTrabalho = string.Join(",", DiasSelecionados);
            ModelState.Remove("DiasTrabalho");

            if (!ModelState.IsValid)
            {
                var configExistente = await contexto.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);

                if (configExistente != null)
                {
                    configExistente.HoraAbertura = model.HoraAbertura;
                    configExistente.HoraFechamento = model.HoraFechamento;
                    configExistente.DiasTrabalho = model.DiasTrabalho;
                    configExistente.MinutosConsulta = model.MinutosConsulta;
                    configExistente.MinutosBanho = model.MinutosBanho;
                    configExistente.MinutosTosa = model.MinutosTosa;

                    contexto.Configuracoes.Update(configExistente);
                }
                else
                {
                    model.Id = 1; 
                    _contexto.Configuracoes.Add(model);
                }
                
                await contexto.SaveChangesAsync();
                TempData["Sucesso"] = "Configurações da agenda atualizadas com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            TempData["Erro"] = "Verifique os campos preenchidos.";
            return View("Index", model);
        }  
    }
}