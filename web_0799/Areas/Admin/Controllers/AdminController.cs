using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using web_0799.Models;

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

            return View(users);
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
    }
}
