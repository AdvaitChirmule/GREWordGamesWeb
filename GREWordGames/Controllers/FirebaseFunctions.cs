using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Firebase.Database;
using Firebase.Database.Query;
using GREWordGames.Models;

namespace GREWordGames.Controllers
{
    public class FirebaseFunctions
    {
        private readonly HttpClient _httpClient;
        private string _uid;
        private FirebaseClient _firebaseClient;

        public FirebaseFunctions(string token, string uid)
        {
            _httpClient = new HttpClient();
            _uid = uid;
            _firebaseClient = new FirebaseClient("https://grewordgames-default-rtdb.firebaseio.com", 
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token),
                });
        }

        public async Task<int> GetNextFreeRoom(string password)
        {
            int maxRoomCount = await _firebaseClient.Child("rooms").Child("roomCount").OnceSingleAsync<int>();
            for (int i = 1; i < maxRoomCount + 1; i++)
            {
                bool roomOccupied = await _firebaseClient.Child("rooms").Child(i.ToString()).Child("occupied").OnceSingleAsync<bool>();
                if (roomOccupied == false)
                {
                    FirebaseRoomDetails roomDetails = new FirebaseRoomDetails { occupied = true, waiting = true, password = password };
                    await _firebaseClient.Child("rooms").Child(i.ToString()).PutAsync(roomDetails);
                    return i;
                }
            }

            return -1;
        }

        public async Task<(bool,string)> VerifyCredentials(int room, string password)
        {
            int maxRoomCount = await _firebaseClient.Child("rooms").Child("roomCount").OnceSingleAsync<int>();
            if ((room > maxRoomCount) || (room < 0))
            {
                return (false, "Invalid Room Number");
            }
            else
            {
                bool roomOccupied = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("occupied").OnceSingleAsync<bool>();
                if (roomOccupied)
                {
                    bool roomWaiting = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("waiting").OnceSingleAsync<bool>();
                    if (roomWaiting)
                    {
                        string roomPassword = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("password").OnceSingleAsync<string>();
                        if (roomPassword == password)
                        {
                            await _firebaseClient.Child("rooms").Child(room.ToString()).Child("waiting").PutAsync(false);
                            return (true, "All Good");
                        }
                        else
                        {
                            return (false, "Incorrect Password");
                        }
                    }
                    else
                    {
                        return (false, "Game Room Busy");
                    }
                }
                else
                {
                    return (false, "Game Room Not Hosted");
                }
            }
        }
    }
}