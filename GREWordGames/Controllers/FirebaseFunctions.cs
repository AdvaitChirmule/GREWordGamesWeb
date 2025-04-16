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
        private string _token;
        private string _uid;
        private FirebaseClient _firebaseClient;

        public FirebaseFunctions(string token, string uid)
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
            UserMetadata newUser = new UserMetadata { username = name, lastAccessedDevice = ""};
            await _firebaseClient.Child("metadata").Child(_uid).PutAsync(newUser);
        }

        public async Task<int> GetNextFreeRoom(string password, string name1)
        {
            int maxRoomCount = await _firebaseClient.Child("rooms").Child("roomCount").OnceSingleAsync<int>();
            for (int i = 1; i < maxRoomCount + 1; i++)
            {
                bool roomOccupied = await _firebaseClient.Child("rooms").Child(i.ToString()).Child("occupied").OnceSingleAsync<bool>();
                if (roomOccupied == false)
                {
                    FirebaseRoomDetails roomDetails = new FirebaseRoomDetails { occupied = true, player2JoinFlag = true, startFlag = false, password = password, player1 = name1, player2 = "" };
                    await _firebaseClient.Child("rooms").Child(i.ToString()).PutAsync(roomDetails);
                    return i;
                }
            }

            return -1;
        }

        public async Task<(bool,string)> VerifyCredentials(int room, string password, string name2)
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
                    bool player2Joined = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("player2JoinFlag").OnceSingleAsync<bool>();
                    if (!player2Joined)
                    {
                        string roomPassword = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("password").OnceSingleAsync<string>();
                        if (roomPassword == password)
                        {
                            FirebaseRoomDetails gameRoom = await _firebaseClient.Child("rooms").Child(room.ToString()).OnceSingleAsync<FirebaseRoomDetails>();
                            gameRoom.player2JoinFlag = true;
                            gameRoom.player2 = name2;
                            await _firebaseClient.Child("rooms").Child(room.ToString()).PutAsync(gameRoom);
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