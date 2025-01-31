using GREWordGames.Models;
using Microsoft.AspNetCore.Mvc;

namespace GREWordGames.Controllers
{
	public class LoginController : Controller
	{

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginAction(UserDetails UserDetails)
        {
            string name = UserDetails.Name;
            string email = UserDetails.Email;
            string password = UserDetails.Password;

            return RedirectToAction("MyWords");
        }

        [HttpPost]
        public ActionResult TraversalRegister()
        {
            return RedirectToAction("Register");
        }

        [HttpPost]
        public ActionResult RegisterAction(UserDetails UserDetails)
        {
            string name = UserDetails.Name;
            string email = UserDetails.Email;
            string password = UserDetails.Password;
            string passwordReentered = UserDetails.PasswordRe;

            if (password != passwordReentered)
            {
                Console.WriteLine("Jogfggd?");
                return RedirectToAction("Register");
            }
            else
            {
                return RedirectToAction("MyWords");
            }
        }

        [HttpPost]
        public ActionResult TraversalLogin()
        {
            Console.WriteLine("Jo?");
            return RedirectToAction("Login");
        }
    }
}
