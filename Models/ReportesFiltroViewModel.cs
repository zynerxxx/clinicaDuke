using System;
using System.Collections.Generic;

namespace clinicaDukeDB.Models
{
    public class ReportesFiltroViewModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public List<string> UsuariosDisponibles { get; set; } = new();
        public List<string> TiposMovimientoDisponibles { get; set; } = new();
        public List<ProductoDisponible> ProductosDisponibles { get; set; } = new();
    }

    public class ProductoDisponible
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
