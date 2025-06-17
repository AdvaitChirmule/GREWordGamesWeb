using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Firebase.Database;
using Firebase.Database.Query;
using GREWordGames.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GREWordGames.Controllers
{
    public class FirebaseUserAPI
    {
        private readonly HttpClient _httpClient;
        private string _token;
        private string _uid;
        private FirebaseClient _firebaseClient;

        public FirebaseUserAPI(string token, string uid)
        {
            _httpClient = new HttpClient();
            _token = token;
            _uid = uid;
            _firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token),
                });
        }

        public async Task AddUser(string name)
        {
            UserMetadata newUser = new UserMetadata { username = name, lastAccessedDevice = "" };
            await _firebaseClient.Child("metadata").Child(_uid).PutAsync(newUser);
        }

        public async Task SetUser(UserMetadata userMetadata)
        {
            await _firebaseClient.Child("metadata").Child(_uid).PutAsync(userMetadata);
        }

        public async Task<UserMetadata> GetUserDetails()
        {
            UserMetadata userMetadata = await _firebaseClient.Child("metadata").Child(_uid).OnceSingleAsync<UserMetadata>();
            return userMetadata;
        }

        public async Task<string> GetUserWordList()
        {
            string wordList = await _firebaseClient.Child("metadata").Child(_uid).Child("words").OnceSingleAsync<string>();
            return wordList;
        }

        public async Task<string> GetGuestUserWordList(int roomNumber)
        {
            string wordList = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Player2WordList").OnceSingleAsync<string>();
            return wordList;
        }

        public async Task<bool> SetUserWordList(string wordList)
        {
            try
            {
                await _firebaseClient.Child("metadata").Child(_uid).Child("words").PutAsync<string>(wordList);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetUserDateAddedList()
        {
            string dateAddedList = await _firebaseClient.Child("metadata").Child(_uid).Child("dateAdded").OnceSingleAsync<string>();
            return dateAddedList;
        }

        public async Task<bool> SetUserDateAddedList(string dateAddedList)
        {
            try
            {
                await _firebaseClient.Child("metadata").Child(_uid).Child("dateAdded").PutAsync<string>(dateAddedList);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetUserProficiencyList()
        {
            string proficiencyList = await _firebaseClient.Child("metadata").Child(_uid).Child("proficiency").OnceSingleAsync<string>();
            return proficiencyList;
        }

        public async Task<bool> SetUserProficiencyList(string proficiencyList)
        {
            try
            {
                await _firebaseClient.Child("metadata").Child(_uid).Child("proficiency").PutAsync<string>(proficiencyList);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}