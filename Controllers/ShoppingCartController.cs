using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewVPP.Models;
using NewVPP.ModelViews;
using NewVPP.Extension;
using Newtonsoft.Json;

namespace NewVPP.Controllers
{
	public class ShoppingCartController : Controller
	{
		private readonly NewWebContext _context;
		public ShoppingCartController(NewWebContext context)
		{
			_context = context;
		}

		public List<CartItem> GioHang
		{
			get
			{
				var gh = HttpContext.Session.GetObjectFromJson<List<CartItem>>("GioHang");
				if (gh == default(List<CartItem>))
				{
					gh = new List<CartItem>();
				}
				return gh;
			}
		}


		[HttpPost]
		[Route("api/cart/add")]
		public IActionResult AddToCart(int productID, int? amount)
		{
			List<CartItem> giohang = GioHang;

			try
			{
				//Them san pham vao gio hang
				CartItem item = GioHang.SingleOrDefault(p => p.product.ProductId == productID);
				if (item != null) // da co => cap nhat so luong
				{
					item.amount = item.amount + amount.Value;
					//chuyen doi list danh sach thanh json
					//luu lai session
					HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(giohang));
				}
				else
				{
					Product hh = _context.Products.SingleOrDefault(p => p.ProductId == productID);
					item = new CartItem
					{
						amount = amount.HasValue ? amount.Value : 1,
						product = hh
					};
					giohang.Add(item);//Them vao gio
				}

				//Luu lai Session
				HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(giohang));
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCart(int productID, int? amount)
        {
            //Lay gio hang ra de xu ly
            var giohang = HttpContext.Session.GetObjectFromJson<List<CartItem>>("GioHang");
            try
            {
                if (giohang != null)
                {
                    CartItem item = giohang.SingleOrDefault(p => p.product.ProductId == productID);
                    if (item != null && amount.HasValue) // da co -> cap nhat so luong
                    {
                        item.amount = amount.Value;
                    }
                    //Luu lai session
                    HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(giohang));
                }
                return Json(new { success = true });
            }
            catch 
            {
                return Json(new { success = false });
            }
        }


        [HttpPost]
		[Route("api/cart/remove")]
		public ActionResult Remove(int productID)
		{
			try
			{
				List<CartItem> gioHang = GioHang;
				CartItem item = gioHang.SingleOrDefault(p => p.product.ProductId == productID);
				if (item != null)
				{
					gioHang.Remove(item);
				}
				//luu lai session
				HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(gioHang));
				return Json(new { success = true });
			}
			catch
			{
				return Json(new { success = false });
			}
		}



        [Route("cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            return View(GioHang);
        }
    }
}
