using System.ComponentModel.DataAnnotations;

namespace LH_PET_WEB.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class EsqueciSenhaViewModel
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    public class UsuarioCreateViewModel
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Perfil é obrigatório.")]
        public string Perfil { get; set; } = "Funcionário";

    }

    public class RedefinirSenhaViewModel
    {
        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "A senha deve ter no minimo 8 caracteres, contendo pelo menos 1 letra maiúscula, 1 minúscula, 1 número e 1 caractere especial.")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a nova senha.")]
        [Compare("NovaSenha", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}