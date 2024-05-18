using Microsoft.AspNetCore.Mvc;

namespace NewVPP.Controllers
{
	public class AjaxContentController : Controller
	{
		public IActionResult HeaderFavourites()
		{
			return ViewComponent("NumberCart");
		}
	}
}
