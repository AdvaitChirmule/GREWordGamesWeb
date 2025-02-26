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

        public UserMetadata AddWordToFirebase(UserMetadata userDetails, string word)
        {
            var wordList = userDetails.words.Substring(0, userDetails.words.Length - 1) + ", " + word + "]";
            userDetails.words = wordList;

            var nowTime = DateTime.UtcNow;
            var dateAddedList = userDetails.dateAdded.Substring(0, userDetails.dateAdded.Length - 1) + ", " + nowTime.ToString("s") + ".000Z]";
            userDetails.dateAdded = dateAddedList;

            var proficiencyList = userDetails.proficiency.Substring(0, userDetails.proficiency.Length - 1) + ", (0|0)]";
            userDetails.proficiency = proficiencyList;

            userDetails.wordCount = userDetails.wordCount + 1;

            return userDetails;
        }

        public async Task<String> SetWordToGlobalDatabase(string word, string token)
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
                            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                                    new FirebaseOptions
                                    {
                                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                                    });

                            int wordCount = await firebaseClient.Child("words").Child("wordCount").OnceSingleAsync<int>();

                            wordCount += 1;
                            var wordMetadata = new WordMetadata { id = wordCount, wordMeaning = $"[{string.Join(", ", apiDataJson[i].defs)}]" };

                            await firebaseClient.Child("words").Child("wordCount").PutAsync(wordCount);
                            await firebaseClient.Child("words").Child(word).PutAsync(wordMetadata);

                            foundWordFlag = true;
                            break;
                        }
                    }

                    if (!foundWordFlag)
                    {
                        return "Word Deemed Invalid";
                    }
                    return "Success";
                }
                else
                {
                    return "Unexpected Server Error while adding Word to the Database";
                }
            }
            catch
            {
                return "Unexpected Server Error while adding Word to the Database";
            }
        }

        public async Task<List<string>> GetUserWordMeanings(List<string> words, string token)
        {
            List<string> wordMeanings = new List<string>();

            var firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com",
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token)
                    });

            foreach (string word in words)
            {
                var wordDetails = await firebaseClient.Child("words").Child(word).OnceSingleAsync<WordMetadata>();
                wordMeanings.Add(wordDetails.wordMeaning);
            }

            return wordMeanings;
        }
    }
}
