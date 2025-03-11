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
using System.Security.Cryptography.X509Certificates;
using System.Numerics;

namespace GREWordGames.Controllers

{
    public class HomeController : Controller
    {
        private readonly WordMuseAPI _wordMuseAPI;
        private readonly CommonFunctions _commonFunctions;
        private readonly PracticePageFunctions _practicePageFunctions;
        private AllWords _practiceWordOrder;

        public HomeController(ILogger<HomeController> logger)
        {
            _wordMuseAPI = new WordMuseAPI();
            _commonFunctions = new CommonFunctions();
            _practicePageFunctions = new PracticePageFunctions();
            _practiceWordOrder = new AllWords();
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

        [HttpPost]
        public ActionResult GoToMyWords()
        {
            return RedirectToAction("MyWords");
        }

        [HttpPost]
        public async Task<IActionResult> AddWordAction(WordViewModel model)
        {
            var addWord = model.AddWord;
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                RedirectToAction("Login");
            }

            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            var uid = HttpContext.Session.GetString("uid");
            var userDetails = await firebaseClient.Child("metadata").Child(uid).OnceSingleAsync<UserMetadata>();

            string wordArrayStr = "[" + string.Join(",", userDetails.words.Trim('[', ']').Split(',').Select(w => $"\"{w.Trim()}\"")) + "]";

            if (wordArrayStr.Contains("\"" + addWord.Word + "\""))
            {
                TempData["Message"] = "Word already in the Database";
                return RedirectToAction("MyWords");
            }

            var globalDictionaryCheck = await firebaseClient.Child("words").Child(addWord.Word).OnceSingleAsync<WordMetadata>();

            if (globalDictionaryCheck == null)
            {
                string addWordMessage = await _wordMuseAPI.SetWordToGlobalDatabase(addWord.Word, token);
                if (addWordMessage != "Success")
                {
                    TempData["Message"] = addWordMessage;
                    return RedirectToAction("MyWords");
                }
            }

            var updatedUserDetails = _wordMuseAPI.AddWordToFirebase(userDetails, addWord.Word);

            await firebaseClient.Child("metadata").Child(uid).PutAsync(updatedUserDetails);

            TempData["Message"] = "Successfully added Word to Database";
            return RedirectToAction("MyWords");
        }

        [HttpGet]
        public async Task<JsonResult> DeleteWord(string id)
        {
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                RedirectToAction("Login");
            }

            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            var uid = HttpContext.Session.GetString("uid");
            var userDetails = await firebaseClient.Child("metadata").Child(uid).OnceSingleAsync<UserMetadata>();

            var user = _commonFunctions.ConvertRawDataToList(userDetails);

            var updatedUser = _commonFunctions.DeleteRow(user, int.Parse(id));

            userDetails.words = _commonFunctions.ConvertListToString(updatedUser.wordList);
            userDetails.proficiency = _commonFunctions.ConvertListToString(updatedUser.proficiencyList);
            userDetails.dateAdded = _commonFunctions.ConvertListToString(updatedUser.dateAddedList);

            await firebaseClient.Child("metadata").Child(uid).PutAsync(userDetails);

            return Json(new { success = true });
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

        public async Task<IActionResult> Practice()
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
                var user = _commonFunctions.ConvertRawDataToList(userDetails);

                var wordSegregated = _practicePageFunctions.SegregateByUserDifficulty(user);
                List<string> wordOrder = _practicePageFunctions.ReorderWordsBasedOnDifficulty(wordSegregated, 0);
                List<string> wordOutcome = Enumerable.Repeat("0", wordOrder.Count).ToList();

                _practiceWordOrder.words = wordOrder;
                _practiceWordOrder.outcome = wordOutcome;
                TempData["WordIndex"] = 0;
                TempData["RandomOrder"] = _commonFunctions.ConvertListToString(wordOrder);
                TempData["Proficiency"] = _commonFunctions.ConvertListToString(wordOutcome);

                return View(_practiceWordOrder);
            }
        }

        [HttpPost]
        public ActionResult GoToMyPractice()
        {
            return RedirectToAction("Practice");
        }

        [HttpGet]
        public async Task<JsonResult> GetNextWord(string word)
        {
            var token = HttpContext.Session.GetString("token");

            if (string.IsNullOrEmpty(token))
            {
                RedirectToAction("Login");
            }
            string wordMeaning = await _wordMuseAPI.GetWordMeaning(word, token);

            return Json(new { success = true, wordMeaning = wordMeaning });
        }

        [HttpGet]
        public JsonResult RegisterWrong()
        {
            ViewBag.Proficiency = TempData["Proficiency"];
            ViewBag.WordIndex = TempData["WordIndex"];
            var proficiencyList = ViewBag.Proficiency;
            var wordIndex = ViewBag.WordIndex;
            var practiceOrder = _commonFunctions.ConvertStringToList(proficiencyList);
            practiceOrder[wordIndex] = "1";
            wordIndex++;
            TempData["WordIndex"] = wordIndex;
            TempData["Proficiency"] = _commonFunctions.ConvertListToString(practiceOrder);
            return Json(new { success = true });
        }

        [HttpGet]
        public JsonResult RegisterRight()
        {
            ViewBag.Proficiency = TempData["Proficiency"];
            ViewBag.WordIndex = TempData["WordIndex"];
            var proficiencyList = ViewBag.Proficiency;
            var wordIndex = ViewBag.WordIndex;
            var practiceOrder = _commonFunctions.ConvertStringToList(proficiencyList);
            practiceOrder[wordIndex] = "2";
            wordIndex++;
            TempData["WordIndex"] = wordIndex;
            TempData["Proficiency"] = _commonFunctions.ConvertListToString(practiceOrder);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<JsonResult> CompleteGame()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false });
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
                var user = _commonFunctions.ConvertRawDataToList(userDetails);

                ViewBag.Proficiency = TempData["Proficiency"];
                ViewBag.Words = TempData["RandomOrder"];

                _practiceWordOrder.words = _commonFunctions.ConvertStringToList(ViewBag.Words);
                _practiceWordOrder.outcome = _commonFunctions.ConvertStringToList(ViewBag.Proficiency);

                var updatedProficiency = _practicePageFunctions.UpdateWordProficiencies(user, _practiceWordOrder);

                userDetails.proficiency = _commonFunctions.ConvertListToString(updatedProficiency.proficiencyList);

                await firebaseClient.Child("metadata").Child(uid).PutAsync(userDetails);
                return Json(new { success = true });
            }
        }
    }
}
