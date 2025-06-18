using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using web_0799.Models;
using Microsoft.EntityFrameworkCore;

public class CartItemCountViewComponent : ViewComponent
{
    private readonly ProductDBContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartItemCountViewComponent(ProductDBContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return View(0);
        }

        var user = await _userManager.GetUserAsync(HttpContext.User);
        var count = await _context.CartItems
            .Where(i => i.UserId == user.Id)
            .SumAsync(i => i.Quantity);

        return View(count);
    }
}
