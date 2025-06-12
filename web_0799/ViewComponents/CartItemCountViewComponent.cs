using Microsoft.AspNetCore.Mvc;
using web_0799.Extensions;
using web_0799.Models;

namespace web_0799.ViewComponents
{
    public class CartItemCountViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            int itemCount = cart.Items.Sum(i => i.Quantity);
            return View(itemCount);
        }
    }
}