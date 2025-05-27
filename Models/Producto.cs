using System;
using System.ComponentModel.DataAnnotations;

namespace clinicaDukeDB.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int IdTipoProducto { get; set; }
        public int? IdPresentacion { get; set; }
        public int? IdLaboratorio { get; set; }
        public int? IdCategoria { get; set; }
        [Required]
        public int IdUnidadMedida { get; set; }
        [MaxLength(50)]
        public string? Concentracion { get; set; }
        [MaxLength(50)]
        public string? CodigoBarras { get; set; }
        [MaxLength(300)]
        public string? Descripcion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }
}
