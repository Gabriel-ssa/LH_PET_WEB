using Microsoft.AspNetCore.Authentication.JwtBearer; using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using System.Security.Claims;
using LH_PET_WEB.Data; using LH_PET_WEB.Models;
using LH_PET_WEB.Models.ViewModels;

namespace LH_PET_WEB.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Cliente")]
    [ApiController] [Route("api/cliente")]
    public class ApiClienteController : ControllerBase
    {
        private readonly ContextoBanco _contexto;
        public ApiClienteController(ContextoBanco contexto)
        {
            _contexto = contexto;
        }
        private async Task<Cliente?> ObterClienteLogadoAsync()
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdClaim)) return null;
            int usuarioId = int.Parse(usuarioIdClaim);
            return await _contexto.Clientes.FirstOrDefaultAsync(c=> c.UsuarioId == usuarioId);
        }

        [HttpGet("perfil")]
        public async Task<IActionResult> ObterPerfil()
        {
            var cliente = await ObterClienteLogadoAsync(); if (cliente == null) return NotFound();
            var usuario = await
            _contexto.Usuarios.FindAsync(cliente.UsuarioId);
            return Ok(new {
                nome = cliente.Nome, 
                cpf = cliente.Cpf, 
                telefone = cliente.Telefone, 
                email = usuario?.Email
                });
        }

        [HttpPut("perfil")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] ApiPerfilUpdateDTO dto)
        {
            var cliente = await ObterClienteLogadoAsync(); 
            if (cliente == null) return Unauthorized();

            var usuario = await _contexto.Usuarios.FindAsync(cliente.UsuarioId);   
            if (usuario == null) return NotFound();

            if (usuario.Email != dto.Email)
            {
                var emailEmUso = await _contexto.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != usuario.Id);
                if (emailEmUso) return BadRequest(new { mensagem = "Este e-mail já está em uso por outra conta." });
            }

            await _contexto.SaveChangesAsync();
            return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
        }

        [HttpGet("pets")]
        public async Task<IActionResult> ListarPets()
        {
            var cliente = await ObterClienteLogadoAsync(); 
            if (cliente == null) return Unauthorized();

            var petsBanco = await _contexto.Pets
                .Where(p => p.ClienteId == cliente.Id)
                .ToListAsync();
            var petsRetorno = petsBanco.Select(p => new {
                p.Id, 
                p.Nome, 
                p.Especie, 
                p.Raca, 
                DataNascimento = p.DataNascimento.ToString("yyyy-MM-dd"),
                Idade = p.IdadeCalculada});
            return Ok(petsRetorno);
        }
        [HttpsPost("pets")]
        public async Task<iActionResult> AdicionarPet ([FromBody]ApiPetDTO dto)
        {
            var cliente = await ObterClienteLogadoAsync(); 
            if (cliente == null) return Unauthorized();
            
            var novoPet = new Pet
            {
                ClienteId = cliente.Id, 
                Nome = dto.Nome, 
                Especie = dto.Especie, 
                Raca = dto.Raca,
                DataNascimento = dto.DataNascimento
            };
            _contexto.Pets.Add(novoPet);
            await _contexto.SaveChangesAsync();
            return Ok(new { mensagem = "Pet adicionado com sucesso!" });
        }
        [HttpPut("pets/{id}")]
        public async Task<IActionResult> AtualizarPet(int id, [FromBody] ApiPetDTO dto)
        {
            var cliente = await ObterClienteLogadoAsync(); 
            if (cliente == null) return Unauthorized();

            var pet = await _contexto.Pets.FirstOrDefaultAsync(p => p.Id == id && p.ClienteId == cliente.Id);
            if (pet == null) return NotFound(new { mensagem = "Pet não encontrado." });

            pet.Nome = dto.Nome; 
            pet.Especie = dto.Especie; 
            pet.Raca = dto.Raca;
            pet.DataNascimento = dto.DataNascimento;

            _contexto.Pets.Update(pet);
            await _contexto.SaveChangesAsync();
            return Ok(new { mensagem = "Dados do pet atualizados com sucesso!" });
        } 
        [HttpDelete("pets/{id}")]
        public async Task<IActionResult> RemoverPet(int id)
        {
            var cliente = await ObterClienteLogadoAsync(); 
            if (cliente == null) return Unauthorized();

            var pet = await _contexto.Pets.FirstOrDefaultAsync(p => p.Id == id && p.ClienteId == cliente.Id);
            if (pet == null) return NotFound(new { mensagem = "Pet não encontrado." });
            
            var temAgendamentoPendente = await _contexto.Agendamentos.AnyAsync(a => a.PetId == pet.Id && a.Status == "Pendente");
            if (temAgendamentoPendente) return BadRequest(new { mensagem = "Não é possível remover o pet, pois eele possui agendamentos pendentes." });

            _contexto.Pets.Remove(pet);
            await _contexto.SaveChangesAsync();
            return Ok(new { mensagem = "Pet removido com sucesso!" });
        }
        [HttpGet("agendamentos")]
        public async Task<IActionResult> ListarAgendamentos()
        {
            var cliente = await ObterClienteLogadoAsync(); 
            if (cliente == null) return Unauthorized();

            var agendamentos = await _contexto.Agendamentos
                .Include(a => a.Pet)
                .Where(a => a.Pet!.ClienteId == cliente.Id)
                .OrderByDescending(a => a.DataHora)
                .Select(a => new {
                    a.Id, 
                    PetNome = a.Pet!.Nome, 
                    a.DataHora, 
                    a.Tipo, 
                    a.Status
                })
                .ToListAsync();
            
            return Ok(agendamentosRetorno);
        }
        [HttpPost("agendamentos")]
        public async Task<IActionResult> AgendarServico([FromBody] ApiAgendamentoDTO dto)
        {
            var cliente = await ObterClienteLogadoAsync();
            if (cliente == null) return Unauthorized();

            var petPertenceAoCliente = await _contexto.Pets.AnyAsync(p => p.Id == dto.PetId && p.ClienteId == cliente.Id);
            if (!petPertenceAoCliente) return BadRequest(new { mensagem = "Pet inválido ou não pertence a você." });

            var horarioOcupado = await _contexto.Agendamentos.AnyAsync(a => a.DataHora == dto.DataHora && a.Status == "Cancelado");
            if (horarioOcupado) return BadRequest(new { mensagem = "Esse horário já não está mais disponível." });

            var novoAgendamento = new Agendamento
            {
                PetId = dto.PetId,
                DataHora = dto.DataHora,
                Tipo = dto.Tipo,
                Status = "Pendente"
            };
            _contexto.Agendamentos.Add(novoAgendamento);
            await _contexto.SaveChangesAsync();
            return Ok(new { mensagem = "Serviço agendado com sucesso!" });        
        }
        [HttpGet("horarios-disponiveis")]
        public async Task<IActionResult> ObterHorariosDisponiveis([FromQuery]string data, [FromQuery]string tipo)
        {
            if (!DateTime.TryParse(data, out DateTime dataEscolhida)) return BadRequest("Data inválida." );
            if (string.IsNullOrEmpty(tipo)) return BadRequest("Tipo de serviço não informado.");
            
            var config = await _contexto.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);
            if (config == null) return BadRequest("Regras da clínica não configuradas." );

            int diaSemana = (int)dataEscolhida.DayOfWeek;
            if
            (!config.DiasTrabalho.Contains(diaSemana.ToString())) 
            return Ok(new List<string>());

            int minutosServico = tipo == "Consulta" ? config.MinutosConsulta : (tipo == "Banho" ? config.MinutosBanho :config.MinutosTosa);
            if (minutosServico <= 0) minutosServico = 30;

            var agendamentosDoDia = await _contexto.Agendamentos
                .Where(a => a.DataHora.Date == dataEscolhida.Date && a.Status != "Cancelado")
                .ToListAsync();
            
            var horariosLivres = new List<string>();
            TimeSpan atual = config.HoraAbertura;
            TimeSpan duracao = TimeSpan.FromMinutes(minutosServico);
            while (atual.Add(duracao) <= config.HoraFechamento)
            {
                TimeSpan fimAtual = atual.Add(duracao);
                bool temConflito = agendamentosDoDia.Any(a =>
                {
                   TimeSpan aInicio = a.DataHora.TimeOfDay;
                   int aDuracao = a.Tipo == "Consulta" ? config.MinutosConsulta : (a.Tipo == "Banho" ? config.MinutosBanho : config.MinutosTosa); 
                     TimeSpan aFim = aInicio.Add(TimeSpan.FromMinutes(aDuracaoMin));
                    return (atual < aFim) && (fimAtual > aInicio);
                });
                if (!temConflito) horariosLivres.Add(atual.ToString(@"hh\:mm"));
                atual = atual.Add(duracao);                                   
            }
            return Ok(horariosLivres);
        }
    }
}


