using NewVPP.Models;

namespace NewVPP.ModelViews
{
    public class CartItem
    {
            public Product product { get; set; }
            public int amount { get; set; }

            public int TotalMoney => amount * product.Price.Value;
    }
}
