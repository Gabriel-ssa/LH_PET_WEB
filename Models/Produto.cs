using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_produto")]
    public class Produto
    {
        [Key]
        [Column("pk_produto")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O Nome do produto é obrigatório.")]
        [MaxLength(100)]
        [Column("nm_produto")]
        public string Nome { get; set; }
        
        [Required(ErrorMessage = "O Preço do produto é obrigatório.")]
        [Column("vl_preco")]
        public decimal Preco { get; set; }

        [Required]
        [Column("vl_estoque")]
        public int Estoque { get; set; } = 0;

        [Column("ds_imagem.url")]
        public string ImagemUrl { get; set; } = string.Empty;
    }
}