using FoodOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FoodOnline.Controllers
{
    public class ChatController : Controller
    {
        FoodOrdersOnlineEntities db = new FoodOrdersOnlineEntities();
        public bool CheckRole(string type)
        {
            Models.User user = Session["Users"] as Models.User;
            if (user != null && user.NameUser == type)
            {
                return true;
            }
            return false;
        }
        public ActionResult Index()
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            User user = Session["User"] as User;
            IEnumerable<User> listUser = db.Users.ToList();
            List<Message> messages = new List<Message>();
            foreach (User item in listUser)
            {
                Message message = db.Messages.Where(x => x.FromIdUser == item.IdUser && x.FromIdUser != user.IdUser).ToList().LastOrDefault();
                if(message != null)
                {
                    messages.Add(message);
                }
            }
            return View(messages);
        }
        public ActionResult Chating(int WithUserID, int MessageID = 0)
        {
            if (CheckRole("Admin"))
            {

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            IEnumerable<Message> listMessage;
            if(MessageID != 0)
            {
                Message message = db.Messages.Find(MessageID);
                if (!message.Send.Value)
                {
                    message.Send = true;
                    db.SaveChanges();
                }
            }
            listMessage = db.Messages.Where(x => x.FromIdUser == WithUserID || x.ToIdUser == WithUserID).OrderBy(x => x.CreateDate).ToList();

            ViewBag.UserFullName = db.Users.Find(WithUserID).NameUser;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetAllMessageChating(int UserID)
        {
            try
            {
                IEnumerable<Message> listMessage1 = db.Messages.Where(x => x.FromIdUser == UserID || x.ToIdUser == UserID).OrderBy(x => x.CreateDate).ToList();
                var listMessage = listMessage1.Select(x =>
                new
                {
                    IDMessage = x.IdMessage,
                    FromIdUser = x.FromIdUser,
                    ToIdUser = x.ToIdUser,
                    Content = x.Content,
                    CreateDate = x.CreateDate,
                    FromUserName = x.User.NameUser
                });
                return Json(listMessage, JsonRequestBehavior.AllowGet);
            }
            catch(Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public JsonResult Send(int FromUserID, int ToUserID, string Content, string Side)
        {
            if(Side == "Client")
            {
                Message message1 = db.Messages.ToList().LastOrDefault();
                if(message1 != null && !message1.Send.Value)
                {
                    message1.Send = true;
                    db.SaveChanges();
                }
                Message message = new Message();
                message.Send = false;
                message.FromIdUser = FromUserID;
                message.ToIdUser = ToUserID;
                message.Content = Content;
                message.CreateDate = DateTime.Now;
                
                db.Messages.Add(message);
                db.SaveChanges();
                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet); 
            }
            else
            {
                Message message = new Message();

                message.FromIdUser = FromUserID;
                message.ToIdUser = ToUserID;
                message.Content = Content;
                message.CreateDate = DateTime.Now;
                message.Send = true;

                db.Messages.Add(message);
                db.SaveChanges();
                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public JsonResult GetNotiMessage()
        {
            User user = Session["User"] as User;
            try
            {
                var listMessage = db.Messages.Where(x => x.Send == false && x.FromIdUser != user.IdUser).ToList().Select(x => new { Id = x.IdMessage, FromUserID = x.FromIdUser, FromUserName = x.User.NameUser, CreatedDate = (DateTime.Now - x.CreateDate.Value).Minutes });
                return Json(listMessage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
}