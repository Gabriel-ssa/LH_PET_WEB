using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_cliente")]
    public class ArquivoCliente
    {
        [Key]
        [Column("pk_cliente")]
        public int Id { get; set; }
        
        [Required]
        [Column("fk_usuario")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        [Column("nm_nome")]
        public string Nome { get; set; }
        
        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [MaxLength(14)]
        [Column("cd_cpf")]
        public string Cpf { get; set; }
        
        [Required(ErrorMessage = "O Telefone é obrigatório.")]
        [MaxLength(20)]
        [Column("cd_telefone")]
        public string Telefone { get; set; }
        
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        public ICollection<Pet>? Pets { get; set; }
    }
}