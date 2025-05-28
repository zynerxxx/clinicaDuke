using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using clinicaDukeDB.Models;
using clinicaDukeDB.Filters;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;

namespace clinicaDukeDB.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    [ServiceFilter(typeof(TempDataLoginRedirectFilter))]
    public IActionResult Dashboard()
    {
        var model = new DashboardViewModel();
        string? connectionString = _configuration.GetConnectionString("ClinicaDukeDb");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("No se encontró la cadena de conexión 'ClinicaDukeDb'.");
        }
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            // KPIs principales usando funciones auxiliares
            model.MovimientosHoy = ObtenerMovimientosHoy(connection);
            model.ProductosStockBajo = ObtenerProductosStockBajo(connection);
            // TotalProductos y ProductosProximosAVencer siguen igual
            using (var command = new SqlCommand("sp_DashboardResumen", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.TotalProductos = reader.GetInt32(reader.GetOrdinal("TotalProductos"));
                        int colIdx = -1;
                        try { colIdx = reader.GetOrdinal("ProductosProximosAVencer"); } catch { }
                        ViewBag.ProductosProximosAVencer = (colIdx >= 0) ? reader.GetInt32(colIdx) : ObtenerProductosProximosAVencer(connection);
                    }
                }
            }
            // Productos con stock bajo
            using (var command = new SqlCommand("sp_DashboardProductosStockBajo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.ProductosStockBajoList.Add(new ProductoStockBajo
                        {
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            CantidadActual = Convert.ToInt32(reader["CantidadActual"])
                        });
                    }
                }
            }
            // Movimientos recientes
            using (var command = new SqlCommand("sp_DashboardMovimientosRecientes", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.MovimientosRecientes.Add(new MovimientoReciente
                        {
                            Producto = reader["Producto"].ToString() ?? string.Empty,
                            Tipo = reader["Tipo"].ToString() ?? string.Empty,
                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                            Fecha = reader["Fecha"].ToString() ?? string.Empty,
                            Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                            Observaciones = reader["Observaciones"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            // Movimientos por día (para gráfico)
            using (var command = new SqlCommand("sp_DashboardMovimientosPorDia", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.MovimientosPorDia.Add(new MovimientoPorDia
                        {
                            Fecha = Convert.ToDateTime(reader["Fecha"]).ToString("yyyy-MM-dd"),
                            Total = Convert.ToInt32(reader["Total"]) // Usar el alias correcto del SP
                        });
                    }
                }
            }
            // Productos próximos a vencer (lista)
            using (var command = new SqlCommand("sp_ProductosProximosAVencerList", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.ProductosProximosAVencerList.Add(new clinicaDukeDB.Models.ProductoProximoAVencer
                        {
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            FechaVencimiento = reader["FechaVencimiento"] != DBNull.Value ? Convert.ToDateTime(reader["FechaVencimiento"]) : DateTime.MinValue
                        });
                    }
                }
            }
            // Top 5 productos más movidos
            using (var command = new SqlCommand("sp_TopProductosMasMovidos", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.TopProductosMasMovidos.Add(new clinicaDukeDB.Models.TopProductoMovido
                        {
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            TotalMovimientos = reader["TotalMovimientos"] != DBNull.Value ? Convert.ToInt32(reader["TotalMovimientos"]) : 0
                        });
                    }
                }
            }
            // Todos los productos y su stock actual para el modal
            using (var command = new SqlCommand(@"SELECT p.Nombre, ISNULL(s.CantidadActual, 0) AS CantidadActual
                                                  FROM Producto p
                                                  LEFT JOIN Stock s ON s.IdProducto = p.Id", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.TodosLosProductosList.Add(new ProductoStockBajo
                        {
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            CantidadActual = Convert.ToInt32(reader["CantidadActual"])
                        });
                    }
                }
            }
            // Todos los productos y su stock actual para el modal (detallado)
            using (var command = new SqlCommand(@"SELECT p.Id, p.Nombre, p.Concentracion, p.CodigoBarras, p.Descripcion, p.FechaVencimiento, 
                                                        ISNULL(s.CantidadActual, 0) AS CantidadActual,
                                                        tp.Nombre AS TipoProducto, um.Nombre AS UnidadMedida, 
                                                        pre.Nombre AS Presentacion, lab.Nombre AS Laboratorio, cat.Nombre AS Categoria
                                                 FROM Producto p
                                                 LEFT JOIN Stock s ON s.IdProducto = p.Id
                                                 LEFT JOIN TipoProducto tp ON tp.Id = p.IdTipoProducto
                                                 LEFT JOIN UnidadMedida um ON um.Id = p.IdUnidadMedida
                                                 LEFT JOIN Presentacion pre ON pre.Id = p.IdPresentacion
                                                 LEFT JOIN Laboratorio lab ON lab.Id = p.IdLaboratorio
                                                 LEFT JOIN Categoria cat ON cat.Id = p.IdCategoria", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.TodosLosProductosDetallado.Add(new clinicaDukeDB.ViewModels.ProductoViewModel
                        {
                            Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            Concentracion = reader["Concentracion"]?.ToString() ?? string.Empty,
                            CodigoBarras = reader["CodigoBarras"]?.ToString() ?? string.Empty,
                            Descripcion = reader["Descripcion"]?.ToString() ?? string.Empty,
                            FechaVencimiento = reader["FechaVencimiento"] != DBNull.Value ? Convert.ToDateTime(reader["FechaVencimiento"]) : (DateTime?)null,
                            CantidadActual = reader["CantidadActual"] != DBNull.Value ? Convert.ToInt32(reader["CantidadActual"]) : 0,
                            TipoProducto = reader["TipoProducto"]?.ToString() ?? string.Empty,
                            UnidadMedida = reader["UnidadMedida"]?.ToString() ?? string.Empty,
                            Presentacion = reader["Presentacion"]?.ToString() ?? string.Empty,
                            Laboratorio = reader["Laboratorio"]?.ToString() ?? string.Empty,
                            Categoria = reader["Categoria"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            // Pasar el rol del usuario a la vista para lógica de acceso en el KPI de medicamentos
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "Usuario";
            ViewBag.UserRole = userRole;
        }
        ViewBag.ProductosStockBajo = model.ProductosStockBajo;
        ViewBag.MovimientosHoy = model.MovimientosHoy;
        ViewBag.MovimientosPorDiaJson = JsonSerializer.Serialize(model.MovimientosPorDia);
        // Sincronizar el contador de productos próximos a vencer con la lista
        model.ProductosProximosAVencer = model.ProductosProximosAVencerList.Count;
        ViewBag.ProductosProximosAVencer = model.ProductosProximosAVencer;
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Reportes(DateTime? FechaInicio, DateTime? FechaFin, string Usuario, string TipoMovimiento, string Producto, string export)
    {
        var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        if (userRole != "admin" && userRole != "Admin")
        {
            TempData["LoginError"] = "No tienes permiso para realizar esta acción.";
            return RedirectToAction("Dashboard");
        }
        var model = new clinicaDukeDB.Models.ReportesViewModel();
        model.Filtros.FechaInicio = FechaInicio;
        model.Filtros.FechaFin = FechaFin;
        model.Filtros.Usuario = Usuario ?? string.Empty;
        model.Filtros.TipoMovimiento = TipoMovimiento ?? string.Empty;
        model.Filtros.Producto = Producto ?? string.Empty;
        string? connectionString = _configuration.GetConnectionString("ClinicaDukeDb");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("No se encontró la cadena de conexión 'ClinicaDukeDb'.");
        }
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            // Llenar listas de filtros disponibles
            using (var cmd = new SqlCommand("SELECT DISTINCT Nombre FROM Usuarios WHERE Activo = 1", connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read()) model.Filtros.UsuariosDisponibles.Add(reader["Nombre"].ToString() ?? "");
            using (var cmd = new SqlCommand("SELECT DISTINCT Nombre FROM TipoMovimiento", connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read()) model.Filtros.TiposMovimientoDisponibles.Add(reader["Nombre"].ToString() ?? "");
            using (var cmd = new SqlCommand("SELECT DISTINCT Id, Nombre FROM Producto", connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read()) model.Filtros.ProductosDisponibles.Add(new ProductoDisponible
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nombre = reader["Nombre"].ToString() ?? string.Empty
                    });
            // Movimientos filtrados
            var movimientos = new List<clinicaDukeDB.Models.MovimientoReciente>();
            var query = @"SELECT p.Nombre AS Producto, tm.Nombre AS Tipo, m.Cantidad, CONVERT(varchar(16), m.FechaMovimiento, 120) AS Fecha, u.Nombre AS Usuario, m.Observaciones
                        FROM Movimiento m
                        INNER JOIN Producto p ON p.Id = m.IdProducto
                        INNER JOIN TipoMovimiento tm ON tm.Id = m.IdTipoMovimiento
                        INNER JOIN Usuarios u ON u.Id = m.IdUsuario
                        WHERE (m.FechaMovimiento >= @FechaInicio OR @FechaInicio IS NULL)
                          AND (m.FechaMovimiento <= @FechaFin OR @FechaFin IS NULL)
                          AND (@Usuario = '' OR u.Nombre = @Usuario)
                          AND (@TipoMovimiento = '' OR tm.Nombre = @TipoMovimiento)
                          AND (@Producto = '' OR p.Nombre = @Producto)
                        ORDER BY m.FechaMovimiento DESC";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@FechaInicio", (object?)FechaInicio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object?)FechaFin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Usuario", Usuario ?? "");
                cmd.Parameters.AddWithValue("@TipoMovimiento", TipoMovimiento ?? "");
                cmd.Parameters.AddWithValue("@Producto", Producto ?? "");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movimientos.Add(new clinicaDukeDB.Models.MovimientoReciente
                        {
                            Producto = reader["Producto"].ToString() ?? string.Empty,
                            Tipo = reader["Tipo"].ToString() ?? string.Empty,
                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                            Fecha = reader["Fecha"].ToString() ?? string.Empty,
                            Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                            Observaciones = reader["Observaciones"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            var movimientosTodos = new List<clinicaDukeDB.Models.MovimientoReciente>();
            var queryTodos = @"SELECT p.Nombre AS Producto, tm.Nombre AS Tipo, m.Cantidad, CONVERT(varchar(16), m.FechaMovimiento, 120) AS Fecha, u.Nombre AS Usuario, m.Observaciones
                   FROM Movimiento m
                   INNER JOIN Producto p ON p.Id = m.IdProducto
                   INNER JOIN TipoMovimiento tm ON tm.Id = m.IdTipoMovimiento
                   INNER JOIN Usuarios u ON u.Id = m.IdUsuario
                   ORDER BY m.FechaMovimiento DESC";
            using (var cmdTodos = new SqlCommand(queryTodos, connection))
            {
                using (var readerTodos = cmdTodos.ExecuteReader())
                {
                    while (readerTodos.Read())
                    {
                        movimientosTodos.Add(new clinicaDukeDB.Models.MovimientoReciente
                        {
                            Producto = readerTodos["Producto"].ToString() ?? string.Empty,
                            Tipo = readerTodos["Tipo"].ToString() ?? string.Empty,
                            Cantidad = Convert.ToInt32(readerTodos["Cantidad"]),
                            Fecha = readerTodos["Fecha"].ToString() ?? string.Empty,
                            Usuario = readerTodos["Usuario"]?.ToString() ?? string.Empty,
                            Observaciones = readerTodos["Observaciones"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            model.MovimientosRecientes = movimientos.Take(10).ToList(); // Para tabla principal
            // Stock bajo y top productos igual que antes
            using (var command = new SqlCommand("sp_DashboardProductosStockBajo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.ProductosStockBajo.Add(new clinicaDukeDB.Models.ProductoStockBajo
                        {
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            CantidadActual = Convert.ToInt32(reader["CantidadActual"])
                        });
                    }
                }
            }
            // Top 5 productos más movidos
            using (var command = new SqlCommand("sp_TopProductosMasMovidos", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.TopProductosMasMovidos.Add(new clinicaDukeDB.Models.TopProductoMovido
                        {
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            TotalMovimientos = reader["TotalMovimientos"] != DBNull.Value ? Convert.ToInt32(reader["TotalMovimientos"]) : 0
                        });
                    }
                }
            }
            // Resumen por usuario
            if (movimientos.Any())
            {
                model.ResumenPorUsuario = movimientos
                    .GroupBy(m => string.IsNullOrWhiteSpace(m.Usuario) ? "Sin usuario" : m.Usuario)
                    .Select(g => new clinicaDukeDB.Models.ResumenUsuario { Usuario = g.Key, TotalMovimientos = g.Count() })
                    .OrderByDescending(x => x.TotalMovimientos).ToList();
                // Gráfica pastel por usuario (usa todos los movimientos)
                model.GraficaPorUsuario = movimientosTodos
                    .GroupBy(m => string.IsNullOrWhiteSpace(m.Usuario) ? "Sin usuario" : m.Usuario)
                    .Select(g => new clinicaDukeDB.Models.GraficaPastelDato { Etiqueta = g.Key, Valor = g.Count() })
                    .OrderByDescending(x => x.Valor).ToList();
                // Gráfica pastel por tipo (usa todos los movimientos)
                model.GraficaPorTipo = movimientosTodos
                    .GroupBy(m => string.IsNullOrWhiteSpace(m.Tipo) ? "Sin tipo" : m.Tipo)
                    .Select(g => new clinicaDukeDB.Models.GraficaPastelDato { Etiqueta = g.Key, Valor = g.Count() })
                    .OrderByDescending(x => x.Valor).ToList();
            }
            else
            {
                model.ResumenPorUsuario = new List<clinicaDukeDB.Models.ResumenUsuario>();
                model.GraficaPorUsuario = new List<clinicaDukeDB.Models.GraficaPastelDato> { new clinicaDukeDB.Models.GraficaPastelDato { Etiqueta = "Sin datos", Valor = 1 } };
                model.GraficaPorTipo = new List<clinicaDukeDB.Models.GraficaPastelDato> { new clinicaDukeDB.Models.GraficaPastelDato { Etiqueta = "Sin datos", Valor = 1 } };
            }
        }
        // Exportar Excel/PDF (estructura, lógica pendiente)
        if (!string.IsNullOrEmpty(export))
        {
            // Aquí puedes agregar la lógica real de exportación
            // if (export == "excel") ...
            // if (export == "pdf") ...
        }
        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Reportes()
    {
        // Llama al método POST con strings vacíos y export vacío (no null)
        return Reportes(null, null, string.Empty, string.Empty, string.Empty, string.Empty);
    }

    [HttpGet]
    public IActionResult ObtenerHistorialMovimientos(int pagina = 1, int tamanoPagina = 10)
    {
        string? connectionString = _configuration.GetConnectionString("ClinicaDukeDb");
        if (string.IsNullOrEmpty(connectionString))
        {
            return StatusCode(500, "Error interno del servidor: Cadena de conexión no encontrada.");
        }

        List<MovimientoReciente> historial = new List<MovimientoReciente>();
        int totalRegistros = 0;

        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("sp_ObtenerHistorialMovimientosPaginado", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Pagina", pagina);
                command.Parameters.AddWithValue("@TamanoPagina", tamanoPagina);

                using (var reader = command.ExecuteReader())
                {
                    // Leer el primer resultado (historial)
                    while (reader.Read())
                    {
                        string? fechaRaw = reader["Fecha"]?.ToString();
                        string fechaISO = string.Empty;
                        if (!string.IsNullOrWhiteSpace(fechaRaw))
                        {
                            if (DateTime.TryParse(fechaRaw, out var fechaDt))
                            {
                                fechaISO = fechaDt.ToString("yyyy-MM-ddTHH:mm:ss");
                            }
                        }
                        historial.Add(new MovimientoReciente
                        {
                            Producto = reader["Producto"].ToString() ?? string.Empty,
                            Tipo = reader["Tipo"].ToString() ?? string.Empty,
                            Cantidad = Convert.ToInt32(reader["Cantidad"]),
                            Fecha = fechaISO, // Ahora siempre ISO o string.Empty
                            Usuario = reader["Usuario"]?.ToString() ?? string.Empty,
                            Observaciones = reader["Observaciones"]?.ToString() ?? string.Empty
                        });
                    }

                    // Avanzar al siguiente resultado (total de registros)
                    if (reader.NextResult() && reader.Read())
                    {
                        totalRegistros = Convert.ToInt32(reader["TotalRegistros"]);
                    }
                }
            }
        }

        return Json(new { historial, totalRegistros });
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        try
        {
            string? connectionString = _configuration.GetConnectionString("ClinicaDukeDb");
            if (!string.IsNullOrEmpty(connectionString))
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("sp_DashboardResumen", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ViewBag.ProductosStockBajo = reader.GetInt32(reader.GetOrdinal("ProductosStockBajo"));
                                ViewBag.MovimientosHoy = reader.GetInt32(reader.GetOrdinal("MovimientosHoy"));
                            }
                        }
                    }
                }
            }
        }
        catch { /* Silenciar errores para no romper otras vistas */ }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Método auxiliar para obtener el conteo si el SP no lo devuelve
    private int ObtenerProductosProximosAVencer(SqlConnection connection)
    {
        using (var cmd = new SqlCommand("sp_DashboardProductosProximosAVencer", connection))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return reader.GetInt32(reader.GetOrdinal("ProductosProximosAVencer"));
            }
        }
        return 0;
    }

    // Devuelve la cantidad de movimientos realizados hoy
    private int ObtenerMovimientosHoy(SqlConnection connection)
    {
        using (var command = new SqlCommand("sp_DashboardResumen", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(reader.GetOrdinal("MovimientosHoy"));
                }
            }
        }
        return 0;
    }

    // Devuelve la cantidad de productos con stock bajo
    private int ObtenerProductosStockBajo(SqlConnection connection)
    {
        using (var command = new SqlCommand("sp_DashboardResumen", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(reader.GetOrdinal("ProductosStockBajo"));
                }
            }
        }
        return 0;
    }
}
