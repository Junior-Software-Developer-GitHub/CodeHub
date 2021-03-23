using CodeHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CodeHub.Controllers
{
    public class ContactUsController : Controller
    {
        // GET: ContactUs
        public ActionResult Index()
        {
            return View("~/Views/User/Welcome.cshtml");
        }

        [HttpPost]
        //[NonAction]
        public RedirectToRouteResult SendMsg(ContactViewModel vm)//uspeo sam da odradim da se posalje i da se relouda strana, kako bez relouda?
        {
                try
                {
                MailMessage msz = new MailMessage
                {
                    From = new MailAddress(vm.EmailID)//Email which you are getting 
                };
                //from contact us page 
                msz.To.Add("codehubsupp@gmail.com");//Where mail will be sent 
                    msz.Subject = vm.Subject;
                    msz.Body = vm.Message + "\nName: " + vm.Name + "\nEmail: " + vm.EmailID;
                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential
                ("codehubsupp@gmail.com", "Support123"),

                    EnableSsl = true
                };

                smtp.Send(msz);

                    ModelState.Clear();
                    ViewBag.Message = "Thank you for Contacting us ";
                    //return View("~/Views/User/Welcome.cshtml");
                }
                catch (Exception ex)
                {
                    ModelState.Clear();
                    ViewBag.Message = $" Sorry we are facing Problem here {ex.Message}";
                }
            //return View("~/Views/User/Welcome.cshtml");
            return RedirectToAction("Index");
        }
    }
}