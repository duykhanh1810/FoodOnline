using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using FoodOnline.Models;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;

namespace FoodOnline.Controllers
{
    public class HomeController : Controller
    {
        FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            User check = db.Users.SingleOrDefault(u => u.Email == user.Email);
            if (check != null)
            {
                ViewBag.Message = "Email already exists";
                return View();
            }

            User userAdd = new User();
            string pass = GetMD5(user.Password);
            try
            {
                user.Captcha = new Random().Next(100000, 999999).ToString();
                user.IsComfirm= false;
                user.IdRole= 3;
                user.Status = true;
                user.Img = "pr.jpg";
                user.Password = pass;
                userAdd = db.Users.Add(user);
                db.SaveChanges();
            }catch(Exception ex)
            {
                ViewBag.Message = "Sign up failed " +ex.Message;
                return View();
            }
            return RedirectToAction("ConfirmEmail", "Users", new { ID = userAdd.IdUser });
        }
        public ActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignIn(string email, string password)
        {
            string passHash = GetMD5(password);
            User check = db.Users.FirstOrDefault(u => u.Email == email && u.Password == passHash);
            User wrongEmail = db.Users.FirstOrDefault(u => u.Email != email);
            User wrongPass = db.Users.FirstOrDefault(u => u.Email == email && u.Password != passHash);
            //var check2 = from u in db.Users
            //             join r in db.Roles on u.IdRole equals r.IdRole
            //             where (u.Email == email && u.Password == passHash)
            //             select new
            //             {
            //                 u,r,
            //                 NameUs = u.NameUser,
            //                 Rolesss = r.NameRole,
            //                 idu = u.IdUser,
            //             };
            //if (check2 != null)
            //{
            //    Session["User"] = check2;
            //    Session["Roles"] = check2.Select(x=>x.Rolesss);
            //    Session["User_Id"] = check2.Select(x => x.idu).ToString();
            //    Session["Name"] = check2.Select(x => x.NameUs);
            //    return RedirectToAction("Index", "Home");
            //}
            if (check != null)
            {
                Session["User"] = check;
                Session["Roles"] = check.Role.NameRole;
                Session["User_Id"] = check.IdUser;
                Session["Name"] = check.NameUser;
                return RedirectToAction("Index", "Home");
            }
            else if(wrongEmail!= null)
            {
                ViewBag.Message = "Email does not exist";
                return View();
            }else if(wrongPass != null)
            {
                ViewBag.Message = "Wrong password";
                return View();
            }
            return View();
        }
        public ActionResult SignOut()
        {
            Session.Remove("User");
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(User user)
        {
            User check = db.Users.SingleOrDefault(u => u.Email == user.Email);
            if (check == null)
            {
                ViewBag.Message = "Email does not exist";
                return View();
            }
            try
            {
                check.fg_otp = new Random().Next(100000, 999999).ToString();
                Session["fg_Name"] = check.NameUser;
                Session["fg_id"] = check.IdUser;
                Session["fg_otp"] = check.fg_otp;
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Failed to send email " + ex.Message;
                return View();
            }
            return RedirectToAction("ConfirmForgotPassword", "Users", new { ID = check.IdUser });           
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