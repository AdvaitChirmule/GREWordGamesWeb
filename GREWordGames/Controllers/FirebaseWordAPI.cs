using Firebase.Database;
using Firebase.Database.Query;
using GREWordGames.Models;

namespace GREWordGames.Controllers
{
    public class FirebaseWordAPI
    {

        private readonly HttpClient _httpClient;
        private string _token;
        private string _uid;
        private FirebaseClient _firebaseClient;

        public FirebaseWordAPI(string token, string uid)
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

        public async Task<bool> CheckGlobalDatabase(string word)
        {
            var checkWord = await _firebaseClient.Child("words").Child(word).OnceSingleAsync<WordMetadata>();
            if (checkWord == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> IncrementWordCount()
        {
            try
            {
                int wordCount = await _firebaseClient.Child("words").Child("wordCount").OnceSingleAsync<int>();
                wordCount++;
                await _firebaseClient.Child("words").Child("wordCount").PutAsync<int>(wordCount);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddWordToDatabase(string word, string wordMeaning)
        {
            try
            {
                int wordCount = await _firebaseClient.Child("words").Child("wordCount").OnceSingleAsync<int>();
                wordCount++;

                var wordMetadata = new WordMetadata { id = wordCount, wordMeaning = wordMeaning };

                await _firebaseClient.Child("words").Child("wordCount").PutAsync<int>(wordCount);
                await _firebaseClient.Child("words").Child(word).PutAsync<WordMetadata>(wordMetadata);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
