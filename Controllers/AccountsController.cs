using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewVPP.Helpper;
using NewVPP.Models;
using NewVPP.ModelViews;

namespace NewVPP.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly NewWebContext _context;
        public AccountsController(NewWebContext context)
        {
            _context = context;
        }

		[Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
		public IActionResult Dashboard()
		{
			var taikhoanID = HttpContext.Session.GetString("CustomerId");
			if (taikhoanID != null)
			{
				var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
				if (khachhang != null)
				{
					var lsDonHang = _context.Orders
						.Include(x => x.TransactStatus)
						.AsNoTracking()
						.Where(x => x.CustomerId == khachhang.CustomerId)
						.OrderByDescending(x => x.OrderDate)
						.ToList();
                    ViewBag.DonHang = lsDonHang;
					return View(khachhang);
				}

			}
			return RedirectToAction("Login");
		}
		[HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public IActionResult DangkyTaiKhoan()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangkyTaiKhoan(RegisterViewModel taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SHA256 hashMethod = SHA256.Create();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = Extension.HashMD5.GetHash(hashMethod, taikhoan.Password),
                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        //Lưu Session MaKh
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");

                        //Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,khachhang.FullName),
                            new Claim("CustomerId", khachhang.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    catch
                    {
                        return RedirectToAction("DangkyTaiKhoan", "Accounts");
                    }
                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }
        }

        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);

                    if (khachhang == null) return RedirectToAction("DangkyTaiKhoan");

                    SHA256 hashMethod = SHA256.Create();

                    if (Extension.HashMD5.VerifyHash(hashMethod, customer.Password, khachhang.Password))
                    {
                        //Luu Session MaKh
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");

                        //Identity
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString())
                    };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToAction("Dashboard", "Accounts");
                        }
                        else
                        {
                            return Redirect(returnUrl);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Dang nhap that bai");
                        return View(customer);
                    }
                }
            }
            catch
            {
                return RedirectToAction("DangkyTaiKhoan", "Accounts");
            }
            return View(customer);
        }

		[HttpGet]
		[Route("dang-xuat.html", Name = "DangXuat")]
		public IActionResult Logout()
		{
			HttpContext.SignOutAsync();
			HttpContext.Session.Remove("CustomerId");
			return RedirectToAction("Index", "Home");
		}


        [HttpPost]
		public IActionResult ChangePassword(ChangePasswordViewModel model)
		{
			try
			{
				var taikhoanID = HttpContext.Session.GetString("CustomerId");
				if (taikhoanID == null)
				{
					return RedirectToAction("Login", "Accounts");
				}
				if (ModelState.IsValid)
				{
					var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
					if (taikhoan == null) return RedirectToAction("Login", "Accounts");

					SHA256 hashMethod = SHA256.Create();
                    var pass = Extension.HashMD5.GetHash(hashMethod, taikhoan.Password);
					{
						string passnew = Extension.HashMD5.GetHash(hashMethod, model.Password);
						taikhoan.Password = passnew;
						_context.Update(taikhoan);
						_context.SaveChanges();
						return RedirectToAction("Dashboard", "Accounts");
					}
				}
			}
			catch
			{
				return RedirectToAction("Dashboard", "Accounts");
			}
			return RedirectToAction("Dashboard", "Accounts");
		}
	}
}
