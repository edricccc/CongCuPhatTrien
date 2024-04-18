using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class ProductImageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductImage
        public ActionResult Index(int id)
        {
            // Thiết lập dữ liệu ViewBag để truyền ProductId vào View
            ViewBag.ProductId = id;
            // Lấy danh sách hình ảnh của sản phẩm dựa trên ProductId
            var items = db.ProductImages.Where(x => x.ProductId == id).ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult AddImage(int productId,string url)
        {
            // Thêm một hình ảnh mới cho sản phẩm vào csdl
            db.ProductImages.Add(new ProductImage {
                //truyền vào 2 tham số là: ProductId, đường dẫn url
                ProductId = productId,
                Image=url,
                IsDefault=false
            });
            db.SaveChanges();
            return Json(new { Success=true});
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Tìm hình ảnh trong cơ sở dữ liệu dựa trên ID
            var item = db.ProductImages.Find(id);
            db.ProductImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }
    }
}