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
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Products
        public ActionResult Index(int? page)
        {
            // Lấy ds sp từ csdl và sắp xếp theo ID giảm dần
            IEnumerable<Product> items = db.Products.OrderByDescending(x => x.Id);
            // Đặt kích thước trang là 10 sản phẩm
            var pageSize = 10;
            // Kiểm tra trang hiện tại, nếu không có thì mặc định là trang đầu tiên
            if (page == null)
            {
                page = 1;
            }

            // Chuyển đổi giá trị trang thành kiểu số nguyên
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

            // Sử dụng PagedList để phân trang ds sp
            items = items.ToPagedList(pageIndex, pageSize);
            // Truyền thông tin phân trang vào ViewBag để sử dụng trong View
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    //Duyệt qua ds ảnh và thiết lập ảnh mặc định dựa trên chỉ số từ ds rDefault.
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }
                //Thiết lập ngày tạo và ngày sửa đổi của sản phẩm.
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                //Kiểm tra và thiết lập giá trị mặc định cho các trường SeoTitle và Alias nếu chúng trống.
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                //Thêm sản phẩm vào csdl và lưu thay đổi.
                db.Products.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //Truy xuất ds danh mục sp để hiển thị trong DropDownList.
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            // Truy xuất danh sách danh mục sản phẩm để hiển thị trong DropDownList
            ViewBag.ProductCategory = new SelectList(db.ProductCategories.ToList(), "Id", "Title");
            // Lấy thông tin sp cần chỉnh sửa từ csdl dựa trên ID
            var item = db.Products.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.Products.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Tìm kiếm sp cần xóa dựa trên ID
            var item = db.Products.Find(id);
            if (item != null)
            {
                // Kiểm tra và xóa các hình ảnh của sp
                var checkImg = item.ProductImage.Where(x => x.ProductId == item.Id);
                if (checkImg != null)
                {
                    foreach(var img in checkImg)
                    {
                        db.ProductImages.Remove(img);
                        db.SaveChanges();
                    }
                }
                db.Products.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                // Đảo ngược trạng thái kích hoạt
                item.IsActive = !item.IsActive;
                // Cập nhật trạng thái mới vào cơ sở dữ liệu
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult IsHome(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsHome = !item.IsHome;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsHome = item.IsHome });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsSale(int id)
        {
            var item = db.Products.Find(id);
            if (item != null)
            {
                item.IsSale = !item.IsSale;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, IsSale = item.IsSale });
            }

            return Json(new { success = false });
        }
    }
}