using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using clinicaDukeDB.Data;
using clinicaDukeDB.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Importar el espacio de nombres necesario
using Microsoft.Data.SqlClient; // Importar el espacio de nombres para SqlParameter

namespace clinicaDukeDB.Controllers
{
    [Authorize]
    public class MovimientosController : Controller
    {
        private readonly ClinicaDukeDbContext _context;

        public MovimientosController(ClinicaDukeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult RegistrarEntrada()
        {
            var productos = _context.Producto.ToList();
            var stock = _context.Stock
                .GroupBy(s => s.IdProducto)
                .Select(g => g.OrderByDescending(s => s.FechaUltimaActualizacion).FirstOrDefault())
                .ToList();
            // Unir productos y stock (último stock por producto, null si no hay stock)
            var productosConStock = productos.Select(p => {
                var s = stock != null ? stock.FirstOrDefault(x => x != null && x.IdProducto == p.Id) : null;
                return new {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CantidadActual = s?.CantidadActual ?? 0
                };
            }).ToList();
            ViewBag.Productos = productosConStock;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarEntrada(int productoId, int cantidad, string observaciones)
        {
            if (cantidad <= 0)
            {
                ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a cero.");
                ViewBag.Productos = _context.Producto.ToList();
                return View();
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == "UsuarioNombre")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("User", "No se pudo identificar al usuario.");
                ViewBag.Productos = _context.Producto.ToList();
                return View();
            }

            // Obtener el Id del usuario autenticado
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == userName);
            if (usuario == null)
            {
                ModelState.AddModelError("User", "Usuario no encontrado en la base de datos.");
                ViewBag.Productos = _context.Producto.ToList();
                return View();
            }

            try
            {
                // Registrar los parámetros enviados
                Console.WriteLine($"Parámetros enviados: IdProducto={productoId}, IdTipoMovimiento=1, Cantidad={cantidad}, IdUsuario={usuario.Id}, Observaciones={observaciones}");

                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC RegistrarMovimientoInventario @IdProducto = {0}, @IdTipoMovimiento = {1}, @Cantidad = {2}, @IdUsuario = {3}, @Observaciones = {4}",
                    productoId, 1, cantidad, usuario.Id, observaciones ?? (object)DBNull.Value
                );

                // Siempre mostrar éxito si no hay excepción
                TempData["Success"] = "Entrada registrada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar la entrada: {ex.Message}";
            }

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public IActionResult RegistrarSalida()
        {
            var productos = _context.Producto.ToList();
            var stock = _context.Stock
                .GroupBy(s => s.IdProducto)
                .Select(g => g.OrderByDescending(s => s.FechaUltimaActualizacion).FirstOrDefault())
                .ToList();
            // Unir productos y stock (último stock por producto, null si no hay stock)
            var productosConStock = productos.Select(p => {
                var s = stock != null ? stock.FirstOrDefault(x => x != null && x.IdProducto == p.Id) : null;
                return new {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CantidadActual = s?.CantidadActual ?? 0
                };
            }).ToList();
            ViewBag.Productos = productosConStock;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida(int productoId, int cantidad, string observaciones)
        {
            if (cantidad <= 0)
            {
                ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a cero.");
                ViewBag.Productos = _context.Producto.ToList();
                return View();
            }

            var userName = User.Claims.FirstOrDefault(c => c.Type == "UsuarioNombre")?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("User", "No se pudo identificar al usuario.");
                ViewBag.Productos = _context.Producto.ToList();
                return View();
            }

            // Obtener el Id del usuario autenticado
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioNombre == userName);
            if (usuario == null)
            {
                ModelState.AddModelError("User", "Usuario no encontrado en la base de datos.");
                ViewBag.Productos = _context.Producto.ToList();
                return View();
            }

            // Asegurarse de que los parámetros sean del tipo correcto
            var productoIdParam = new SqlParameter("@IdProducto", System.Data.SqlDbType.Int) { Value = productoId };
            var tipoMovimientoParam = new SqlParameter("@IdTipoMovimiento", System.Data.SqlDbType.Int) { Value = 2 };
            var cantidadParam = new SqlParameter("@Cantidad", System.Data.SqlDbType.Int) { Value = -cantidad };
            var userIdParam = new SqlParameter("@IdUsuario", System.Data.SqlDbType.Int) { Value = usuario.Id };
            var observacionesParam = new SqlParameter("@Observaciones", System.Data.SqlDbType.NVarChar) { Value = observaciones ?? (object)DBNull.Value };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC RegistrarMovimientoInventario @IdProducto = {0}, @IdTipoMovimiento = {1}, @Cantidad = {2}, @IdUsuario = {3}, @Observaciones = {4}",
                    productoId, 2, -cantidad, usuario.Id, observaciones ?? (object)DBNull.Value
                );

                TempData["Success"] = "Salida registrada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar la salida: {ex.Message}";
            }

            return RedirectToAction("Dashboard", "Home");
        }
    }
}
