using Microsoft.AspNetCore.Mvc;
using NewVPP.Extension;
using NewVPP.ModelViews;

namespace NewVPP.Controllers.Components
{
	public class NumberCartViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("GioHang");
			return View(cart);
		}
	}
}
