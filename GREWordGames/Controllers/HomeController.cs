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
        private AllWords _practiceWordOrder;
        MyWordsFunctions _myWordsFunctions;

        private string _token;
        private string _uid;
        private string _name;

        public HomeController(ILogger<HomeController> logger)
        {
            _wordMuseAPI = new WordMuseAPI();
            _commonFunctions = new CommonFunctions();
            _practiceWordOrder = new AllWords();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _token = HttpContext.Session.GetString("token");
            _uid = HttpContext.Session.GetString("uid");
            _name = HttpContext.Session.GetString("name");

            _myWordsFunctions = new MyWordsFunctions(_token, _uid, HttpContext.Session);
            base.OnActionExecuting(context);
        }

        public IActionResult Index(UserAuthenticated userAuthenticated)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                userAuthenticated.Condition = false;
            }
            else
            {
                userAuthenticated.Condition = true;
            }
            return View();
        }

        public async Task<IActionResult> MyWords()
        {
            if (string.IsNullOrEmpty(_token))
            {
                return RedirectToAction("Login");
            }
            else
            {                
                var userDetails = await _myWordsFunctions.GetUserTableRows();

                if (TempData["Message"] != null)
                {
                    ViewBag.Message = TempData["Message"];
                }
                else
                {
                    ViewBag.Message = "";
                }

                var model = _myWordsFunctions.PrepareModel(userDetails, ViewBag.Message);

                
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

            addWord.Word = addWord.Word.ToLower();

            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_uid))
            {
                TempData["LoginMessage"] = "Session Expired. Please login again";
                RedirectToAction("Login");
            }

            if (await _myWordsFunctions.CheckIfWordInDatabase(addWord.Word))
            {
                TempData["Message"] = "Word already in the Database";
                return RedirectToAction("MyWords");
            }

            if (!await _myWordsFunctions.CheckIfWordInGlobalDatabase(addWord.Word))
            {
                (bool success, string message) = await _myWordsFunctions.AddWordToGlobalDatabase(addWord.Word);
                if (!success)
                {
                    TempData["Message"] = message;
                    return RedirectToAction("MyWords");
                }
            }
            
            if (await _myWordsFunctions.AddWordToUserDatabase(addWord.Word))
            {
                TempData["Message"] = "Successfully added " + addWord.Word + " to the database";
            }
            else
            {
                TempData["Message"] = "Server side error in adding word, please try again";
                return RedirectToAction("MyWords");
            }

            return RedirectToAction("MyWords");
        }

        [HttpGet]
        public async Task<JsonResult> DeleteWord(int wordIndex)
        {
            var deleteAlready = TempData["DeleteInProcess"];
            if (deleteAlready != null)
            {
                return Json(new { success = false });
            }

            TempData["DeleteInProcess"] = true;

            if (await _myWordsFunctions.DeleteWordFromUserDatabase(wordIndex))
            {
                TempData["Message"] = "Word successfully deleted!";
                HttpContext.Session.Remove("DeleteInProcess");
                return Json(new { success = true });
            }
            else
            {
                HttpContext.Session.Remove("DeleteInProcess");
                return Json(new { success = false });
            }
        }

        public IActionResult AboutGame()
        {
            return View();
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
                PracticePageFunctions _practicePageFunctions = new PracticePageFunctions(_token, _uid, HttpContext.Session);

                var wordSegregated = await _practicePageFunctions.SegregateByUserDifficulty();
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
                ViewBag.Proficiency = TempData["Proficiency"];
                ViewBag.Words = TempData["RandomOrder"];

                _practiceWordOrder.words = _commonFunctions.ConvertStringToList(ViewBag.Words);
                _practiceWordOrder.outcome = _commonFunctions.ConvertStringToList(ViewBag.Proficiency);

                PracticePageFunctions _practicePageFunctions = new PracticePageFunctions(_token, _uid, HttpContext.Session);

                await _practicePageFunctions.UpdateWordProficiencies(_practiceWordOrder);

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
        public async Task<JsonResult> HostRoom(string password, int wordCount)
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
                    await roomFunctions.SetRoomNumber(password, _name);
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

        [HttpGet]
        public async Task<JsonResult> JoinRoom(int roomNumber, string password)
        {
            if (string.IsNullOrEmpty(_token) && string.IsNullOrEmpty(_uid))
            {
                return Json(new { success = false, message = "Login session expired. Please refresh and login again", roomNumber = -1 });
            }
            else
            {
                RoomFunctions roomFunctions = new RoomFunctions(roomNumber, password, _token, _uid, HttpContext.Session);
                (bool success, string message) = await roomFunctions.VerifyRoomDetails(roomNumber, password, _name);
                return Json(new { success = success, message = message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> WaitForPlayers(int roomNumber)
        {
            RoomFunctions roomFunctions = new RoomFunctions(roomNumber, _token, _uid, HttpContext.Session);
            string name2 = await roomFunctions.WaitForPlayers(roomNumber);
            if (name2 == "")
            {
                await roomFunctions.CloseRoom(roomNumber);
                return Json(new { success = false, name = "", message = "Closing Room due to no Response" });
            }
            else
            {
                return Json(new { success = true, name = name2, message = name2 + " joined the Room!" });
            }
        }


        public async Task<ActionResult> WaitToStart(int roomNumber)
        {
            RoomFunctions roomFunctions = new RoomFunctions(_token, _uid, HttpContext.Session);
            bool success = await roomFunctions.WaitToStart(roomNumber);
            if (success)
            {
                return RedirectToAction("GameRoom");
            }
            else
            {
                return RedirectToAction("WaitingRoom");
            }

        }

        [HttpPost]
        public async Task<ActionResult> StartGame(HostRoomDetails hostRoomDetails)
        {
            RoomFunctions roomFunctions = new RoomFunctions(_token, _uid, HttpContext.Session);
            bool success = await roomFunctions.StartGame(hostRoomDetails.Rounds);
            if (success)
            {
                return RedirectToAction("GameRoom");
            }
            else
            {
                return RedirectToAction("WaitingRoom");

            }
        }

        public async Task<IActionResult> GameRoom()
        {
            RoomFunctions _roomFunctions = new RoomFunctions(_token, _uid, HttpContext.Session);
            int roomNumber = _roomFunctions.GetRoomNumber();
            if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_uid) || roomNumber == -1)
            {
                return RedirectToAction("Login");
            }
            else
            {
                List<string> wordList = await _roomFunctions.GetWordList(roomNumber);
                Debug.WriteLine(wordList);

            }
            return View();
        }

        [HttpPost]
        public ActionResult GoToGameRoom()
        {
            return RedirectToAction("GameRoom");
        }
    }
}
