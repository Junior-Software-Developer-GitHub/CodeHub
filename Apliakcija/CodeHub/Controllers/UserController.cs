using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CodeHub.Models;
using System.Web.Security;

namespace CodeHub.Controllers//javne metode u kontolerskoj klasi su akcije 
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }  

        //Registraton POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")]User user)//bind exclude ce
                             //iskljuciti navedene parametre i nece ih cuvati na serveru,kako bi obeezbedili sigurnost
        {                     //takodje necemo unositi ove parametre!!!!!

            bool status = false;
            string message = null;
            //Model Validation
            if (ModelState.IsValid)
            {
                bool isExist = IsEmailExist(user.EmailID);
                if (isExist)//Email is already exist
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                //Generate Activation Code
                user.ActivationCode = Guid.NewGuid();//creates a new GUID using an algorithm that is designed to 
                                                     //make collisions very, very unlikely. 7c9e6679-7425-40de-944b-e07fc1f90ae7 for example

                user.IsEmailVerified = false;

                //Save data to Database
                using (CodeHubDatabaseEntities cd = new CodeHubDatabaseEntities())
                {
                    //Send Email to User
                    if(SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString()))
                    {
                        cd.Users.Add(user);
                        cd.SaveChanges();
                    }
                    message = "Registration sucessfully done. Activation link " +
                        "has been sent to your email id:" + user.EmailID;
                    status = true;
                }
            }

            else
            {
                message = "invalid request";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }
        //Verify acc
        [HttpGet]//Verify Email LINK
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using(CodeHubDatabaseEntities cd = new CodeHubDatabaseEntities())
            {
                cd.Configuration.ValidateOnSaveEnabled = false;//this line i have added here to avoid confirm pass doesnt match isssue on save changes
                var v = cd.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                   
                    cd.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string returnUrl)
        {
            string message = null;
            using(CodeHubDatabaseEntities cd = new CodeHubDatabaseEntities())
            {
                User v = cd.Users.Where(a => a.EmailID.Equals(login.EmailID)).FirstOrDefault();
                if (v != null)
                {
                    if (v.IsEmailVerified == true && string.Compare(login.Password, v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;//525600 min is 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
                        {
                            Expires = DateTime.Now.AddMinutes(timeout),
                            HttpOnly = true
                        };
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        else
                            return RedirectToAction("Welcome",User);//ovde baca na zeljenu stranicu(view)
                    }
                
                else
                {
                    message = "Invalid credential provided";
                }
                }
            }
            ViewBag.Message = message;
            return View();
        }
        public ActionResult Welcome()
        {
            return View();
        }
        //LOGOUT
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");//vraca na login
        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using(CodeHubDatabaseEntities cd = new CodeHubDatabaseEntities())
            {
                var v = cd.Users.Where(a => a.EmailID.Equals(emailID)).FirstOrDefault();
                return v != null;//return v == null ? false : true;

            }
        }

        [NonAction]
        public bool SendVerificationLinkEmail(string emailID, string activationCode)
        {
            string fromaddr = "codehubsupp@gmail.com";
            string password = "Support123";
            var toEmail = new MailAddress(emailID);
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            string subject = " Your account is successfully created!";

            string body = "We are excited to tell you that your CodeHub Account is" +
                " successfully created.<br/>Please click on the below link to verify your account:" +
                "<br/><br/><a href='" + link + "'>" + link + "<a/> ";
            MailMessage msg = new MailMessage
            {
                Subject = subject,
                From = new MailAddress(fromaddr), 
                Body = body
            };
            msg.IsBodyHtml = true;
            msg.To.Add(toEmail);
            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                UseDefaultCredentials = false,
                EnableSsl = true
            };
            smtp.Credentials = new NetworkCredential(fromaddr, password);
            smtp.Send(msg);
            return true;
        }
    }
   
}