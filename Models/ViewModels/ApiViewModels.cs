using System;
using System.ComponentModel.DataAnnotations;
using LH_PET_WEB.Validations;

namespace LH_PET_WEB.Models.ViewModels
{
    public class ApiLoginDTO
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class ApiRegistroDTO
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "O Nome deve conter no mínimo 3 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [Cpf(ErrorMessage = "O CPF informado é matematicamente inválido.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Telefone é obrigatório.")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$", ErrorMessage = "Formato de telefone inválido.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "A senha é muito fraca.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class ApiPetDTO
    {
        public string Nome { get; set; } = string.Empty;
 
        public string Especie { get; set; } = string.Empty;
        
        public string Raca { get; set; } = string.Empty;

        public DateTime DataNascimento { get; set; }

    }

    public class ApiAgendamentoDTO
    {
        public int PetId { get; set; }

        public DateTime DataHora { get; set; }

        public string Tipo { get; set; } = string.Empty;
    }
    public class ApiperfilUpdateDTO
    {
        [Required]
        [MinLength(3)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$")]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}