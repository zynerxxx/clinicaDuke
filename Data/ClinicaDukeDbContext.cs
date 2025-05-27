using Microsoft.EntityFrameworkCore;
using clinicaDukeDB.Models;

namespace clinicaDukeDB.Data
{
    public class ClinicaDukeDbContext : DbContext
    {
        public ClinicaDukeDbContext(DbContextOptions<ClinicaDukeDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<clinicaDukeDB.Models.TipoProducto> TipoProducto { get; set; }
        public DbSet<clinicaDukeDB.Models.UnidadMedida> UnidadMedida { get; set; }
        public DbSet<clinicaDukeDB.Models.Laboratorio> Laboratorio { get; set; }
        public DbSet<clinicaDukeDB.Models.Presentacion> Presentacion { get; set; }
        public DbSet<clinicaDukeDB.Models.Categoria> Categoria { get; set; }
        public DbSet<clinicaDukeDB.Models.Stock> Stock { get; set; }
    }
}
