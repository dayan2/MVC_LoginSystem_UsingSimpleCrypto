using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Mvc4_Login.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.User user)
        {
            if (ModelState.IsValid)
            {
                if (IsValid(user.Email, user.Password))
                {
                    FormsAuthentication.SetAuthCookie(user.Email, false);
                    return RedirectToAction("Index", "User");
                }
                else
                    ModelState.AddModelError("","Login details are incorrect");
            }
            return View(user);
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(Models.User user)
        {
            if (ModelState.IsValid)
            {
                using (var con = new MainDBContext())
                {
                    var crypto = new SimpleCrypto.PBKDF2();
                    var pwd = crypto.Compute(user.Password);
                    var sysuser = con.Users.Create();

                    sysuser.Email = user.Email;
                    sysuser.Password = pwd;
                    sysuser.PasswordSalt = crypto.Salt;
                    sysuser.Id = Guid.NewGuid();

                    con.Users.Add(sysuser);
                    con.SaveChanges();

                    return RedirectToAction("Index","Home");
                }
            }
            return View(user);
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index","User");
        }

        private bool IsValid(string email, string pwd)
        {
            var cryto = new SimpleCrypto.PBKDF2();
            bool isvalid = false;

            using (var con = new MainDBContext())
            {
                User user = con.Users.FirstOrDefault(e => e.Email == email);

                if (user != null)
                {
                    if (user.Password == cryto.Compute(pwd, user.PasswordSalt))
                        isvalid = true;
                }
            }

            return isvalid;
        }
    }
}
