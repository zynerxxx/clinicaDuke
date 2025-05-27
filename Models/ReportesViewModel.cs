using System.Collections.Generic;

namespace clinicaDukeDB.Models
{
    public class ReportesViewModel
    {
        public List<ProductoStockBajo> ProductosStockBajo { get; set; } = new();
        public List<MovimientoReciente> MovimientosRecientes { get; set; } = new();
        public List<TopProductoMovido> TopProductosMasMovidos { get; set; } = new();
        public ReportesFiltroViewModel Filtros { get; set; } = new();
        public List<ResumenUsuario> ResumenPorUsuario { get; set; } = new();
        public List<GraficaPastelDato> GraficaPorUsuario { get; set; } = new();
        public List<GraficaPastelDato> GraficaPorTipo { get; set; } = new();
    }

    public class ResumenUsuario
    {
        public string Usuario { get; set; } = string.Empty;
        public int TotalMovimientos { get; set; }
    }
    public class GraficaPastelDato
    {
        public string Etiqueta { get; set; } = string.Empty;
        public int Valor { get; set; }
    }
}
