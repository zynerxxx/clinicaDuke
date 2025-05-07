using Microsoft.EntityFrameworkCore;
using clinicaDukeDB.Models;

namespace clinicaDukeDB.Data
{
    public class ClinicaDukeDbContext : DbContext
    {
        public ClinicaDukeDbContext(DbContextOptions<ClinicaDukeDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}
