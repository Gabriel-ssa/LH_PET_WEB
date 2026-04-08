using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_agendamento")]
    public class ArquivoAgendamento
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Column("fk_pet")]
        public int PetId { get; set; }
        
        [Required(ErrorMessage = "A Data e Hora são obrigatórios.")]
        [Column("dt_data_hora")]
        public DateTime DataHora { get; set; }
        
        [Required(ErrorMessage = "O Tipo de serviço é obrigatório.")]
        [MaxLength(100)]
        [Column("ds_tipo")]
        public string Tipo { get; set; }
        
        [Required]
        [MaxLength(50)]
        [Column("ds_status")]
        public string Status { get; set; }

        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }

        public Atendimento? Atendimento { get; set; }
    }
}