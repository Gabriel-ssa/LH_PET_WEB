using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LH_PET_WEB.Controllers
{
    [Authorize]
    public class PainelController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.NomeUsuario = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            ViewBag.EmailUsuario = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            ViewBag.PerfilUsuario = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            return View();
        }
    }
}