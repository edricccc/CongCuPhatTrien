using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class StatisticalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Statistical
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        //truy vấn và tính toán thống kê dữ liệu đơn hàng trong cơ sở dữ liệu.
        public ActionResult GetStatistical(string fromDate, string toDate)
        {
            //Dùng LINQ để tạo một câu truy vấn kết hợp thông tin từ
            ////bảng Orders, OrderDetails và Products.
            var query = from o in db.Orders
                        join od in db.OrderDetails
                        on o.Id equals od.OrderId
                        join p in db.Products
                        on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.OriginalPrice
                        };
            // Kiểm tra và lọc theo ngày bắt đầu nếu có
            if (!string.IsNullOrEmpty(fromDate))
            {
                //Chuyển đổi chuỗi fromDate thành kiểu dữ liệu DateTime sử dụng định dạng "dd/MM/yyyy".
                DateTime startDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);

                //Lọc các dữ liệu trong query để chỉ giữ lại những đơn hàng được tạo từ ngày startDate trở đi.
                query = query.Where(x => x.CreatedDate >= startDate);
            }

            // Kiểm tra và lọc theo ngày kết thúc nếu có
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.CreatedDate < endDate);
            }

            // Tính toán tổng số lượng mua, tổng doanh thu và tổng lợi nhuận theo ngày
            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(x => new
            {
                Date = x.Key.Value,
                TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                TotalSell = x.Sum(y => y.Quantity * y.Price),
            }).Select(x => new
            {
                Date = x.Date,
                DoanhThu = x.TotalSell,
                LoiNhuan = x.TotalSell - x.TotalBuy
            });
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

    }
}