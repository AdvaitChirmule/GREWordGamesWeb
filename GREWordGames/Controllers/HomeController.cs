using System.Diagnostics;
using GREWordGames.Models;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using Newtonsoft.Json;
using NuGet.Common;
using System.Reflection;
using FirebaseAdmin.Auth;
using UserMetadata = GREWordGames.Models.UserMetadata;
using System.Net.Http;

namespace GREWordGames.Controllers

{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WordMuseAPI _wordMuseAPI;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _wordMuseAPI = new WordMuseAPI();
        }

        public IActionResult Index(UserAuthenticated userAuthenticated)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                userAuthenticated.Condition = true;
            }
            else
            {
                userAuthenticated.Condition = false;
            }
            return View();
        }

        public async Task<IActionResult> MyWords()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            else
            {
                var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

                var uid = HttpContext.Session.GetString("uid");
                var userDetails = await firebaseClient.Child("metadata").Child(uid).OnceSingleAsync<UserMetadata>();
                var emptyWord = new AddWord { Word = "" };

                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                else
                {
                    ViewBag.Message = "nothing rn";
                }

                var notificationMessage = new Message { NotificationMessage = ViewBag.Message };

                var model = new WordViewModel
                {
                    UserMetadata = userDetails,
                    AddWord = emptyWord,
                    Message = notificationMessage
                };
                return View(model);
            }
        }

        public IActionResult Practice()
        {
            return View();
        }

        public IActionResult AboutGame()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult GoToHome(UserAuthenticated userAuthenticated)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                userAuthenticated.Condition = true;
            }
            else
            {
                userAuthenticated.Condition = false;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GoToMyWords()
        {
            return RedirectToAction("MyWords");
        }

        [HttpPost]
        public ActionResult GoToMyPractice()
        {
            return RedirectToAction("Practice");
        }

        [HttpPost]
        public ActionResult GoToAboutGame()
        {
            return RedirectToAction("AboutGame");
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

        [HttpPost]
        public async Task<IActionResult> AddWordAction(WordViewModel model)
        {
            var addWord = model.AddWord;
            var token = HttpContext.Session.GetString("token");
            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            var uid = HttpContext.Session.GetString("uid");
            var userDetails = await firebaseClient.Child("metadata").Child(uid).OnceSingleAsync<UserMetadata>();

            string wordArrayStr = "[" + string.Join(",", userDetails.words.Trim('[', ']').Split(',').Select(w => $"\"{w.Trim()}\"")) + "]";

            if (wordArrayStr.Contains("\"" + addWord.Word + "\"")){
                TempData["Message"] = "Word already in the Database";
                return RedirectToAction("MyWords");
            }

            var globalDictionaryCheck = await firebaseClient.Child("words").Child(addWord.Word).OnceSingleAsync<WordMetadata>();

            if (globalDictionaryCheck == null)
            {
                bool addWordSuccess = await AddWordToGlobalDictionary(addWord.Word);
                if (!addWordSuccess)
                {
                    return RedirectToAction("MyWords");
                }
            }

            var wordList = userDetails.words.Substring(0, userDetails.words.Length - 1) + ", " + addWord.Word + "]";
            userDetails.words = wordList;

            var nowTime = DateTime.UtcNow;
            var dateAddedList = userDetails.dateAdded.Substring(0, userDetails.dateAdded.Length - 1) + ", " + nowTime.ToString("s") + ".000Z]";
            userDetails.dateAdded = dateAddedList;

            var proficiencyList = userDetails.proficiency.Substring(0, userDetails.proficiency.Length - 1) + ", (0|0)]";
            userDetails.proficiency = proficiencyList;

            userDetails.wordCount = userDetails.wordCount + 1;


            await firebaseClient.Child("metadata").Child(uid).PutAsync(userDetails);

            TempData["Message"] = "Successfully added Word to Database";
            return RedirectToAction("MyWords");
        }
        public async Task<bool> AddWordToGlobalDictionary(string word)
        {
            var token = HttpContext.Session.GetString("token");
            var wordMuseOutput = await _wordMuseAPI.SetWordToGlobalDatabase(word, token);
            if (wordMuseOutput == "Success")
            {
                TempData["Message"] = "Success";
                return true;
            }
            else
            {
                TempData["Message"] = wordMuseOutput;
                return false;
            }
        }
    }
}
