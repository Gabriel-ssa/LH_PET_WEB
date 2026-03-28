using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using System.ComponentModel.Design;

namespace LH_PET_WEB.Controllers
{
    [Authorize]
    
    public class AtendimentoController : Controller
    {
        private readonly ContextoBanco _contexto;
        public AtendimentoController(ContextoBanco contexto)
        {
            _contexto = contexto;
        }
        public async Task<IActionResult> Index()
        {
            var atendimentos = await _contexto.Atendimentos
                .Include(a => a.Pet)
                .ThenInclude(p => p!.Cliente)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
            return View(atendimentos);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatus(int id, string novoStatus)
        {
            var agendamento = await _contexto.Agendamentos.FindAsync(id);
            if (agendamento != null) {
                agendamento.Status = novoStatus;
                _contexto.Agendamentos.Update(agendamento);
                await _contexto.SaveChangesAsync();
                TempData["Sucesso"] = $"Status atualizado para {novoStatus}";
            }
            else {
                TempData["Erro"] = "Agendamento não encontrado.";
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Prontuario(int agendamentoId)
        {
            var agendamentos = await _contexto.Agendamentos
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.Id == agendamentoId);
            if (agendamentos == null) return NotFound();
            
            var atendimentoExistente = await _contexto.Atendimentos
                .FirstOrDefaultAsync(a => a.AgendamentoId == agendamentoId);
            if (atendimentoExistente != null)
            {
                return View("VerProntuario", atendimentoExistente);
            }
            var novoAtendimento = new Atendimento
            {
                AgendamentoId = agendamentoId,
                Agendamento = agendamento
            };
            return View(novoAtendimento);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarProntuario(Atendimento model)
        {
            Model.State.Remove("Agendamento");
            if (!ModelState.IsValid)
            {
                _contexto.Atendimentos.Add(model);

                var agendamento = await _contexto.Agendamentos.FindAsync(model.AgendamentoId);
                 if (agendamento != null)
                 {
                     agendamento.Status = "Concluído";
                     _contexto.Agendamentos.Update(agendamento);
                 }
                await _contexto.SaveChangesAsync();
            TempData["Sucesso"] = "Prontuário salvo e atendimento concluído com sucesso!";
            return RedirectToAction(nameof(Index));
            }
            return View("Prontuario", model);
        }
        [HttpGet]
        public async Task<IActionResult> Historico(string pesquisa)
        {
            var query = _contexto.Atendimentos
                .Include(a => a.Agendamento)
                .ThenInclude(ag => ag!.Pet)
                .ThenInclude(p => p!.Cliente)
                .AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                pesquisa = pesquisa.ToLower();
                query = query.Where(a => a.Agendamento!.Pet!.Nome.ToLower().Contains(pesquisa) ||a.Agendamento.Pet.Cliente!.Nome.ToLower().Contains(pesquisa));
            
            }
            var historico = await query.OrderByDescending(a => a.Agendamento!.DataHora).ToListAsync();
            ViewBag.PesquisaAtual = pesquisa;
            
            return View(historico);
        }
        [HttpGet]
        public async Task<IActionResult> NovoAgendamento()
        {
        ViewBag.PetsLista = await _contexto.Pets.Include(p => p.Cliente).Select(p => new { p.Id, NomeExibicao = $"{p.Nome} (Tutor: {p.Cliente!.Nome})" }).ToListAsync();
        return View(new Agendamento { DataHora = DateTime.Now.AddHours(8) });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoAgendamento(Agendamento model)
        {
            ModelState.Remove("Pet");
            if (!ModelState.IsValid)
            {
                model.Status = "Pendente";
                var existeAgendamento = await _contexto.Agendamentos.AnyAsync(a => a.DataHora == model.DataHora && a.Status != "Cancelado");
                if (existeAgendamento)
                {
                    TempData["Erro"] = "Já existe um agendamento marcadopara esse horário exato.";
                    return RedirectToAction(nameof(NovoAgendamento));
                }
                _contexto.Agendamentos.Add(model);
                await _contexto.SaveChangesAsync();
                TempData["Sucesso"] = "Agendamento manual criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            TempData["Erro"] = "Preencha todos os campos corretamente.";
            return RedirectToAction(nameof(NovoAgendamento));
        }       
    }
}