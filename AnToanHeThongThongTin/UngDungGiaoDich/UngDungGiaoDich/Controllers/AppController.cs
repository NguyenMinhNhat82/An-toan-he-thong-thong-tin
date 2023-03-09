using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using UngDungGiaoDich.Models;
using System.Data.Entity.Validation;
using UngDungGiaoDich.Commons;
using RestSharp;

namespace UngDungGiaoDich.Controllers
{
    public class AppController : Controller
    {

        UngDungGiaoDichEntities9 db = new UngDungGiaoDichEntities9();

        // GET: App
        public ActionResult Index()
        {
            
            return View();
        }
        public ActionResult ChungKhoan()
        {
            
            return View(db.CoPhieu.ToList());
        }
        public ActionResult Vang()
        {
            return View();
        }
        public ActionResult NhaDat()
        {
            return View();
        }
       

        

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(NguoiDung user)
        {
            var checkLogin = db.NguoiDung.Where(x => x.TenTK.Equals(user.TenTK) && x.Password.Equals(user.Password)).ToList();
            if (checkLogin != null)
            {
                Session["UserID"] = checkLogin.FirstOrDefault().ID;
                Session["UserName"] = checkLogin.FirstOrDefault().Name;
     
                return RedirectToAction("Index", "App");
            }
            else {
                ViewBag.Message = "Tên đăng nhập hoặc mật khẩu sai";
            }
                
            return View();
        }
        [HttpPost]
        //Chua sử lý bán hay mua quá số lượng phiếu phát hành
        public ActionResult SignUp(NguoiDung user) {
            
            if (db.NguoiDung.Any(x => x.TenTK == user.TenTK))
            {
                ViewBag.Message = "Tên tài khoản đã tồn tại";
            }
            else {
                user.Cash = 0;
                user.Avatar = "https://res.cloudinary.com/dexbjwfjg/image/upload/v1671517934/dqromcjzfkhg4g2vr73i.jpg";
                db.NguoiDung.Add(user);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index", "App");
                }
                catch(DbEntityValidationException E) {
                    Console.WriteLine(E);
                }
                
            }
            Console.WriteLine();
            return View();
        }

        public ActionResult LogOut() {
            Session.Clear();
            return RedirectToAction("Login", "App");
        }

        public ActionResult DatLenh() {
            if (Session["UserID"] != null)
                return View();
            else
                return RedirectToAction("Login", "App");
        }

        [HttpPost]
        public ActionResult DatLenh(LenhGiaoDich lenhGiaoDich) {
            var cophieu = db.CoPhieu.Where(x => x.Ten == lenhGiaoDich.TenCoPhieu).ToList().FirstOrDefault();
            var IDUSer = int.Parse(Session["UserID"].ToString());
            string strConnect = "Data Source=DESKTOP-FSP9BUJ;Initial Catalog=UngDungGiaoDich;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework";
            SqlConnection cnn = new SqlConnection(strConnect);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = cnn;
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }
            
           
           
            if (lenhGiaoDich.TheLoai == "Bán") {
                var CheckcoPhieu = db.User_CoPhieu.Where(x => x.IDChuSoHuu == IDUSer
                && x.IDCoPhieu == cophieu.ID).ToList();
               
                cmd.CommandText = "select SoLuong from UngDungGiaoDich.dbo.User_CoPhieu where IDChuSoHuu ="+IDUSer+" and IDCoPhieu = "+cophieu.ID;
                cmd.Connection = cnn;
                var soLuong = cmd.ExecuteScalar();
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                if (cophieu != null && CheckcoPhieu != null )
                {
#pragma warning restore CS0472 
                        if (double.Parse((lenhGiaoDich.Gia).ToString()) <= cophieu.GiaHigh
                        && double.Parse((lenhGiaoDich.Gia).ToString()) >= cophieu.GiaLow)
                    {
                        if (lenhGiaoDich.TongSoCoPhieu > 0 && lenhGiaoDich.TongSoCoPhieu <= (int)soLuong)
                        {
                            var LenhMua1 = db.LenhGiaoDich.Where(x => x.IdCoPhieu == cophieu.ID && x.Gia == lenhGiaoDich.Gia && x.KetQua == false && x.TheLoai.Equals("Mua") && x.IDUser != IDUSer).ToList().FirstOrDefault();
                            if (LenhMua1 != null)
                            {
                                var TongCoPhieu = lenhGiaoDich.TongSoCoPhieu;
                                while(TongCoPhieu != 0) {
                                    var LenhMua = db.LenhGiaoDich.Where(x => x.IdCoPhieu == cophieu.ID && x.Gia == lenhGiaoDich.Gia && x.KetQua == false && x.TheLoai.Equals("Mua") && x.IDUser != IDUSer).ToList().FirstOrDefault();

                                    if (TongCoPhieu > LenhMua.TongSoCoPhieu)
                                    {
                                        cmd.CommandText = "update NguoiDung set Cash = Cash + @tien6 where ID =@id6";
                                        cmd.Parameters.Add("@tien6", SqlDbType.Decimal).Value = LenhMua.Gia * LenhMua.TongSoCoPhieu;
                                        cmd.Parameters.Add("@id6", SqlDbType.Int).Value = IDUSer;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "update LenhGiaoDich set KetQua = 1 where ID = " + LenhMua.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "update NguoiDung set Cash = Cash - @tien1 where ID =@id1";
                                        cmd.Parameters.Add("@tien1", SqlDbType.Decimal).Value = LenhMua.Gia * LenhMua.TongSoCoPhieu;
                                        cmd.Parameters.Add("@id1", SqlDbType.Int).Value = LenhMua.IDUser;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong + " + LenhMua.TongSoCoPhieu + " where IDChuSoHuu = " + LenhMua.IDUser + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong - " + LenhMua.TongSoCoPhieu + " where IDChuSoHuu = " + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        TongCoPhieu -= LenhMua.TongSoCoPhieu;
                                    }
                                    else if (TongCoPhieu < LenhMua.TongSoCoPhieu)
                                    {
                                        cmd.CommandText = "update NguoiDung set Cash = Cash + @tien4 where ID =@id4";
                                        cmd.Parameters.Add("@tien4", SqlDbType.Decimal).Value = LenhMua.Gia * TongCoPhieu;
                                        cmd.Parameters.Add("@id4", SqlDbType.Int).Value = IDUSer;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "Update NguoiDung set Cash = Cash -@a where ID = @b";
                                        cmd.Parameters.Add("@a", SqlDbType.Decimal).Value = LenhMua.Gia * TongCoPhieu;
                                        cmd.Parameters.Add("@b", SqlDbType.Int).Value = LenhMua.IDUser;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "update LenhGiaoDich set TongSoCoPhieu = " + (LenhMua.TongSoCoPhieu - TongCoPhieu) + "where ID =" + LenhMua.ID ;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong + " + TongCoPhieu + " where IDChuSoHuu = " + LenhMua.IDUser + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong - " + TongCoPhieu + " where IDChuSoHuu = " + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        ViewBag.Succes = "Đặt lệnh thành công";
                                        break;
                                    }
                                    else {
                                        cmd.CommandText = "update NguoiDung set Cash = Cash + @tien3 where ID =@id3";
                                        cmd.Parameters.Add("@tien3", SqlDbType.Decimal).Value = LenhMua.Gia * LenhMua.TongSoCoPhieu;
                                        cmd.Parameters.Add("@id3", SqlDbType.Int).Value = IDUSer;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "Update NguoiDung set Cash = Cash -@a where ID = @b";
                                        cmd.Parameters.Add("@a", SqlDbType.Decimal).Value = LenhMua.Gia * LenhMua.Gia;
                                        cmd.Parameters.Add("@b", SqlDbType.Int).Value = LenhMua.IDUser;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "update LenhGiaoDich set KetQua = 1 where ID = " + LenhMua.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong + " + TongCoPhieu + " where IDChuSoHuu = " + LenhMua.IDUser + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong - " + TongCoPhieu + " where IDChuSoHuu = " + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        ViewBag.Succes = "Đặt lệnh thành công";
                                        break;
                                    }
                                }
                                cnn.Close();
                            }
                            else
                            {
                                lenhGiaoDich.IDUser = IDUSer;
                                lenhGiaoDich.Ngay = System.DateTime.Now;
                                lenhGiaoDich.IdCoPhieu = cophieu.ID;
                                lenhGiaoDich.KetQua = false;
                                db.LenhGiaoDich.Add(lenhGiaoDich);
                                cmd.CommandText = "update User_CoPhieu set SoLuong = SoLuong - " + lenhGiaoDich.TongSoCoPhieu + "where IDChuSoHuu =" + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                cmd.ExecuteNonQuery();
                                cnn.Close();
                                db.SaveChanges();
                                return RedirectToAction("ChungKhoan", "App");
                            }
                        }
                        else ViewBag.erro = "Bạn đặt bán số lượng quá số lượng bạn có";
                    }
                    else
                        ViewBag.erro = "Giá bạn đặt lệnh lớn giá trần hoặc nhỏ hơn giá sàn";
                }
                else
                    ViewBag.erro = "Cổ phiếu bạn đặt ko hợp lý";
    
            }
            else if (lenhGiaoDich.TheLoai == "Mua")
            {
                if (double.Parse((lenhGiaoDich.Gia).ToString()) <= cophieu.GiaHigh
                    && double.Parse((lenhGiaoDich.Gia).ToString()) >= cophieu.GiaLow && cophieu != null) {
                    var checkGia = db.LenhGiaoDich.Where(x => x.Gia == lenhGiaoDich.Gia && x.TheLoai.Equals("Bán")).ToList();
                    if (checkGia != null)
                    {
                        var nguoiMua = db.NguoiDung.Where(x => x.ID == IDUSer).ToList().FirstOrDefault();
                        if (nguoiMua.Cash >= lenhGiaoDich.TongSoCoPhieu * lenhGiaoDich.Gia)
                        {
                            var tongCoPhieu = lenhGiaoDich.TongSoCoPhieu;
                            while (tongCoPhieu != 0)
                            {
                                var LenhBan = db.LenhGiaoDich.Where(x => x.IdCoPhieu == cophieu.ID && x.Gia == lenhGiaoDich.Gia && x.KetQua == false && x.TheLoai.Equals("Bán") && x.IDUser != IDUSer).ToList().FirstOrDefault();
                                int SoCoPhieuNhap = (int)(lenhGiaoDich.TongSoCoPhieu);
                                if (LenhBan != null)
                                {

                                    int CoPhieuBan = (int)LenhBan.TongSoCoPhieu;
                                    if (SoCoPhieuNhap > CoPhieuBan)
                                    {
                                        cmd.CommandText = "update NguoiDung set Cash = Cash + @tien where ID =@id";
                                        cmd.Parameters.Add("@tien", SqlDbType.Decimal).Value = LenhBan.Gia * LenhBan.TongSoCoPhieu;
                                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = LenhBan.IDUser;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "update LenhGiaoDich set KetQua = 1 where ID = " + LenhBan.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "update NguoiDung set Cash = Cash - @tien1 where ID =@id1";
                                        cmd.Parameters.Add("@tien1", SqlDbType.Decimal).Value = LenhBan.Gia * LenhBan.TongSoCoPhieu;
                                        cmd.Parameters.Add("@id1", SqlDbType.Int).Value = IDUSer;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong + " + CoPhieuBan + " where IDChuSoHuu = " + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong - " + CoPhieuBan + " where IDChuSoHuu = " + LenhBan.IDUser + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();

                                        tongCoPhieu -= LenhBan.TongSoCoPhieu;

                                    }
                                    else if (tongCoPhieu < LenhBan.TongSoCoPhieu)
                                    {

                                        cmd.CommandText = "update NguoiDung set Cash = Cash + @tien4 where ID =@id4";
                                        cmd.Parameters.Add("@tien4", SqlDbType.Decimal).Value = LenhBan.Gia * tongCoPhieu;
                                        cmd.Parameters.Add("@id4", SqlDbType.Int).Value = LenhBan.IDUser;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update NguoiDung set Cash = Cash -@a where ID = @b";
                                        cmd.Parameters.Add("@a", SqlDbType.Decimal).Value = tongCoPhieu* LenhBan.Gia;
                                        cmd.Parameters.Add("@b", SqlDbType.Int).Value = IDUSer;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "update LenhGiaoDich set TongSoCoPhieu = " + (LenhBan.TongSoCoPhieu - tongCoPhieu) + "where ID =" + LenhBan.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong + " + tongCoPhieu + " where IDChuSoHuu = " + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong - " + tongCoPhieu + " where IDChuSoHuu = " + LenhBan.IDUser + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();

                                        ViewBag.Succes = "Đặt lệnh thành công";
                                        break;

                                    }
                                    else
                                    {
                                        cmd.CommandText = "update NguoiDung set Cash = Cash + @tien3 where ID =@id3";
                                        cmd.Parameters.Add("@tien3", SqlDbType.Decimal).Value = LenhBan.Gia * LenhBan.TongSoCoPhieu;
                                        cmd.Parameters.Add("@id3", SqlDbType.Int).Value = LenhBan.IDUser;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "Update NguoiDung set Cash = Cash -@a where ID = @b";
                                        cmd.Parameters.Add("@a", SqlDbType.Decimal).Value = LenhBan.Gia * LenhBan.Gia;
                                        cmd.Parameters.Add("@b", SqlDbType.Int).Value = IDUSer;
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        cmd.CommandText = "update LenhGiaoDich set KetQua = 1 where ID = " + LenhBan.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong + " + tongCoPhieu + " where IDChuSoHuu = " + IDUSer + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();
                                        cmd.CommandText = "Update User_CoPhieu set SoLuong = SoLuong - " + tongCoPhieu + " where IDChuSoHuu = " + LenhBan.IDUser + " and IDCoPhieu = " + cophieu.ID;
                                        cmd.ExecuteNonQuery();


                                        ViewBag.Succes = "Đặt lệnh thành công";
                                        break;

                                    }
                                }
                                else
                                {
                                    lenhGiaoDich.TheLoai = "Bán";
                                    lenhGiaoDich.TongSoCoPhieu = SoCoPhieuNhap;
                                    lenhGiaoDich.IDUser = IDUSer;
                                    lenhGiaoDich.Ngay = System.DateTime.Now;
                                    lenhGiaoDich.IdCoPhieu = cophieu.ID;
                                    lenhGiaoDich.KetQua = false;
                                    db.LenhGiaoDich.Add(lenhGiaoDich);
                                    db.SaveChanges();
                                    break;
                                }
                            }
                            cnn.Close();


                        }
                        else {
                            ViewBag.erro = "Bạn không đủ tiền để mua";
                            return View();
                        }

                    }
                    else { ViewBag.erro = "Không có loại cổ phiếu bạn muốn mua"; }
                       
                    }
                else
                {
                    var LenhBan = db.LenhGiaoDich.Where(x => x.IdCoPhieu == cophieu.ID && x.Gia == lenhGiaoDich.Gia && x.KetQua == false && x.TheLoai.Equals("Bán") && x.IDUser != IDUSer).ToList().FirstOrDefault();
                    cmd.CommandText = "Update NguoiDung set Cash = Cash -@a where ID = @b";
                                        cmd.Parameters.Add("@a", SqlDbType.Decimal).Value = LenhBan.Gia;
                                        cmd.Parameters.Add("@b", SqlDbType.Int).Value = IDUSer;
                    cmd.ExecuteNonQuery();
                    cnn.Close();
                    lenhGiaoDich.IDUser = IDUSer;
                    lenhGiaoDich.Ngay = System.DateTime.Now;
                    lenhGiaoDich.IdCoPhieu = cophieu.ID;
                    lenhGiaoDich.KetQua = false;
                    db.LenhGiaoDich.Add(lenhGiaoDich);
                    db.SaveChanges();
                    ViewBag.Succes = "Đặt lệnh thành công";
                }
            }

            else { ViewBag.erro = "Giá bạn đặt lệnh lớn giá trần hoặc nhỏ hơn giá sàn"; }
            return View();
        }
    }
}