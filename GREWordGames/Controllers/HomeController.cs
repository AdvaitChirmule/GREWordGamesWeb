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
using Microsoft.AspNetCore.Mvc.Filters;

namespace GREWordGames.Controllers

{
    public class HomeController : Controller
    {
        private readonly WordMuseAPI _wordMuseAPI;
        private readonly CommonFunctions _commonFunctions;
        private readonly PracticePageFunctions _practicePageFunctions;
        private AllWords _practiceWordOrder;

        private string _token;
        private string _uid;

        public HomeController(ILogger<HomeController> logger)
        {
            _wordMuseAPI = new WordMuseAPI();
            _commonFunctions = new CommonFunctions();
            _practicePageFunctions = new PracticePageFunctions();
            _practiceWordOrder = new AllWords();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _token = HttpContext.Session.GetString("token");
            _uid = HttpContext.Session.GetString("uid");
            base.OnActionExecuting(context);
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
                    ViewBag.Message = "";
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
            var uid = HttpContext.Session.GetString("uid");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uid))
            {
                TempData["LoginMessage"] = "Session Expired. Please login again";
                RedirectToAction("Login");
            }

            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            var userDetails = await firebaseClient.Child("metadata").Child(uid).OnceSingleAsync<UserMetadata>();

            if (_commonFunctions.CheckIfWordInDatabase(userDetails.words, addWord.Word) == true)
            {
                TempData["Message"] = "Word already in the Database";
                return RedirectToAction("MyWords");
            }
               
            var globalDictionaryCheck = await firebaseClient.Child("words").Child(addWord.Word).OnceSingleAsync<WordMetadata>();

            if (globalDictionaryCheck == null)
            {
                string addWordMessage = await _wordMuseAPI.AddWordToGlobalDatabase(addWord.Word, token);
                if (addWordMessage != "Success")
                {
                    TempData["Message"] = addWordMessage;
                    return RedirectToAction("MyWords");
                }
            }

            bool result = await _wordMuseAPI.AddWordToFirebase(userDetails, addWord.Word, token, uid);

            if (result == false)
            {
                TempData["Message"] = "Unexpected Error occured";
            }
            else
            {
                TempData["Message"] = "Successfully added Word to Database";
            }

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

        public IActionResult WaitingRoom()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            
            return View();
        }

        [HttpPost]
        public ActionResult GoToWaitingRoom()
        {
            return RedirectToAction("WaitingRoom");
        }

        [HttpGet]
        public async Task<JsonResult> SetRoom(string password)
        {
            if (string.IsNullOrEmpty(_token) && string.IsNullOrEmpty(_uid))
            {
                return Json(new { success = false, message = "Login session expired. Please refresh and login again", roomNumber = -1 });
            }
            else
            {
                RoomFunctions roomFunctions = new RoomFunctions(password, _token, _uid, HttpContext.Session);
                if (roomFunctions.RoomAssigned())
                {
                    return Json(new { success = true, message = "You have already created a room", roomNumber = roomFunctions.GetRoomNumber() });
                }
                else
                {
                    await roomFunctions.SetRoomNumber(password);
                    if (roomFunctions.RoomAssigned())
                    {
                        return Json(new { success = true, message = "Found Room!", roomNumber = roomFunctions.GetRoomNumber() });
                    }
                    else
                    {
                        return Json(new { success = false, message = "All Rooms Occupied. Please try again later.", roomNumber = -1 });
                    }
                }
            }
        }

        public async Task<JsonResult> GetRoom(int roomNumber, string password)
        {
            if (string.IsNullOrEmpty(_token) && string.IsNullOrEmpty(_uid))
            {
                return Json(new { success = false, message = "Login session expired. Please refresh and login again", roomNumber = -1 });
            }
            else
            {
                RoomFunctions roomFunctions = new RoomFunctions(roomNumber, password, _token, _uid, HttpContext.Session);
                (bool success, string message) = await roomFunctions.VerifyRoomDetails(roomNumber, password);
                return Json(new { success = success, message = message });
            }
        }

        public IActionResult GameRoom()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GoToGameRoom()
        {
            return RedirectToAction("GameRoom");
        }
    }
}
