using System;
using System.ComponentModel.DataAnnotations;

namespace clinicaDukeDB.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UsuarioNombre { get; set; } = string.Empty;

        [Required]
        public string ContrasenaHash { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public string? AvatarUrl { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? UltimoAcceso { get; set; }
        public int IntentosFallidos { get; set; } = 0;
        public bool Bloqueado { get; set; } = false;
        public string? TokenRecuperacion { get; set; }
        public DateTime? TokenExpira { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; } = true;
    }
}