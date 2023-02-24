using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace FoodOnline.Controllers
{
    public class UsersController : Controller
    {
        FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();
        // GET: Users
        public ActionResult Index()
        {
            return View();
        }
        public bool CheckRole(string type)
        {
            User user = Session["User"] as User;
            if(user.Role.NameRole== type)
            {
                return true;
            }
            return false;
        }
        public void SentMail(string title,string ToEmail, string FromEmail, string password, string content)
        {
            //string passHash = GetMD5(password);
            MailMessage mail = new MailMessage();
            mail.To.Add(ToEmail);
            mail.From = new MailAddress(ToEmail);
            mail.Subject = title;
            mail.Body = content;
            mail.IsBodyHtml= true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host="smtp.gmail.com";
            smtp.Port = 587;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(FromEmail, password);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
        [HttpGet]
        public ActionResult ConfirmEmail(int ID) {
            User user = db.Users.SingleOrDefault(u=>u.IdUser == ID);
            if (user.IsComfirm.Value)
            {
                ViewBag.Message = "Email Confirmed";
                return View();
            }
            string urlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");
            ViewBag.Email = "Access to Email to verify account: " + user.Email;
            SentMail("Mã xác minh tài khoản", user.Email, "duykhanh18102002@gmail.com", "ytlipmoseyimohec", "Xác minh nhanh bằng cách click vào link: " 
                + urlBase + "Users/ConfirmEmailLink/" + ID + "?Captcha=" + user.Captcha + "</div>");
            return View();
        }
        [HttpGet]
        public ActionResult ConfirmEmailLink(int ID, string captcha) {
            User user = db.Users.SingleOrDefault(u => u.IdUser == ID && u.Captcha == captcha);
            if (user != null)
            {
                user.IsComfirm = true;
                db.SaveChanges();
                ViewBag.Message = "Account Verification Successful";
                return View();
            }
            ViewBag.Message = "\r\nAccount Verification Failed";
            return View();
        }

        [HttpGet]
        public ActionResult ConfirmForgotPassword(int ID)
        {
            User user = db.Users.SingleOrDefault(u => u.IdUser == ID);
            if (user.fg_otp.Length>0)
            {
                ViewBag.Message = "Email Confirmed";
                return View();
            }
            string urlBase = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~");
            ViewBag.Email = "<div style=\"color:#fff\">\r\nAccess to Email to verify account: " + user.Email;
            SentMail("Mã xác minh tài khoản", user.Email, "duykhanh18102002@gmail.com", "ytlipmoseyimohec", "Reset password bằng cách click vào link: "
                + urlBase + "Users/ResetPassword/?fg_otp=" + user.fg_otp + "&email="+user.Email+ "</div>");
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(int ID, string fgOtp)
        {
            User us = db.Users.SingleOrDefault(x=>x.IdUser== ID && x.fg_otp == fgOtp);
            if (us != null)
            {
                us.Password = GetMD5(us.Password);
                db.SaveChanges();
                ViewBag.Message = "Reset Password successful";
                return View();
            }
            ViewBag.Message = "\r\nReset Password failed";
            return View();
        }

        [HttpGet]
        public ActionResult ProfileUser(int? userID)
        {
            if(userID == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                var user = db.Users.SingleOrDefault(x => x.IdUser == userID);
                return View(user);
            }
        }

        [HttpPost]
        public ActionResult ProfileUser(User user, FormCollection form)
        {
            int userID = (int)user.IdUser;
            var updateUser = db.Users.SingleOrDefault(x => x.IdUser == userID);
            var passHash = GetMD5(user.Password);
            //var updatePass = GetMD5(updateUser.Password);
            updateUser.NameUser = user.NameUser;
            updateUser.Address = user.Address;
            updateUser.Phone = user.Phone;
            updateUser.Password = passHash;
            db.SaveChanges(); return View(user);
            //var passHash = GetMD5(user.Password);
            //if (passHash == updateUser.Password)
            //{
            //    if (form["txtRepassword"] == null || form["txtRepassword"] == "")
            //    {
            //        ViewBag.Message = "Update success";
            //        return View(user);
            //    }
            //    else
            //    {
            //        ViewBag.Message = "Do not input comfirm password if you do not change password";
            //        return View(user);
            //    }
            //}
            //else
            //{
            //    if (user.Password == form["txtRepassword"])
            //    {
            //        updateUser.Password = GetMD5(user.Password);
            //        db.SaveChanges();
            //        ViewBag.Message = "Update success";
            //        return View(user);
            //    }
            //    else
            //    {
            //        ViewBag.Message = "Password and Confirm Password are not match";
            //        return View(user);
            //    }
            //}
        }

        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase file,int IdUser)
        {
            int userID = (int)IdUser;
            try
            {
                if (file.ContentLength > 0)
                {
                    string _fileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/Images"), _fileName);
                    file.SaveAs(_path);
                    var user = db.Users.SingleOrDefault(x => x.IdUser == userID);
                    if (user != null)
                    {
                        string filename = (string)file.FileName;
                        user.Img = filename;
                        db.SaveChanges();
                    }
                }
                ViewBag.Message = "Upload Success";
                return RedirectToAction("ProfileUser", new { userID = IdUser });
            }
            catch
            {
                ViewBag.Message = "Upload Fail";
                return RedirectToAction("ProfileUser", new { userID = IdUser });
            }
        }

        public ActionResult ChangePass()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(string Password, string newPassword, string Confirmpwd)
        {
            User objadmin = new User();
            string ad = Session["Name"].ToString();
            int id = int.Parse(Session["User_Id"].ToString());
            var login = db.Users.Where(u => u.NameUser.Equals(ad) && u.IdUser.Equals(id)).FirstOrDefault();
            var f_pass = GetMD5(Password);
            if (login.Password == f_pass)
            {
                if (Confirmpwd == newPassword)
                {
                    //login.ConfirmPassword = GetMD5(Confirmpwd);
                    login.Password = GetMD5(newPassword);
                    var str = GetMD5(newPassword);
                    db.SaveChanges();
                    ViewBag.Message = "Password has been changed successfully !!!";
                }
                else
                {
                    ViewBag.Message = "New password match !!! Please check !!!";
                }
            }
            else
            {
                ViewBag.Message = "Old password not match !!! Please check !!!";                
            }
            return View();
        }

        public static string GetMD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.UTF8.GetBytes(str);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x2");

            }
            return byte2String;
        }
    }
}