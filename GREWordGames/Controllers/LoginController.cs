using Firebase.Auth;
using Firebase.Database;
using GREWordGames.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace GREWordGames.Controllers
{
	public class LoginController : Controller
	{
        private readonly FirebaseAuthClient _firebaseAuth;

        public LoginController(FirebaseAuthClient firebaseAuth)
        {
            _firebaseAuth = firebaseAuth;
        }

        public IActionResult Login()
        {
            if (TempData["LoginMessage"] != null)
            {
                ViewBag.Message = TempData["LoginMessage"];
            }
            else
            {
                ViewBag.Message = "";
            }

            var emptyUserDetails = new UserDetails { };
            var notificationMessage = new Message { NotificationMessage = ViewBag.Message };

            var model = new LoginClass
            {
                UserDetails = emptyUserDetails,
                Message = notificationMessage
            };
            return View(model);
        }

        public IActionResult Register()
        {
            if(TempData["RegisterMessage"] != null)
            {
                ViewBag.Message = TempData["RegisterMessage"];
            }
            else
            {
                ViewBag.Message = "";
            }

            var emptyUserDetails = new UserDetails { };
            var notificationMessage = new Message { NotificationMessage = ViewBag.Message };

            var model = new LoginClass
            {
                UserDetails = emptyUserDetails,
                Message = notificationMessage
            };

            return View(model);
        }

        public IActionResult SignOutUser()
        {
            _firebaseAuth.SignOut();
            HttpContext.Session.Remove("token");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> LoginAction(UserDetails userDetails)
        {
            string name = userDetails.Name;
            string email = userDetails.Email;
            string password = userDetails.Password;
            try
            {
                var userCredentials = await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                if (userCredentials != null)
                {
                    var token = await userCredentials.User.GetIdTokenAsync();
                    var uid = userCredentials.User.Uid;
                    HttpContext.Session.SetString("token", token);
                    HttpContext.Session.SetString("uid", uid);
                    TempData["LoginMessage"] = "Logged In Successfully!";
                    return RedirectToAction("MyWords");
                }
                else
                {
                    TempData["LoginMessage"] = "Username or Password Incorrect";
                    return RedirectToAction("Login");
                }
            }
            catch (FirebaseAuthException ex)
            {
                TempData["LoginMessage"] = "Unexpected Server Error encountered";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> RegisterAction(UserDetails userDetails)
        {
            if (userDetails.Password != userDetails.PasswordReentered)
            {
                TempData["RegisterMessage"] = "Passwords do not match";
                return RedirectToAction("Register");
            }
            else
            {
                try
                {
                    var userCredentials = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(userDetails.Email, userDetails.Password);
                    if (userCredentials != null)
                    {
                        TempData["RegisterMessage"] = "Registered Successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["LoginMessage"] = "User already exists. Please Login instead.";
                        return RedirectToAction("Login");
                    }
                }
                catch (FirebaseAuthException ex)
                {
                    TempData["RegisterMessage"] = "Unexpected Server Error encountered";
                    return RedirectToAction("Register");
                }

            }
        }

        [HttpPost]
        public ActionResult GoToLogin()
        {
            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult GoToRegister()
        {
            return RedirectToAction("Register");
        }
    }
}
