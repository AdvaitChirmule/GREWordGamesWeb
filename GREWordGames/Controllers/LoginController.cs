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
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult SignOutUser()
        {
            _firebaseAuth.SignOut();
            HttpContext.Session.Remove("token");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> LoginAction(UserDetails userDetails, UserMetadata userMetadata)
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
                    return RedirectToAction("MyWords");
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            catch (FirebaseAuthException ex)
            {
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<ActionResult> RegisterAction(UserDetails userDetails)
        {
            if (userDetails.Password != userDetails.PasswordReentered)
            {
                return RedirectToAction("Register");
            }
            else
            {
                try
                {
                    var userCredentials = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(userDetails.Email, userDetails.Password);
                    if (userCredentials != null)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }
                catch (FirebaseAuthException ex)
                {
                    return RedirectToAction("Login");
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
