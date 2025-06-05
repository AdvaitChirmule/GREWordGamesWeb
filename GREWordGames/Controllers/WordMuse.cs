using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GREWordGames.Models;
using System.Diagnostics;
using Firebase.Database;
using Microsoft.AspNetCore.Http;
using Firebase.Database.Query;
using System.Collections.Generic;
using NuGet.Common;

namespace GREWordGames.Controllers
{
    public class WordMuseAPI
    {
        private readonly HttpClient _httpClient;

        public WordMuseAPI()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> AddWordToFirebase(UserMetadata userDetails, string word, string token, string uid)
        {
            if (userDetails.words.Length == 2)
            {
                var wordList = "[" + word + "]";
                userDetails.words = wordList;

                var nowTime = DateTime.UtcNow;
                var dateAddedList = "[" + nowTime.ToString("s") + "]";
                userDetails.dateAdded = dateAddedList;

                var proficiencyList = "[(0|0)]";
                userDetails.proficiency = proficiencyList;

                userDetails.wordCount = userDetails.wordCount + 1;
            }
            else
            {
                var wordList = userDetails.words.Substring(0, userDetails.words.Length - 1) + ", " + word + "]";
                userDetails.words = wordList;

                var nowTime = DateTime.UtcNow;
                var dateAddedList = userDetails.dateAdded.Substring(0, userDetails.dateAdded.Length - 1) + ", " + nowTime.ToString("s") + "]";
                userDetails.dateAdded = dateAddedList;

                var proficiencyList = userDetails.proficiency.Substring(0, userDetails.proficiency.Length - 1) + ", (0|0)]";
                userDetails.proficiency = proficiencyList;

                userDetails.wordCount = userDetails.wordCount + 1;
            }

            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            try
            {
                await firebaseClient.Child("metadata").Child(uid).PutAsync(userDetails);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<(bool, String)> GetWordMeaning(string word)
        {
            string wordMuseURL = "https://api.datamuse.com/words?sl=" + word + "&max=10&md=d";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(wordMuseURL);
                if (response.IsSuccessStatusCode)
                {
                    var apiData = await response.Content.ReadAsStringAsync();
                    List<WordMuseAPIJSON> apiDataJson = JsonConvert.DeserializeObject<List<WordMuseAPIJSON>>(apiData);
                    Debug.WriteLine(apiDataJson);

                    bool foundWordFlag = false;
                    for (int i=0; i<apiDataJson.Count; i++)
                    {
                        if (apiDataJson[i].word == word)
                        {
                            return (true, $"[{string.Join(", ", apiDataJson[i].defs)}]");
                        }
                    }

                    return (false, "Word Deemed Invalid");
                }
                else
                {
                    return (false, "Unexpected Server Error while adding Word to the Database");
                }
            }
            catch
            {
                return (false, "Unexpected Server Error while adding Word to the Database");
            }
        }

        public async Task<string> GetWordMeaning(string word, string token)
        {
            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            var wordDetails = await firebaseClient.Child("words").Child(word).OnceSingleAsync<WordMetadata>();
            string wordMeanings = wordDetails.wordMeaning;

            return wordMeanings;
        }
    }
}
