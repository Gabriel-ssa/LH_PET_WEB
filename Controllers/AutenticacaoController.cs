using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models.ViewModels;
using LH_PET_WEB.Services;
using Microsoft.EntityFrameworkCore;

namespace LH_PET_WEB.Controllers
{
    public class AutenticacaoController : Controller
    {
        private readonly ContextoBanco contexto;
        private readonly IEmailService emailService;

        public AutenticacaoController(ContextoBanco contexto, IProblemDetailsService emailService)
        {
            contexto = contexto;
            emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity is {IsAuthenticated: true}) return RedirectToAction("Index","Painel");
            Return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.SenhaHash))
            {
                TempData["Erro"] = "Email ou senha inválidos.";
                return View(model);
            }
            
            if(!usuario.Ativo)
            {
                TempData["Erro"] = "Seu usuário está desativado. Contate o administrador.";
                return View(model);
            }

            if (usuario.SenhaTemporario)
            {
                TempData["ResetUsuarioId"] = usuario.Id;
                TempData["AvisoTemporario"] = "Sua senha é temporária. Por favor, defina uma nova senha segura para continuar.";
                return RedirectToAction(nameof(RedefinirSenha));
            }
            
            await FazerloginNoCookie(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
            return RedirectToAction("Index", "Painel");
        }
        [HttpGet]
        public IActionResult RedefinirSenha()
        {
            if (TempData["ResetUsuarioId"] == null) return RedirectToAction("Login");
            TempData.Keep("ResetUsuarioId");
            return View(new RedefinirSenhaViewModel());
        }
        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
        {
            if (TempData["ResetUsuarioId"] == null) return RedirectToAction("Login");
            
            if(!ModelState.IsValid) 
            {
                TempData.Keep("ResetUsuarioId");
                return View(model);
            }

            int usuarioId = (int)TempData["ResetUsuarioId"];
            var usuario = await contexto.Usuarios.FindAsync(usuarioId);

            if (usuario != null)
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
                usuario.SenhaTemporario = false;
                contexto.Usuarios.Update(usuario);
                await contexto.SaveChangesAsync();

                await FazerloginNoCookie(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
                TempData["Sucesso"] = "Senha redefinida com sucesso! Bem-vindo(a).";
                return RedirectToAction("Index", "Painel");
            }
            return RedirectToAction("Login");
        }
        private async Task FazerloginNoCookie(int id, string nome, string email, string perfil)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, nome ?? "Usuario"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, perfil)
            };

            var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identidade);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
        [HttpGet]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult EsqueciSenha()
        {
            return View(new EsqueciSenhaViewModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (usuario != null)
            {
                string senhaTemporaria = Guid.NewGuid().ToString().Substring(0, 8); // Gera uma senha temporária de 8 caracteres
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaTemporaria);
                usuario.SenhaTemporario = true;

                contexto.Usuarios.Update(usuario);
                await contexto.SaveChangesAsync();

                string mensagem = $"Olá {usuario.Nome}!\n\nUma redefinição de senha foi solicitada. \nSua nova senha temporaria é {senhaTemporaria}\n\nVocê será solicitado a alterá-la no próximo acesso.";

                bool emailEnviado = await emailService.EnviarEmailAsync(usuario.Email, "Recuperação de Senha - VetPlus Care", mensagem);
                if (emailEnviado)
                {
                    TempData["Erro"] = "Serviço de e-mail indisponível. Contate o suporte para redefinir sua senha.";
                    return RedirectToAction("Login");
                }       
            }
            TempData["Sucesso"] = "Se o e-mail estiver cadastrado, você receberá as intruções em breve.";
                return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}
