using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
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
            ViewBag.Email = "\r\nAccess to Email to verify account: " + user.Email;
            SentMail("Mã xác minh tài khoản", user.Email, "duykhanh18102002@gmail.com", "ytlipmoseyimohec", "Xác minh nhanh bằng cách click vào link: " 
                + urlBase + "Users/ConfirmEmailLink/" + ID + "?Captcha=" + user.Captcha + "</p>");
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