using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using clinicaDukeDB.Models;

namespace clinicaDukeDB.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IConfiguration _configuration;
        public ReportesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ObtenerHistorialMovimientosPorNombre(string producto = "", string fechaInicio = "", string fechaFin = "", int pagina = 1, int tamanoPagina = 10)
        {
            string? connectionString = _configuration.GetConnectionString("ClinicaDukeDb");
            if (string.IsNullOrEmpty(connectionString))
                return StatusCode(500, "Cadena de conexión no encontrada.");

            var historial = new List<MovimientoReciente>();
            int totalRegistros = 0;
            int? idProducto = null;
            if (!string.IsNullOrWhiteSpace(producto) && int.TryParse(producto, out int id))
                idProducto = id;

            DateTime? fechaInicioDt = null;
            DateTime? fechaFinDt = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio))
            {
                DateTime temp;
                if (DateTime.TryParseExact(fechaInicio, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out temp))
                    fechaInicioDt = temp;
            }
            if (!string.IsNullOrWhiteSpace(fechaFin))
            {
                DateTime temp;
                if (DateTime.TryParseExact(fechaFin, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out temp))
                    fechaFinDt = temp;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("ObtenerHistorialMovimientos", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdProducto", (object?)idProducto ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FechaInicio", (object?)fechaInicioDt ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FechaFin", (object?)fechaFinDt ?? DBNull.Value);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? fechaRaw = reader["FechaMovimiento"]?.ToString();
                            string fechaISO = string.Empty;
                            if (!string.IsNullOrWhiteSpace(fechaRaw) && DateTime.TryParse(fechaRaw, out var fechaDt))
                                fechaISO = fechaDt.ToString("yyyy-MM-ddTHH:mm:ss");
                            historial.Add(new MovimientoReciente
                            {
                                Producto = reader["Producto"]?.ToString() ?? string.Empty,
                                Tipo = reader["TipoMovimiento"]?.ToString() ?? string.Empty,
                                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                Fecha = fechaISO,
                                Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                                Observaciones = reader["Observaciones"]?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
            }
            totalRegistros = historial.Count;
            var paged = historial.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToList();
            return Json(new { historial = paged, totalRegistros });
        }
    }
}
