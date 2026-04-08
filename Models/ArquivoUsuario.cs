using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_usuario")]
    public class Usuario
    {
        [Key]
        [Column("pk_usuario")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(255)]
        [Column("nm_usuario")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [MaxLength(150)]
        [Column("nm_email")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        [Column("ds_senha_hash")]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("ds_perfil")]
        public string Perfil { get; set; } ="Cliente";

        [Column("fl_ativo")]
        public bool Ativo { get; set; } = true;

        [Column("fl_senha_temporaria")]
        public bool SenhaTemporaria { get; set; } = false;
    }
}
