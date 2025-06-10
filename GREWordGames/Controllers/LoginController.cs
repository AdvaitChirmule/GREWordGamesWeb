using Firebase.Auth;
using Firebase.Database;
using GREWordGames.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using FirebaseAdmin.Auth;


namespace GREWordGames.Controllers
{
	public class LoginController : Controller
	{
        private readonly FirebaseAuthClient _firebaseAuth;
        private readonly LoginFunctions _loginFunctions;
        private FirebaseUserAPI _firebaseAPI;

        public LoginController(FirebaseAuthClient firebaseAuth)
        {
            _firebaseAuth = firebaseAuth;
        }

        public IActionResult Login()
        {
            LoginFunctions _loginFunctions = new LoginFunctions(_firebaseAuth, HttpContext.Session);

            var existingNotifications = _loginFunctions.CheckMessages(TempData["LoginMessage"]);
            var model = _loginFunctions.PrepareModel(existingNotifications);

            return View(model);
        }

        public IActionResult Register()
        {
            LoginFunctions _loginFunctions = new LoginFunctions(_firebaseAuth, HttpContext.Session);

            var existingNotifications = _loginFunctions.CheckMessages(TempData["RegisterMessage"]);
            var model = _loginFunctions.PrepareModel(existingNotifications);

            return View(model);
        }

        public IActionResult SignOutUser()
        {
            _firebaseAuth.SignOut();
            HttpContext.Session.Remove("token");
            HttpContext.Session.Remove("uid");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> LoginAction(UserDetails userDetails)
        {
            LoginFunctions _loginFunctions = new LoginFunctions(_firebaseAuth, HttpContext.Session);

            bool emailCheck = _loginFunctions.CheckEmailNotEmpty(userDetails.Email);
            if (!emailCheck)
            {
                var model = _loginFunctions.PrepareModel("Email Not Entered");
                return View("Login", model);
            }

            bool passwordCheck = _loginFunctions.CheckPasswordNotEmpty(userDetails.Password);
            if (!passwordCheck)
            {
                var model = _loginFunctions.PrepareModel("Password Not Entered");
                return View("Login", model);
            }

            bool loginSuccess = await _loginFunctions.VerifyLoginDetails(userDetails.Email, userDetails.Password);
            if (!loginSuccess)
            {
                var model = _loginFunctions.PrepareModel("Incorrect Credentials or Server Error");
                return View("Login", model);
            }
            else
            {
                return RedirectToAction("Index");
            }          
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAction(UserDetails userDetails)
        {
            LoginFunctions _loginFunctions = new LoginFunctions(_firebaseAuth, HttpContext.Session);

            bool emailCheck = _loginFunctions.CheckEmailNotEmpty(userDetails.Email);
            if (!emailCheck)
            {
                var model = _loginFunctions.PrepareModel("Email Not Entered");
                return View("Register", model);
            }

            bool passwordCheck = _loginFunctions.CheckPasswordNotEmpty(userDetails.Password) || _loginFunctions.CheckPasswordNotEmpty(userDetails.PasswordReentered);
            if (!passwordCheck)
            {
                var model = _loginFunctions.PrepareModel("Password Not Entered");
                return View("Register", model);
            }

            bool bothPasswordSameCheck = _loginFunctions.CheckBothPasswordsSame(userDetails.Password, userDetails.PasswordReentered);
            if (!bothPasswordSameCheck)
            {
                var model = _loginFunctions.PrepareModel("Passwords Do Not Match");
                return View("Register", model);
            }

            bool registerSuccess = await _loginFunctions.RegisterUser(userDetails);
            if (!registerSuccess)
            {
                var model = _loginFunctions.PrepareModel("User Already Exists. Please Login instead");
                return View("Login", model);
            }
            else
            {
                return RedirectToAction("Index");
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
