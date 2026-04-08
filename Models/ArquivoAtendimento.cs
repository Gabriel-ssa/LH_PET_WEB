using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_atendimento")]
    public class ArquivoAtendimento
    {
        [Key]
        [Column("pk_atendimento")]
        public int Id { get; set; }
        
        [Required]
        [Column("fk_agendamento")]
        public int AgendamentoId { get; set; }
        
        [Required(ErrorMessage = "O Prontuário é obrigatório.")]
        [Column("ds_prontuario")]
        public string Prontuario { get; set; }
        
        [Required(ErrorMessage = "O Diagnóstico é obrigatório.")]
        [Column("ds_diagnostico")]
        public string Diagnostico { get; set; }

        [Required(ErrorMessage = "A Prescrição é obrigatório.")]
        [Column("ds_prescricao")]
        public string Prescricao { get; set; }

        [ForeignKey("AgendamentoId")]
        public Agendamento? Agendamento { get; set; }
    }
}