using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using web_0799.Models;
using Microsoft.AspNetCore.Identity;

namespace web_0799.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly ProductDBContext _context;

        public AdminController(ProductDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            return View();
        }

        public async Task<IActionResult> User()
        {
            var users = await _context.Users.ToListAsync();
            var userRoles = await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToDictionaryAsync(k => k.UserId, v => v.RoleName);
            ViewBag.UserRoles = userRoles;
            ViewBag.Roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .FirstOrDefaultAsync(ur => ur.UserId == id);

            ViewBag.UserRole = userRole?.RoleName ?? "Không có";
            return View(user);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            return View(new ApplicationUser());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string password, string roleId)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(password))
            {
                var hasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = hasher.HashPassword(user, password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(roleId))
                {
                    _context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = roleId });
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(User));
            }

            ViewBag.Roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            return View(user);
        }

        public async Task<IActionResult> AssignRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, r.Id, r.Name })
                .FirstOrDefaultAsync(ur => ur.UserId == id);

            ViewBag.Roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            ViewBag.CurrentRoleId = userRole?.Id;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string id, string roleId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var existingRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == id);
            if (existingRole != null)
            {
                _context.UserRoles.Remove(existingRole);
            }

            if (!string.IsNullOrEmpty(roleId))
            {
                _context.UserRoles.Add(new IdentityUserRole<string> { UserId = id, RoleId = roleId });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Phân quyền thành công!" });
        }
    }
}