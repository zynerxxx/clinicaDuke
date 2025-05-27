namespace clinicaDukeDB.Models
{
    public class DashboardViewModel
    {
        public int TotalProductos { get; set; }
        public int MovimientosHoy { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosProximosAVencer { get; set; } // Cantidad de productos próximos a vencer
        public List<ProductoStockBajo> ProductosStockBajoList { get; set; } = new();
        public List<MovimientoReciente> MovimientosRecientes { get; set; } = new();
        public List<MovimientoPorDia> MovimientosPorDia { get; set; } = new();
        public List<ProductoProximoAVencer> ProductosProximosAVencerList { get; set; } = new();
        public List<TopProductoMovido> TopProductosMasMovidos { get; set; } = new();
        public List<ProductoStockBajo> TodosLosProductosList { get; set; } = new();
        public List<ViewModels.ProductoViewModel> TodosLosProductosDetallado { get; set; } = new();
    }

    public class ProductoStockBajo
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadActual { get; set; }
    }
    public class MovimientoReciente
    {
        public string Producto { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
    }
    public class MovimientoPorDia
    {
        public string Fecha { get; set; } = string.Empty;
        public int Total { get; set; }
    }
    public class ProductoProximoAVencer
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
    }
    public class TopProductoMovido
    {
        public string Nombre { get; set; } = string.Empty;
        public int TotalMovimientos { get; set; }
    }
}
