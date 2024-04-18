using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using PagedList;
using System.Globalization;
using System.Data.Entity;
using WebBanHangOnline.Models.ViewModels;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        public ActionResult Index(int? page)
        {
            // Lấy ds đơn đặt hàng từ csdl và sxep theo ngày tạo giảm dần
            var items = db.Orders.OrderByDescending(x => x.CreatedDate).ToList();

            // Xác định số trang hiện tại, mặc định là trang đầu tiên nếu không có trang nào được chọn
            if (page == null)
            {
                page = 1;
            }
            var pageNumber = page ?? 1;  // Trang hiện tại
            var pageSize = 10;  // Số lượng mục trên mỗi trang

            // Truyền th-tin về kích thước trang và trang hiện tại vào ViewBag
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;

            // Trả về view chứa danh sách đơn đặt hàng phân trang
            return View(items.ToPagedList(pageNumber, pageSize));
        }

      

        public ActionResult View(int id)
        {
            // Tìm kiếm đơn đặt hàng trong csdl dựa trên ID
            var item = db.Orders.Find(id);

            // Trả về view chứa thông tin chi tiết của đơn đặt hàng
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            // Lấy ds các chi tiết đơn đặt hàng từ csdl dựa trên ID đơn đặt hàng
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            // Trả về PartialView chứa danh sách các chi tiết đơn đặt hàng
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            // Tìm kiếm đơn đặt hàng trong csdl dựa trên ID
            var item = db.Orders.Find(id);
            // Kiểm tra xem đơn đặt hàng có tồn tại không
            if (item != null)
            {
                // Đính kèm đơn đặt hàng để theo dõi sự thay đổi
                db.Orders.Attach(item);
                // Cập nhật trạng thái thanh toán của đơn đặt hàng
                item.TypePayment = trangthai;
                // Đánh dấu thuộc tính TypePayment là đã thay đổi
                db.Entry(item).Property(x => x.TypePayment).IsModified = true;
                db.SaveChanges();
                // Trả về kết quả thông báo thành công
                return Json(new { message = "Success", Success = true });
            }
            return Json(new { message = "Unsuccess", Success = false });
        }

        public void ThongKe(string fromDate, string toDate)
        {
            var query = from o in db.Orders
                        join od in db.OrderDetails on o.Id equals od.OrderId
                        join p in db.Products on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.Price
                        };
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime start = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.CreatedDate >= start);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
                query = query.Where(x => x.CreatedDate < endDate);
            }
            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(r => new
            {
                Date = r.Key.Value,
                TotalBuy = r.Sum(x => x.OriginalPrice * x.Quantity), // tổng giá bán
                TotalSell = r.Sum(x => x.Price * x.Quantity) // tổng giá mua
            }).Select(x => new RevenueStatisticViewModel
            {
                Date = x.Date,
                Benefit = x.TotalSell - x.TotalBuy,
                Revenues = x.TotalSell
            });
        }
    }
}