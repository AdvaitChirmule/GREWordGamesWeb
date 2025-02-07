using Firebase.Auth;
using GREWordGames.Models;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult> LoginAction(UserDetails UserDetails)
        {
            string name = UserDetails.Name;
            string email = UserDetails.Email;
            string password = UserDetails.Password;
            try
            {
                var userCredentials = await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                if (userCredentials != null)
                {
                    var token = await userCredentials.User.GetIdTokenAsync();
                    HttpContext.Session.SetString("token", token);
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
        public async Task<ActionResult> RegisterAction(UserDetails UserDetails)
        {
            string name = UserDetails.Name;
            string email = UserDetails.Email;
            string password = UserDetails.Password;
            string passwordReentered = UserDetails.PasswordRe;

            if (password != passwordReentered)
            {
                return RedirectToAction("Register");
            }
            else
            {
                try
                {
                    var userCredentials = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
                    if (userCredentials != null)
                    {
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
