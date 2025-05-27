using clinicaDukeDB.Data;
using clinicaDukeDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace clinicaDukeDB.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ClinicaDukeDbContext _context;
        public ProductosController(ClinicaDukeDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tipos = _context.TipoProducto.ToDictionary(x => x.Id, x => x.Nombre);
            var unidades = _context.UnidadMedida.ToDictionary(x => x.Id, x => x.Nombre);
            var laboratorios = _context.Laboratorio.ToDictionary(x => x.Id, x => x.Nombre);
            var presentaciones = _context.Presentacion.ToDictionary(x => x.Id, x => x.Nombre);
            var categorias = _context.Categoria.ToDictionary(x => x.Id, x => x.Nombre);

            var productos = _context.Producto
                .AsNoTracking()
                .Select(p => new clinicaDukeDB.ViewModels.ProductoViewModel {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    IdTipoProducto = p.IdTipoProducto,
                    TipoProducto = tipos.ContainsKey(p.IdTipoProducto) ? tipos[p.IdTipoProducto] : string.Empty,
                    IdUnidadMedida = p.IdUnidadMedida,
                    UnidadMedida = unidades.ContainsKey(p.IdUnidadMedida) ? unidades[p.IdUnidadMedida] : string.Empty,
                    IdPresentacion = p.IdPresentacion,
                    Presentacion = p.IdPresentacion != null && presentaciones.ContainsKey(p.IdPresentacion.Value) ? presentaciones[p.IdPresentacion.Value] : string.Empty,
                    IdLaboratorio = p.IdLaboratorio,
                    Laboratorio = p.IdLaboratorio != null && laboratorios.ContainsKey(p.IdLaboratorio.Value) ? laboratorios[p.IdLaboratorio.Value] : string.Empty,
                    IdCategoria = p.IdCategoria,
                    Categoria = p.IdCategoria != null && categorias.ContainsKey(p.IdCategoria.Value) ? categorias[p.IdCategoria.Value] : string.Empty,
                    Concentracion = p.Concentracion ?? string.Empty,
                    CodigoBarras = p.CodigoBarras ?? string.Empty,
                    Descripcion = p.Descripcion ?? string.Empty,
                    FechaVencimiento = p.FechaVencimiento,
                    CantidadActual = 0 // O el valor real si lo tienes
                })
                .ToList();

            ViewBag.Tipos = tipos;
            ViewBag.Unidades = unidades;
            ViewBag.Laboratorios = laboratorios;
            ViewBag.Presentaciones = presentaciones;
            ViewBag.Categorias = categorias;
            return View(productos);
        }

    [HttpGet]
    public IActionResult Detalle(int id)
    {
        var producto = _context.Producto.FirstOrDefault(p => p.Id == id);
        if (producto == null)
            return NotFound();
        return Json(producto);
    }

        [HttpPost]
        public IActionResult Crear(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Producto.Add(producto);
                _context.SaveChanges();
                TempData["Success"] = "Producto creado correctamente.";
                return RedirectToAction("Index");
            }
            TempData["Error"] = "Corrija los errores e intente nuevamente.";
            // Volver a cargar los catálogos y productos como ViewModel
            var tipos = _context.TipoProducto.ToDictionary(x => x.Id, x => x.Nombre);
            var unidades = _context.UnidadMedida.ToDictionary(x => x.Id, x => x.Nombre);
            var laboratorios = _context.Laboratorio.ToDictionary(x => x.Id, x => x.Nombre);
            var presentaciones = _context.Presentacion.ToDictionary(x => x.Id, x => x.Nombre);
            var categorias = _context.Categoria.ToDictionary(x => x.Id, x => x.Nombre);
            var productos = _context.Producto
                .AsNoTracking()
                .ToList()
                .Select(p => new clinicaDukeDB.ViewModels.ProductoViewModel {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    IdTipoProducto = p.IdTipoProducto,
                    TipoProducto = tipos.ContainsKey(p.IdTipoProducto) ? tipos[p.IdTipoProducto] : string.Empty,
                    IdUnidadMedida = p.IdUnidadMedida,
                    UnidadMedida = unidades.ContainsKey(p.IdUnidadMedida) ? unidades[p.IdUnidadMedida] : string.Empty,
                    IdPresentacion = p.IdPresentacion,
                    Presentacion = p.IdPresentacion != null && presentaciones.ContainsKey(p.IdPresentacion.Value) ? presentaciones[p.IdPresentacion.Value] : string.Empty,
                    IdLaboratorio = p.IdLaboratorio,
                    Laboratorio = p.IdLaboratorio != null && laboratorios.ContainsKey(p.IdLaboratorio.Value) ? laboratorios[p.IdLaboratorio.Value] : string.Empty,
                    IdCategoria = p.IdCategoria,
                    Categoria = p.IdCategoria != null && categorias.ContainsKey(p.IdCategoria.Value) ? categorias[p.IdCategoria.Value] : string.Empty,
                    Concentracion = p.Concentracion ?? string.Empty,
                    CodigoBarras = p.CodigoBarras ?? string.Empty,
                    Descripcion = p.Descripcion ?? string.Empty,
                    FechaVencimiento = p.FechaVencimiento,
                    CantidadActual = 0 // O el valor correcto si lo tienes
                })
                .ToList();
            ViewBag.Tipos = tipos;
            ViewBag.Unidades = unidades;
            ViewBag.Laboratorios = laboratorios;
            ViewBag.Presentaciones = presentaciones;
            ViewBag.Categorias = categorias;
            return View("Index", productos);
        }

        [HttpPost]
        public IActionResult Editar(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Producto.Update(producto);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            var productos = _context.Producto.ToList();
            return View("Index", productos);
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var producto = _context.Producto.Find(id);
            if (producto != null)
            {
                _context.Producto.Remove(producto);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ObtenerAjax()
        {
            // Parámetros DataTables
            var draw = Request.Query["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Query["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Query["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Query["search[value]"].FirstOrDefault()?.ToLower();
            var sortColumnIndex = Convert.ToInt32(Request.Query["order[0][column]"].FirstOrDefault() ?? "0");
            var sortDirection = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

            // Columnas para ordenar
            string[] columnNames = { "Nombre", "IdTipoProducto", "IdUnidadMedida", "Concentracion", "CodigoBarras", "Id" };
            string sortColumn = columnNames[sortColumnIndex];

            var query = _context.Producto.AsNoTracking();
            int recordsTotal = query.Count();

            // Filtro de búsqueda
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(searchValue) ||
                    (p.Concentracion ?? "").ToLower().Contains(searchValue) ||
                    (p.CodigoBarras ?? "").ToLower().Contains(searchValue)
                );
            }
            int recordsFiltered = query.Count();

            // Ordenamiento dinámico seguro
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            // Paginación
            var data = query.Skip(start).Take(length).ToList();

            // Proyección a ViewModel para la tabla
            var productos = data.Select(p => new clinicaDukeDB.ViewModels.ProductoViewModel {
                Id = p.Id,
                Nombre = p.Nombre,
                IdTipoProducto = p.IdTipoProducto,
                IdUnidadMedida = p.IdUnidadMedida,
                Concentracion = p.Concentracion ?? string.Empty,
                CodigoBarras = p.CodigoBarras ?? string.Empty
            }).ToList();

            return Json(new {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = productos
            });
        }
    }
}