using System.Diagnostics;
using System.Net.Http;
using Firebase.Database;
using Firebase.Database.Query;
using GREWordGames.Models;
using Newtonsoft.Json;
using NuGet.Common;

namespace GREWordGames.Controllers
{
    public class MyWordsFunctions
    {
        private readonly CommonFunctions _commonFunctions;
        private readonly WordMuseAPI _wordMuseAPI;
        private readonly FirebaseWordAPI _firebaseWordAPI;
        private readonly FirebaseUserAPI _firebaseUserAPI;
        private string _token;
        private string _uid;
        private ISession _session;
        public MyWordsFunctions(string token, string uid, ISession session)
        {
            _token = token;
            _uid = uid;
            _session = session;
            _commonFunctions = new CommonFunctions();
            _wordMuseAPI = new WordMuseAPI();
            _firebaseUserAPI = new FirebaseUserAPI(_token, _uid);
            _firebaseWordAPI = new FirebaseWordAPI(_token, _uid);

        }

        public async Task<UserMetadata> GetUserTableRows()
        {
            var userDetails = await _firebaseUserAPI.GetUserDetails();
            return userDetails;
        }

        public WordViewModel PrepareModel(UserMetadata userDetails, string message)
        {
            var emptyWord = new AddWord { Word = "" };

            var notificationMessage = new Message { NotificationMessage = message };

            var model = new WordViewModel
            {
                UserMetadata = userDetails,
                AddWord = emptyWord,
                Message = notificationMessage
            };

            return model;
        }

        public async Task<bool> CheckIfWordInDatabase(string word)
        {
            string userWordListRaw = await _firebaseUserAPI.GetUserWordList();
            if (userWordListRaw.Contains("\"" + word + "\""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> CheckIfWordInGlobalDatabase(string word)
        {
            bool globalDictionaryCheck = await _firebaseWordAPI.CheckGlobalDatabase(word);
            if (globalDictionaryCheck)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<(bool, string)> AddWordToGlobalDatabase(string word)
        {
            (bool success, string result) = await _wordMuseAPI.GetWordMeaning(word);
            if (!success)
            {
                return (false, result);
            }

            if (await _firebaseWordAPI.AddWordToDatabase(word, result))
            {
                return (true, "Successfully Added Word");
            }
            else
            {
                return (false, "Error adding word to database");
            }
        }

        public async Task<bool> AddWordToUserDatabase(string word)
        {
            string userWordListRaw = await _firebaseUserAPI.GetUserWordList();
            string userDateAddedListRaw = await _firebaseUserAPI.GetUserDateAddedList();
            string userProficiencyListRaw = await _firebaseUserAPI.GetUserProficiencyList();

            var nowTime = DateTime.UtcNow;
            if (userWordListRaw == "[]")
            {
                userWordListRaw = "[" + word + "]";
                userDateAddedListRaw = "[" + nowTime.ToString("s") + "]";
                userProficiencyListRaw = "[(0|0)]";
            }
            else
            {
                userWordListRaw = userWordListRaw.Substring(0, userWordListRaw.Length - 1) + ", " + word + "]";
                userDateAddedListRaw = userDateAddedListRaw.Substring(0, userDateAddedListRaw.Length - 1) + ", " + nowTime.ToString("s") + "]";
                userProficiencyListRaw = userProficiencyListRaw.Substring(0, userProficiencyListRaw.Length - 1) + ", (0|0)]";
            }

            try
            {
                bool success = await _firebaseUserAPI.SetUserWordList(userWordListRaw);
                success = success && await _firebaseUserAPI.SetUserDateAddedList(userDateAddedListRaw);
                success = success && await _firebaseUserAPI.SetUserProficiencyList(userProficiencyListRaw);
                return success;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteWordFromUserDatabase(int wordIndex)
        {
            string userWordListRaw = await _firebaseUserAPI.GetUserWordList();
            string userDateAddedListRaw = await _firebaseUserAPI.GetUserDateAddedList();
            string userProficiencyListRaw = await _firebaseUserAPI.GetUserProficiencyList();

            List<string> userWordList = _commonFunctions.ConvertStringToList(userWordListRaw);
            List<string> userDateAddedList = _commonFunctions.ConvertStringToList(userDateAddedListRaw);
            List<string> userProficiencyList = _commonFunctions.ConvertStringToList(userProficiencyListRaw);

            userWordList.RemoveAt(wordIndex);
            userDateAddedList.RemoveAt(wordIndex);
            userProficiencyList.RemoveAt(wordIndex);

            userWordListRaw = _commonFunctions.ConvertListToString(userWordList);
            userDateAddedListRaw = _commonFunctions.ConvertListToString(userDateAddedList);
            userProficiencyListRaw = _commonFunctions.ConvertListToString(userProficiencyList);

            try
            {
                bool success = await _firebaseUserAPI.SetUserWordList(userWordListRaw);
                success = success && await _firebaseUserAPI.SetUserDateAddedList(userDateAddedListRaw);
                success = success && await _firebaseUserAPI.SetUserProficiencyList(userProficiencyListRaw);
                return success;
            }
            catch
            {
                return false;
            }
        }
    }
}
