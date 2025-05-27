namespace clinicaDukeDB.ViewModels
{
    public class ProductoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? IdTipoProducto { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
        public int? IdUnidadMedida { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public int? IdPresentacion { get; set; }
        public string Presentacion { get; set; } = string.Empty;
        public int? IdLaboratorio { get; set; }
        public string Laboratorio { get; set; } = string.Empty;
        public int? IdCategoria { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Concentracion { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? FechaVencimiento { get; set; }
        public int CantidadActual { get; set; }
    }
}
