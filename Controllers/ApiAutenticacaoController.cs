using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Model.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using LH_PET_WEB.Models.ViewModels;

namespace LH_PET_WEB.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class ApitAutenticacaoController : ControllerBase
    {
        private readonly ContextoBanco _contexto;
        private readonly IConfiguration _configuracao;

        public ApitAutenticacaoController (ContextoBanco contexto, IConfiguration configuracao)
        {
            _contexto = contexto;
            _configuracao = configuracao;
        }
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody]ApiRegistroDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var usuarioExiste = await _contexto.Usuarios.AnyAsync(c => c.Email == dto.Email);
                    if (usuarioExiste) return BadRequest(new { mensagem = "E-mail já está em uso."});
                var cpfExiste = await _contexto.Usuarios.AnyAsync(u => u.CPF == dto.CPF);
                    if (cpfExiste) return BadRequest(new { mensagem = "CPF já cadastrado."});
                // 1. Cria o Usuário de acesso(agora passando o nome obrigatório)
                var novoUsuario = new Usuario
                {
                    Nome = dto.Nome, // <-- A CORREÇÃO ESTÁ AQUI
                    Email = dto.Email,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                    Perfil = "Cliente",
                    Ativo = true,
                    SenhaTemporaria = false, // O cliente já cria a senha que ele quer usar
                };
                _contexto.Usuarios.Add(novoUsuario);
                await _contexto.SaveChangesAsync();
                // 2. Cria o perfil do cliente vinculado
                var novoCliente = new Cliente
                {
                    UsuarioId = novoUsuario.Id,
                    Nome = dto.Nome, 
                    Cpf = dto.CPF,
                    Telefone = dto.Telefone,
                };
                _contexto.Clientes.Add(novoCliente);
                await _contexto.SaveChangesAsync();
                
                return Ok(new {mensagem = "Conta criada com sucesso!"});
            }
            catch (Exception ex)
            {
                string erroReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, new { mensagem = "Erro interno ao tentar salvar no banco de dados.", detalhe = erroReal });
            }
        }
            [HttpPost("login")]
            public async Task<IActionResult> Login ([FromBody] ApiLoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
                try
            {
               var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
                {
                    return Unauthorized(new { mensagem = "Credenciais inválidas." });
                }
            }                                        
        }
    }
}