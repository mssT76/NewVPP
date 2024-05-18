using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using NewVPP.Models;
using PagedList.Core;

namespace NewVPP.Controllers
{
    public class ProductController : Controller
    {
        private readonly NewWebContext _context;
        public ProductController(NewWebContext context)
        {
            _context = context;
        }


        [Route("shop.html", Name = "ShopProduct")]
        public IActionResult Index(int? page)
        {
            try
            {
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 6;
                var lsProduct = _context.Products
                    .Include(x => x.Cat)
                    .AsNoTracking()
                    .OrderByDescending(x => x.ProductId);
                PagedList<Product> models = new PagedList<Product>(lsProduct, pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("search.html")]
        public async Task<IActionResult> Search(string txtSearch, int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 6;
            var products = _context.Products.Where(m => m.ProductName.Contains(txtSearch))
                .Select(m => new Product()
                {
                    ProductName = m.ProductName,
                    Thumb = m.Thumb,
                    Price = m.Price
                });
            PagedList<Product> models = new PagedList<Product>(products, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View("Index", models);
        }

        [Route("/{Name}", Name = "ListProduct")]
        public IActionResult List(string name, int page = 1)
        {
            try
            {
                var pageSize = 5;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.CatName == name);
                var lsProduct = _context.Products
                    .Include(x => x.Cat)
                    .AsNoTracking()
                    .Where(x => x.CatId == danhmuc.CatId)
                    .OrderByDescending(x => x.DateCreate);
                PagedList<Product> models = new PagedList<Product>(lsProduct, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = danhmuc;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [Route("/{ProductName}-{id}.html", Name = "ProductDetails")]
        public IActionResult Details(int id)
        {
            try
            {
                var product = _context.Products.Include(x => x.Cat).FirstOrDefault(x => x.ProductId == id);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var lsProduct = _context.Products.AsNoTracking()
                    .Where(x => x.CatId == product.CatId && x.ProductId != id)
                    .OrderByDescending(x => x.DateCreate)
                    .Take(4)
                    .ToList();
                ViewBag.SanPham = lsProduct;

                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
