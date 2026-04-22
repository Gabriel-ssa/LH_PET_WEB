using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using LH_PET_WEB.Models.ViewModels;
using LH_PET_WEB.Services;

namespace LH_PET_WEB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly ContextoBanco _contexto;
        private readonly IEmailService _emailService;

        public UsuariosController(ContextoBanco contexto, IEmailService emailService)
        {
            _contexto = contexto;
            _emailService = emailService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _contexto.Usuarios
            .Where(u => u.Perfil != "Cliente")
            .ToListAsync();
            return View(usuarios);
        }
        [HttpGet]
        public IActionResult Criar()
        {
            return View(new UsuarioCreateViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(UsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var existe = await _contexto.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
                return View(model);
            }

            string senhaGerada = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

            var novoUsuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                Perfil = model.Perfil,
                Ativo = true,
                SenhaTemporaria = true,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaGerada)
            };

            _contexto.Usuarios.Add(novoUsuario);
            await _contexto.SaveChangesAsync();

            string mensagem = $"Olá {model.Nome}, bem-vindo(a) à VetPlus Care!\n\nSeu acesso como {model.Perfil} foi criado.\nSeu E-mail: {model.Email}\nSua Senha Inicial: {senhaGerada}\n\nPor favor, faça login no sistema para cadastrar sua senha definitiva.";

            bool emailEnviado = await _emailService.EnviarEmailAsync(model.Email, "Sua conta foi criada - VetPlus Care", mensagem);
            if (!emailEnviado)
            {
                TempData["Sucesso"] = "Colaborador criado com sucesso. A senha foi enviada para o e-mail informado.";
            }
            else
            {
                TempData["Sucesso"] = $"Colaborador criado no banco de dados, mas o e-mail falhou. Copie a senha provisória e envie manualmente: {senhaGerada}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(int id)
        {
           var usuario = await _contexto.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var emailLogado = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (usuario.Email == emailLogado)
            {
                TempData["Erro"] = "Você não pode desativar sua própria conta.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Ativo = !usuario.Ativo;
            await _contexto.SaveChangesAsync();

            TempData["Sucesso"] = $"Status de {usuario.Nome} alterado com sucesso.";
            return RedirectToAction(nameof(Index));

        }
    }
}