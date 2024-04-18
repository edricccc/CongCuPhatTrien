using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin,Employee")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/News
        public ActionResult Index(string Searchtext, int? page)
        {
            // Định kích thước trang
            var pageSize = 10;

            // Thiết lập trang mặc định nếu không có trang được chọn
            if (page == null)
            {
                page = 1;
            }
            // Lấy danh sách tin tức từ cơ sở dữ liệu, sắp xếp theo ID giảm dần
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id);
            // Kiểm tra xem có yêu cầu tìm kiếm hay không
            if (!string.IsNullOrEmpty(Searchtext))
            {
                // Lọc tin tức dựa trên Alias hoặc Tiêu đề chứa đoạn văn bản tìm kiếm
                items = items.Where(x=>x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }

            // Xác định số trang và lấy dữ liệu cho trang hiện tại
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);

            // Truyền dữ liệu cho view thông qua ViewBag
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;

            // Trả về view với danh sách tin tức đã được phân trang và lọc
            return View(items);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(News model)
        {
            // Kiểm tra xem dữ liệu nhập vào có hợp lệ không
            if (ModelState.IsValid)
            {
                // Thiết lập ngày tạo và ngày sửa mới cho tin tức
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;

                // Gán CategoryId cố định (trong ví dụ là 3)
                model.CategoryId = 3;

                // Tạo Alias cho tin tức từ tiêu đề sử dụng hàm FilterChar trong Filter
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);

                // Thêm tin tức mới vào cơ sở dữ liệu
                db.News.Add(model);
                //db.News.Attach(model);
                //db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);

                // Gắn mô hình tin tức vào context để theo dõi sự thay đổi
                db.News.Attach(model);
                // Đánh dấu mô hình tin tức là đã sửa đổi
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.News.Find(id);
            if (item != null)
            {
                db.News.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.News.Find(id);
            // Kiểm tra xem tin tức có tồn tại không
            if (item != null)
            {
                // Đảo ngược trạng thái kích hoạt (IsActive)
                item.IsActive = !item.IsActive;
                // Đánh dấu tin tức là đã sửa đổi
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                // Trả về kết quả JSON là thay đổi thành công và trạng thái mới của tin tức 
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            // Kiểm tra xem chuỗi ID có giá trị không rỗng
            if (!string.IsNullOrEmpty(ids))
            {
                // Tách chuỗi IDs thành một mảng các ID
                var items = ids.Split(',');

                // Kiểm tra xem mảng có tồn tại và có phần tử không
                if (items != null && items.Any())
                {
                    // Duyệt qua từng ID trong mảng và xóa tin tức tương ứng
                    foreach (var item in items)
                    {
                        // Tìm tin tức trong cơ sở dữ liệu dựa trên ID
                        var obj = db.News.Find(Convert.ToInt32(item));
                        
                        db.News.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

    }
}