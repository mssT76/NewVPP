using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewVPP.Models;
using NewVPP.ModelViews;
using NewVPP.Extension;
using Newtonsoft.Json;
using NewVPP.Models.Payment;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace NewVPP.Controllers
{
	public class ShoppingCartController : Controller
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		private readonly NewWebContext _context;

		private readonly IConfiguration _configuration;
		public ShoppingCartController(NewWebContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_configuration = configuration;
			_httpContextAccessor = httpContextAccessor;
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

        [Route("ShoppingCart/VnpayReturn")]
        public ActionResult VnpayReturn()
		{
            if (HttpContext.Request.Query.Count > 0)
            {
                string vnp_HashSecret = _configuration["Vnpay:HashSecret"]; //Chuoi bi mat
                var vnpayData = HttpContext.Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value);
                    }
                }
                long orderCode = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = HttpContext.Request.Query["vnp_SecureHash"];
                String TerminalID = HttpContext.Request.Query["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = _context.Orders.FirstOrDefault(x => x.OrderId == orderCode);
                        if (itemOrder != null)
                        {
                            _context.Update(itemOrder);
                            _context.SaveChanges();
                        }
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                    }
                    else
                    {
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                    }
                    ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                }
            }
            return View();
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
        public IActionResult IndexCart()
        {
            return View(GioHang);
        }
		
		public List<CartItem> GioHangCheck
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

		[Route("checkout.html", Name = "Checkout")]
		public IActionResult Index(string returnUrl = null)
		{
			//Lay gio hang ra de xu ly
			var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("GioHang");

			var taikhoanID = HttpContext.Session.GetString("CustomerId");
			MuaHangVM model = new MuaHangVM();
			if (taikhoanID != null)
			{
				var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
				model.CustomerId = khachhang.CustomerId;
				model.FullName = khachhang.FullName;
				model.Email = khachhang.Email;
				model.Phone = khachhang.Phone;
				model.Address = khachhang.Address;
			}
			ViewBag.GioHang = cart;
			return View(model);
		}


        [HttpPost]
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(MuaHangVM muaHang)
        {
            var code = new { Success = false, Code = -1, Url = "" };
            //Lấy ra giỏ hàng để xử lý
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;

                khachhang.Address = muaHang.Address;
                _context.Update(khachhang);
                _context.SaveChanges();
            }

            if (ModelState.IsValid)
            {
                // Khởi tạo đơn hàng
                Order donhang = new Order();
                donhang.CustomerId = model.CustomerId;
                donhang.Address = model.Address;
                donhang.TypePayment = muaHang.TypePayment;
                donhang.OrderDate = DateTime.Now;
                donhang.Deleted = false;
                donhang.TotalMoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                _context.Add(donhang);
                _context.SaveChanges();

                // Tạo danh sách đơn hàng
                foreach (var item in cart)
                {
                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.OrderId = donhang.OrderId;
                    orderDetail.ProductId = item.product.ProductId;
                    orderDetail.Amount = item.amount;
                    orderDetail.Price = item.product.Price;
                    orderDetail.TotalMoney = donhang.TotalMoney;
                    _context.Add(orderDetail);
                }
                _context.SaveChanges();

                // Xác định URL thanh toán
                if (muaHang.TypePayment == 1)
                {
                    donhang.TransactStatusId = 1; // Đơn hàng mới
                    _context.SaveChanges();
                    return RedirectToAction("Success", "ShoppingCart");
                }
                else if (muaHang.TypePayment == 2)
                {
                    // Nếu chọn hình thức thanh toán là 2, trả về URL thanh toán từ phương thức UrlPayment
                    var url = UrlPayment(muaHang.TypePaymentVN, donhang.OrderId);
                    code = new { Success = true, Code = muaHang.TypePayment, Url = url };
                    donhang.TransactStatusId = 3;//Đã thanh toán
                    _context.SaveChanges();
                    return Redirect(url);
                }

                // Xóa giỏ hàng sau khi đặt hàng thành công
                HttpContext.Session.Remove("GioHang");
            }

            ViewBag.GioHang = cart;
            return View(model);
        }


        [Route("dat-hang-thanh-cong.html", Name = "Success")]
		public IActionResult Success()
		{
			try
			{
				var taikhoanID = HttpContext.Session.GetString("CustomerId");
				if (string.IsNullOrEmpty(taikhoanID))
				{
					return RedirectToAction("Login", "Accounts", new { returnUrl = "/dat-hang-thanh-cong.html" });
				}
				var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
				var donhang = _context.Orders
					.Where(x => x.CustomerId == Convert.ToInt32(taikhoanID))
					.OrderByDescending(x => x.OrderDate)
					.FirstOrDefault();
				MuaHangSuccessVM successVM = new MuaHangSuccessVM();
				successVM.FullName = khachhang.FullName;
				successVM.DonHangID = donhang.OrderId;
				successVM.Phone = khachhang.Phone;
				successVM.Address = khachhang.Address;
				return View(successVM);
			}
			catch
			{
				return View();
			}
		}



		#region Thanh toán vnpay
		public string UrlPayment(int TypePaymentVN, int orderCode)
		{
			var urlPayment = "";
			var order = _context.Orders.FirstOrDefault(x => x.OrderId == orderCode);
			// Get Config Info
			string vnp_Returnurl = _configuration["Vnpay:ReturnUrl"];
			string vnp_Url = _configuration["Vnpay:Url"];
			string vnp_TmnCode = _configuration["Vnpay:TmnCode"];
			string vnp_HashSecret = _configuration["Vnpay:HashSecret"];

			//Build URL for VNPAY
			var tick = DateTime.Now.Ticks.ToString();


			VnPayLibrary vnpay = new VnPayLibrary();
			var Price = (long)order.TotalMoney * 100;
			vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
			vnpay.AddRequestData("vnp_Command", "pay");
			vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
			vnpay.AddRequestData("vnp_Amount", Price.ToString()); 
            //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ.
            //Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ)
            //thì merchant cần nhân thêm 100 lần (khử phần thập phân),
            //sau đó gửi sang VNPAY là: 10000000
			if (TypePaymentVN == 1)
			{
				vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
			}
			else if (TypePaymentVN == 2)
			{
				vnpay.AddRequestData("vnp_BankCode", "VNBANK");
			}
			else if (TypePaymentVN == 3)
			{
				vnpay.AddRequestData("vnp_BankCode", "INTCARD");
			}
            vnpay.AddRequestData("vnp_CreateDate",order.OrderDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
			vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
			vnpay.AddRequestData("vnp_Locale", "vn");
			vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.OrderId);
			vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

			vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
			vnpay.AddRequestData("vnp_TxnRef", tick); 
            // Mã tham chiếu của giao dịch tại hệ thống của merchant.
            // Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

			urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
			return urlPayment;
		}
        #endregion
        public class VnPayLibrary
        {
            public const string VERSION = "2.1.0";
            private SortedList<String, String> _requestData = new SortedList<String, String>(new VnPayCompare());
            private SortedList<String, String> _responseData = new SortedList<String, String>(new VnPayCompare());

            public void AddRequestData(string key, string value)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _requestData.Add(key, value);
                }
            }

            public void AddResponseData(string key, string value)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _responseData.Add(key, value);
                }
            }

            public string GetResponseData(string key)
            {
                string retValue;
                if (_responseData.TryGetValue(key, out retValue))
                {
                    return retValue;
                }
                else
                {
                    return string.Empty;
                }
            }

            #region Request

            public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
            {
                StringBuilder data = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in _requestData)
                {
                    if (!String.IsNullOrEmpty(kv.Value))
                    {
                        data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                    }
                }
                string queryString = data.ToString();

                baseUrl += "?" + queryString;
                String signData = queryString;
                if (signData.Length > 0)
                {

                    signData = signData.Remove(data.Length - 1, 1);
                }
                string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
                baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

                return baseUrl;
            }
            #endregion

            #region Response process

            public bool ValidateSignature(string inputHash, string secretKey)
            {
                string rspRaw = GetResponseData();
                string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
                return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
            }
            private string GetResponseData()
            {

                StringBuilder data = new StringBuilder();
                if (_responseData.ContainsKey("vnp_SecureHashType"))
                {
                    _responseData.Remove("vnp_SecureHashType");
                }
                if (_responseData.ContainsKey("vnp_SecureHash"))
                {
                    _responseData.Remove("vnp_SecureHash");
                }
                foreach (KeyValuePair<string, string> kv in _responseData)
                {
                    if (!String.IsNullOrEmpty(kv.Value))
                    {
                        data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                    }
                }
                //remove last '&'
                if (data.Length > 0)
                {
                    data.Remove(data.Length - 1, 1);
                }
                return data.ToString();
            }

            #endregion
        }

        public static class Utils
        {
            private static IHttpContextAccessor _httpContextAccessor;

            public static void Configure(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public static string GetIpAddress()
            {
                string ipAddress = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (string.IsNullOrEmpty(ipAddress) || ipAddress.ToLower() == "unknown")
                {
                    ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = "127.0.0.1"; // Sử dụng địa chỉ IP mặc định nếu không thể xác định
                }

                return ipAddress;
            }

            public static string HmacSHA512(string key, string inputData)
            {
                var hash = new StringBuilder();
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
                using (var hmac = new HMACSHA512(keyBytes))
                {
                    byte[] hashValue = hmac.ComputeHash(inputBytes);
                    foreach (var theByte in hashValue)
                    {
                        hash.Append(theByte.ToString("x2"));
                    }
                }
                return hash.ToString();
            }
        }


        public class VnPayCompare : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var vnpCompare = CompareInfo.GetCompareInfo("en-US");
                return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
            }
        }
    }
}
