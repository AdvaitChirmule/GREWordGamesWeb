using Firebase.Database;
using Firebase.Database.Query;
using GREWordGames.Models;

namespace GREWordGames.Controllers
{
    public class FirebaseGameRoomAPI
    {
        private readonly HttpClient _httpClient;
        private string _token;
        private string _uid;
        private FirebaseClient _firebaseClient;

        public FirebaseGameRoomAPI(string token, string uid)
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

        public async Task<int> GetNextFreeRoom(string password, string name1)
        {
            int maxRoomCount = await _firebaseClient.Child("rooms").Child("roomCount").OnceSingleAsync<int>();
            for (int i = 1; i < maxRoomCount + 1; i++)
            {
                bool roomOccupied = await _firebaseClient.Child("rooms").Child(i.ToString()).Child("Occupied").OnceSingleAsync<bool>();
                if (roomOccupied == false)
                {
                    FirebaseRoomDetails roomDetails = new FirebaseRoomDetails { Occupied = true, Player2JoinFlag = false, StartFlag = false, Password = password, Player1 = name1, Player2 = "" };
                    await _firebaseClient.Child("rooms").Child(i.ToString()).PutAsync(roomDetails);
                    return i;
                }
            }

            return -1;
        }

        public async Task<int> GetTotalRoomCount()
        {
            int maxRoomCount = await _firebaseClient.Child("rooms").Child("roomCount").OnceSingleAsync<int>();
            return maxRoomCount;
        }

        public async Task<bool> IsRoomOccupied(int room)
        {
            bool roomOccupied = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("Occupied").OnceSingleAsync<bool>();
            return roomOccupied;
        }

        public async Task<bool> IsRoomWaitingForPlayer2(int room)
        {
            bool player2Joined = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("Player2JoinFlag").OnceSingleAsync<bool>();
            return !player2Joined;
        }

        public async Task<(bool, string)> VerifyCredentials(int room, string password, string name2, string uid2)
        {
            bool roomOccupied = await IsRoomOccupied(room);
            if (roomOccupied)
            {
                bool player2Joined = await IsRoomWaitingForPlayer2(room);
                if (player2Joined)
                {
                    string roomPassword = await _firebaseClient.Child("rooms").Child(room.ToString()).Child("Password").OnceSingleAsync<string>();
                    if (roomPassword == password)
                    {
                        FirebaseRoomDetails gameRoom = await _firebaseClient.Child("rooms").Child(room.ToString()).OnceSingleAsync<FirebaseRoomDetails>();
                        gameRoom.Player2JoinFlag = true;
                        gameRoom.Player2 = name2;
                        gameRoom.Player2Uid = uid2;
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

        public async Task<String> WaitForPlayers(int roomNumber)
        {
            int timeToLive = 300;
            while (timeToLive > 0)
            {
                bool player2JoinFlag = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Player2JoinFlag").OnceSingleAsync<bool>();
                if (player2JoinFlag)
                {
                    return await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Player2").OnceSingleAsync<String>();
                }

                timeToLive = timeToLive - 1;
                await Task.Delay(1000);
            }

            return "";
        }

        public async Task CloseRoom(int roomNumber)
        {
            FirebaseRoomDetails emptyRoom = new FirebaseRoomDetails();
            await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).PutAsync(emptyRoom);
        }

        public async Task<bool> StartGame(int roomNumber, string wordList, int rounds)
        {
            FirebaseRoomDetails room = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).OnceSingleAsync<FirebaseRoomDetails>();
            room.WordList = wordList;
            room.Rounds = rounds;
            room.StartFlag = true;
            room.StartTime = DateTime.UtcNow.AddSeconds(10).ToString();
            await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).PutAsync(room);
            return true;
        }

        public async Task<bool> WaitToStart(int roomNumber)
        {
            int checkTimer = 0;
            while (true)
            {
                bool start = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("StartFlag").OnceSingleAsync<bool>();
                if (start)
                {
                    return true;
                }
                checkTimer = checkTimer + 1;
                if (checkTimer % 5 == 0)
                {
                    bool stillExists = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Occupied").OnceSingleAsync<bool>();
                    if (!stillExists)
                    {
                        return false;
                    }
                }

                await Task.Delay(1000);
            }
        }

        public async Task<string> GetGuestUID(int roomNumber)
        {
            string guestUid = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Player2Uid").OnceSingleAsync<string>();
            return guestUid;
        }

        public async Task<string> GetGameWords(int roomNumber)
        {
            string wordList = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("WordList").OnceSingleAsync<string>();
            return wordList;
        }

        public async Task<string> GetStartTime(int roomNumber)
        {
            string startTime = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("StartTime").OnceSingleAsync<string>();
            return startTime;
        }

        public async Task<int> GetRounds(int roomNumber)
        {
            int rounds = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Rounds").OnceSingleAsync<int>();
            return rounds;
        }

        public async Task<bool> GetWhetherPlayerFirstTurn(int roomNumber)
        {
            string guestUid = await _firebaseClient.Child("rooms").Child(roomNumber.ToString()).Child("Player2Uid").OnceSingleAsync<string>();
            if (_uid == guestUid)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
