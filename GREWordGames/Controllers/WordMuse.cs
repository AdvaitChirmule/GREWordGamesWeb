using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using GREWordGames.Models;
using System.Diagnostics;
using Firebase.Database;
using Microsoft.AspNetCore.Http;
using Firebase.Database.Query;
using System.Collections.Generic;

namespace GREWordGames.Controllers
{
    public class WordMuseAPI
    {
        private readonly HttpClient _httpClient;

        public WordMuseAPI()
        {
            _httpClient = new HttpClient();
        }

        public async Task<String> SetWordToGlobalDatabase(String word, String token)
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
    }
}
