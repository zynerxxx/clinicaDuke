using Microsoft.AspNetCore.Mvc;
using clinicaDukeDB.Data;
using clinicaDukeDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace clinicaDukeDB.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClinicaDukeDbContext _context;
        public AccountController(ClinicaDukeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login([FromQuery] string? returnUrl)
        {
            if (TempData["LoginError"] != null)
            {
                ViewBag.Error = TempData["LoginError"]?.ToString();
            }
            else if (!(User.Identity?.IsAuthenticated ?? false) && !string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.Error = "¡Inicia sesión primero!";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Por favor, ingresa usuario y contraseña.";
                return RedirectToAction("Login");
            }
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioNombre == username && u.Activo);
            if (usuario == null || usuario.ContrasenaHash != password)
            {
                TempData["LoginError"] = "Usuario o contraseña incorrectos.";
                return RedirectToAction("Login");
            }
            // Crear claims para el usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim("UsuarioNombre", usuario.UsuarioNombre),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Usuario")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            TempData["Success"] = $"¡Bienvenido, {usuario.Nombre}!";
            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["LogoutMessage"] = "Has cerrado sesión correctamente.";
            return RedirectToAction("Index", "Home");
        }
    }
}
