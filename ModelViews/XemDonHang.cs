using System;
using NewVPP.Models;

namespace NewVPP.ModelViews
{
	public class XemDonHang
	{
		public Order DonHang { get; set; }
		public List<OrderDetail> ChiTietDonHang { get; set; }
	}
}
