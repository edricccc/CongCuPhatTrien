using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Models
{
    public class ShoppingCart
    {
        public List<ShoppingCartItem> Items { get; set; }
        public ShoppingCart()
        {
            this.Items = new List<ShoppingCartItem>();
        }

        public void AddToCart(ShoppingCartItem item,int Quantity)
        {
            //Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa.
            var checkExits = Items.FirstOrDefault(x => x.ProductId == item.ProductId);
            //Nếu sản phẩm đã tồn tại, cập nhật số lượng và tổng giá.
            if (checkExits != null)
            {
                checkExits.Quantity += Quantity;
                checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
            }
            //Nếu sản phẩm chưa tồn tại, thêm sản phẩm vào giỏ hàng.
            else
            {
                Items.Add(item);
            }
        }

        // Phương thức để loại bỏ một mục từ giỏ hàng dựa trên id.
        public void Remove(int id)
        {
            //Tìm kiếm một mục trong ds giỏ hàng với ProductId trùng với id.
            var checkExits = Items.SingleOrDefault(x=>x.ProductId==id);
            // Kiểm tra xem mục có tồn tại không.
            if (checkExits != null)
            {
                // Nếu tồn tại, loại bỏ mục đó khỏi danh sách giỏ hàng.
                Items.Remove(checkExits);
            }
        }

        public void UpdateQuantity(int id,int quantity)
        {
            //Tìm kiếm một mục trong danh sách giỏ hàng với ProductId trùng với id.
            var checkExits = Items.SingleOrDefault(x => x.ProductId == id);
            // Kiểm tra xem mục có tồn tại khôn
            if (checkExits != null)
            {
                //Nếu tồn tại, cập nhật số lượng của mục đó.
                checkExits.Quantity = quantity;
                //Cập nhật tổng giá dựa trên giá và số lượng mới.
                checkExits.TotalPrice = checkExits.Price * checkExits.Quantity;
            }
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(x=>x.TotalPrice);
        }
        public int GetTotalQuantity()
        {
            return Items.Sum(x => x.Quantity);
        }
        public void ClearCart()
        {
            Items.Clear();
        }

    }

    public class ShoppingCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Alias { get; set; }
        public string CategoryName { get; set; }
        public string ProductImg { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }
}