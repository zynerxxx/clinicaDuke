using System;

namespace clinicaDukeDB.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public int CantidadActual { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
    }
}
