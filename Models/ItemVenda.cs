using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_item_venda")]
    public class ItemVenda
    {
        [Key]
        [Column("pk_item_venda")]
        public int Id { get; set; }
        
        [Required]
        [Column("fk_venda")]
        public int VendaId { get; set; }
        
        [Required]
        [Column("fk_produto")]
        public int ProdutoId { get; set; }
        
        [Required]
        [Column("vl_quantidade")]
        public int Quantidade { get; set; }
        
        [Required]
        [Column("vl_preco_unitario")]
        public decimal PrecoUnitario { get; set; }

        
    }
}