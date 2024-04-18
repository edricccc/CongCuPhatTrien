using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using WebBanHangOnline.Models.Payments;

namespace WebBanHangOnline.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ShoppingCart
        public ActionResult Index()
        {
            // Lấy cart từ Session nếu tồn tại
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            // Kiểm tra xem giỏ hàng có tồn tại và có sp trong giỏ hàng không
            if (cart != null && cart.Items.Any())
            {
                // Nếu có, đặt giỏ hàng vào ViewBag để sử dụng trong View
                ViewBag.CheckCart = cart;
            }
            // Trả về View chính
            return View();
        }
        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                //Lấy chuỗi bí mật từ cấu hình ứng dụng (vnp_HashSecret).
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;

                //Tạo một đối tượng VnPayLibrary để xử lý dữ liệu truy vấn.
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                //Duyệt qua các tham số truy vấn và
                ////thêm vào đối tượng VnPayLibrary để chuẩn bị dữ liệu cho việc xác thực chữ ký số.
                //Kiểm tra tính hợp lệ của chữ ký số bằng cách sử dụng phương thức ValidateSignature.

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                //Nếu chữ ký số hợp lệ, tiến hành xử lý kết quả thanh toán.
                if (checkSignature)
                {
                    //Nếu mã phản hồi như dưới,đánh dấu đơn hàng trong cơ sở dữ liệu là đã thanh toán  
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = db.Orders.FirstOrDefault(x=>x.Code== orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.Status = 2;//đã thanh toán
                            db.Orders.Attach(itemOrder);
                            db.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                    ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            //var a = UrlPayment(0, "DH3574");
            return View();
        }
        public ActionResult CheckOut()
        {
            // Lấy giỏ hàng từ Session nếu tồn tại
            ShoppingCart cart = (ShoppingCart)Session["Cart"];

            // Kiểm tra xem giỏ hàng có tồn tại và có sản phẩm trong giỏ hàng không
            if (cart != null && cart.Items.Any())
            {
                // Nếu có, đặt giỏ hàng vào ViewBag để sử dụng trong View
                ViewBag.CheckCart = cart;
            }
            // Trả về View Checkout
            return View();
        }
        public ActionResult CheckOutSuccess()
        {
            // Trả về View cho trang thành công sau khi thanh toán
            return View();
        }
        public ActionResult Partial_Item_ThanhToan()
        {
            // Lấy giỏ hàng từ Session nếu tồn tại
            ShoppingCart cart = (ShoppingCart)Session["Cart"];

            // Kiểm tra xem giỏ hàng có tồn tại và có sp trong giỏ hàng không
            if (cart != null && cart.Items.Any())
            {
                // Nếu có, trả về Partial View chứa thông tin về các sptrong giỏ hàng
                return PartialView(cart.Items);
            }
            return PartialView();
        }

        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }


        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            // Kiểm tra xem giỏ hàng có tồn tại không
            if (cart != null)
            {
                // Trả về số lượng sp trong giỏ hàng dưới dạng JSON
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            // Nếu giỏ hàng không tồn tại, trả về số lượng là 0 dưới dạng JSON
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Partial_CheckOut()
        {
            // hiển thị thông tin trong trang thanh toán.
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Xử lý đơn đặt hàng khi khách hàng hoàn tất quá trình thanh toán.
        public ActionResult CheckOut(OrderViewModel req)
        {
            // Mã khởi tạo code để trả về kết quả
            var code = new { Success = false, Code = -1, Url = "" };
            // Kiểm tra xem ModelState có hợp lệ không
            if (ModelState.IsValid)
            {
                // Lấy giỏ hàng từ Session
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                // Kiểm tra xem giỏ hàng có tồn tại không
                if (cart != null)
                {

                    // Tạo đối tượng Order và
                    // gán thông tin từ req vào đối tượng Order
                    Order order = new Order();
                    order.CustomerName = req.CustomerName;
                    order.Phone = req.Phone;
                    order.Address = req.Address;
                    order.Email = req.Email;
                    order.Status = 1;//chưa thanh toán / 2/đã thanh toán, 3/Hoàn thành, 4/hủy

                    // Duyệt qua sp trong giỏ hàng và thêm vào đối tượng OrderDetails
                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        Price = x.Price
                    }));

                    // Tính tổng số tiền của đơn hàng
                    order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
                    order.TypePayment = req.TypePayment;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    order.CreatedBy = req.Phone;

                    // Tạo mã đơn hàng ngẫu nhiên
                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                    //order.E = req.CustomerName;

                    // Thêm đơn hàng vào cơ sở dữ liệu
                    db.Orders.Add(order);
                    db.SaveChanges();
                    //send mail cho khachs hang
                    var strSanPham = "";
                    var thanhtien = decimal.Zero;
                    var TongTien = decimal.Zero;
                    foreach (var sp in cart.Items)
                    {
                        strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.ProductName + "</td>";
                        strSanPham += "<td>" + sp.Quantity + "</td>";
                        strSanPham += "<td>" + WebBanHangOnline.Common.Common.FormatNumber(sp.TotalPrice, 0) + "</td>";
                        strSanPham += "</tr>";
                        thanhtien += sp.Price * sp.Quantity;
                    }
                    TongTien = thanhtien;
                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                    contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", req.Email);
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhtien, 0));
                    contentCustomer = contentCustomer.Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(TongTien, 0));
                    WebBanHangOnline.Common.Common.SendMail("ShopOnline", "Đơn hàng #" + order.Code, contentCustomer.ToString(), req.Email);

                    string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
                    contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
                    contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
                    contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
                    contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
                    contentAdmin = contentAdmin.Replace("{{Email}}", req.Email);
                    contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
                    contentAdmin = contentAdmin.Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhtien, 0));
                    contentAdmin = contentAdmin.Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(TongTien, 0));
                    WebBanHangOnline.Common.Common.SendMail("ShopOnline", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);

                    // Xóa toàn bộ sản phẩm trong giỏ hàng
                    cart.ClearCart();


                    // Thiết lập thông tin trả về khi thanh toán thành công
                    code = new { Success = true, Code = req.TypePayment, Url = "" };

                    //var url = "";

                    // Nếu là thanh toán online (req.TypePayment == 2)
                    if (req.TypePayment == 2)
                    {
                        // Lấy đường link thanh toán online
                        var url = UrlPayment(req.TypePaymentVN, order.Code);
                        code = new { Success = true, Code = req.TypePayment, Url = url };
                    }

                    //code = new { Success = true, Code = 1, Url = url };
                    //return RedirectToAction("CheckOutSuccess");
                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            // Mã khởi tạo code để trả về kết quả
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            
            // Khởi tạo đối tượng DbContext để tương tác với csdl
            var db = new ApplicationDbContext();

            // Kiểm tra sản phẩm có tồn tại trong csdl không
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
            if (checkProduct != null)
            {
                // Lấy giỏ hàng từ Session
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }

                // Tạo đối tượng ShoppingCartItem để thêm vào giỏ hàng
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.Id,
                    ProductName = checkProduct.Title,
                    CategoryName = checkProduct.ProductCategory.Title,
                    Alias = checkProduct.Alias,
                    Quantity = quantity
                };

                // Kiểm tra và gán hình ảnh mặc định của sản phẩm
                if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
                {
                    item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
                }

                // Gán giá sản phẩm (sử dụng giá sale nếu có)
                item.Price = checkProduct.Price;
                if (checkProduct.PriceSale > 0)
                {
                    item.Price = (decimal)checkProduct.PriceSale;
                }

                // Tính tổng giá của sản phẩm trong giỏ hàng
                item.TotalPrice = item.Quantity * item.Price;
                // Thêm sản phẩm vào giỏ hàng và cập nhật Session
                cart.AddToCart(item, quantity);
                Session["Cart"] = cart;
                // Thiết lập thông tin trả về khi thêm vào giỏ hàng thành công
                code = new { Success = true, msg = "Thêm sản phẩm vào giở hàng thành công!", code = 1, Count = cart.Items.Count };
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            // Lấy giỏ hàng từ Session
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
           // Kiểm tra xem giỏ hàng có tồn tại không
            if (cart != null)
            {
                // Gọi phương thức UpdateQuantity để cập nhật sl sp trong giỏ hàng
                cart.UpdateQuantity(id, quantity);

                // Trả về kết quả dưới dạng JSON khi cập nhật thành công
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // Mã khởi tạo code để trả về kết quả
            var code = new { Success = false, msg = "", code = -1, Count = 0 };

            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                // Kiểm tra xem sp có tồn tại trong giỏ hàng không
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    // Gọi phương thức Remove để xóa sp khỏi giỏ hàng
                    cart.Remove(id);
                    // Thiết lập thông tin trả về khi xóa thành công
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }



        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }



        #region Thanh toán vnpay
        public string UrlPayment(int TypePaymentVN, string orderCode)
        {
            var urlPayment = "";
            var order = db.Orders.FirstOrDefault(x => x.Code == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var Price = (long)order.TotalAmount * 100;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", Price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
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

            vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Code);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.Code); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            return urlPayment;
        }
        #endregion
    }
}