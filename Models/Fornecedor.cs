using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_fornecedor")]
    public class Fornecedor
    {
        [Key]
        [Column("pk_fornecedor")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [MaxLength(18)]
        [Column("cd_cnpj")]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        [Column("nm_fornecedor")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [MaxLength(150)]
        [Column("nm_email")]
        public string Email { get; set; } = string.Empty;
        
        
        
    }
}