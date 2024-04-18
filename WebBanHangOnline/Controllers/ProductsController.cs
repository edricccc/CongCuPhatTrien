using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public ActionResult Index()
        {
            // Lấy tất cả sản phẩm từ cơ sở dữ liệu
            var items = db.Products.ToList();
            
            return View(items);
        }

        public ActionResult Detail(string alias,int id)
        {
            // Tìm sp  trong csdl dựa trên ID
            var item = db.Products.Find(id);
            // Nếu sản phẩm tồn tại
            if (item != null)
            {
                // Gắn kết sản phẩm vào context của cơ sở dữ liệu
                db.Products.Attach(item);

                // Tăng số lần xem của sp lên 1
                item.ViewCount = item.ViewCount + 1;
                // Đặt thuộc tính ViewCount là đã thay đổi để cập nhật vào csdl
                db.Entry(item).Property(x => x.ViewCount).IsModified = true;
                db.SaveChanges();
            }
            
            return View(item);
        }
        public ActionResult ProductCategory(string alias,int id)
        {
            // Lấy tất cả sản phẩm từ cơ sở dữ liệu
            var items = db.Products.ToList();

            // Nếu có ID danh mục được chỉ định, lọc ds sptheo ID đó
            if (id > 0)
            {
                items = items.Where(x => x.ProductCategoryId == id).ToList();
            }
            // Lấy thông tin danh mục từ csdl  dựa trên ID
            var cate = db.ProductCategories.Find(id);
            // Nếu danh mục tồn tại, đặt tên danh mục vào ViewBag để sử dụng trong View
            if (cate != null)
            {
                ViewBag.CateName = cate.Title;
            }

            // Đặt ID danh mục vào ViewBag để sử dụng trong View
            ViewBag.CateId = id;
            return View(items);
        }

        public ActionResult Partial_ItemsByCateId()
        {
            var items = db.Products.Where(x => x.IsHome && x.IsActive).Take(12).ToList();
            return PartialView(items);
        }

        public ActionResult Partial_ProductSales()
        {
            var items = db.Products.Where(x => x.IsSale && x.IsActive).Take(12).ToList();
            return PartialView(items);
        }
    }
}