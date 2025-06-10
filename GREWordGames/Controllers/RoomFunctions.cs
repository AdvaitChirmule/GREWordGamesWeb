using System.Collections.Specialized;
using System.Diagnostics;
using GREWordGames.Models;

namespace GREWordGames.Controllers
{
    public class RoomFunctions
    {
        private ISession _session;
        public int RoomNumber { get; set; } = -1;
        public string Password { get; set; }

        private string _token;
        private string _uid;

        private bool valid = false;

        private readonly FirebaseUserAPI _firebaseUserAPI;
        private readonly FirebaseGameRoomAPI _firebaseGameRoomAPI;
        private readonly CommonFunctions _commonFunctions;

        public RoomFunctions(string token, string uid, ISession session)
        {
            _token = token;
            _uid = uid;
            _firebaseUserAPI = new FirebaseUserAPI(_token, _uid);
            _firebaseGameRoomAPI = new FirebaseGameRoomAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public RoomFunctions(int roomNumber, string token, string uid, ISession session)
        {
            RoomNumber = roomNumber;
            _token = token;
            _uid = uid;
            _firebaseUserAPI = new FirebaseUserAPI(_token, _uid);
            _firebaseGameRoomAPI = new FirebaseGameRoomAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public RoomFunctions(string password, string token, string uid, ISession session)
        {
            Password = password;
            _token = token;
            _uid = uid;
            _firebaseUserAPI = new FirebaseUserAPI(_token, _uid);
            _firebaseGameRoomAPI = new FirebaseGameRoomAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }
        public RoomFunctions(int roomNumber, string password, string token, string uid, ISession session)
        {
            RoomNumber = roomNumber;
            Password = password;
            _token = token;
            _uid = uid;
            _firebaseUserAPI = new FirebaseUserAPI(_token, _uid);
            _firebaseGameRoomAPI = new FirebaseGameRoomAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public async Task SetRoomNumber(string password, string name)
        {
            RoomNumber = await _firebaseGameRoomAPI.GetNextFreeRoom(password, name);
            _session.SetInt32("roomNumber", RoomNumber);
        }

        public void SetGameRoomDetails(int rounds, bool exclusive)
        {
            if (rounds == 0)
            {
                rounds = 5;
            }

            _session.SetInt32("rounds", rounds);
            if (exclusive)
            {
                _session.SetInt32("exclusive", 1);
            }
            else
            {
                _session.SetInt32("exclusive", 0);
            }
        }

        public bool RoomAssigned()
        {
            RoomNumber = _session.GetInt32("roomNumber") ?? -1;
            if (RoomNumber != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetRoomNumber()
        {
            RoomNumber = _session.GetInt32("roomNumber") ?? -1;
            return RoomNumber;
        }

        public async Task<(bool, string)> VerifyRoomDetails(int roomNumber, string password, string name2)
        {
            int maxRoomCount = await _firebaseGameRoomAPI.GetTotalRoomCount();
            if ((roomNumber > maxRoomCount) || (roomNumber < 1))
            {
                return (false, "Invalid Room Number");
            }
            else
            {
                return await _firebaseGameRoomAPI.VerifyCredentials(roomNumber, password, name2, _uid);
            }
        }

        public async Task<String> WaitForPlayers(int roomNumber)
        {
            return await _firebaseGameRoomAPI.WaitForPlayers(roomNumber);
        }

        public async Task CloseRoom(int roomNumber)
        {
            await _firebaseGameRoomAPI.CloseRoom(roomNumber);
        }

        public async Task<bool> WaitToStart(int roomNumber)
        {
            return await _firebaseGameRoomAPI.WaitToStart(roomNumber);
        }

        public async Task<bool> StartGame()
        {
            int rounds = _session.GetInt32("rounds") ?? 5;
            int exclusive = _session.GetInt32("exclusive") ?? 0;
            bool overlap = false;
            if (exclusive == 1)
            {
                overlap = true;
            }

            int roomNumber = GetRoomNumber();

            string wordListHostRaw = await _firebaseUserAPI.GetUserWordList();

            string guestUid = await _firebaseGameRoomAPI.GetGuestUID(roomNumber);
            string wordListGuestRaw = await _firebaseUserAPI.GetUserWordList(guestUid);

            List<string> wordListHost = _commonFunctions.ConvertStringToList(wordListHostRaw);
            List<string> wordListGuest = _commonFunctions.ConvertStringToList(wordListGuestRaw);

            List<string> commonWords = _commonFunctions.FindCommonWordsForNRounds(wordListHost, wordListGuest, rounds, overlap);

            string commonWordsRaw = _commonFunctions.ConvertListToString(commonWords);

            _session.SetInt32("GamePlayer", 1);
            valid = true;

            return await _firebaseGameRoomAPI.StartGame(roomNumber, commonWordsRaw, rounds);
        }

        public bool IsValid()
        {
            return valid;
        }

        public async Task<List<string>> GetWordList(int roomNumber)
        {
            string wordListRaw = await _firebaseGameRoomAPI.GetGameWords(roomNumber);
            List<string> wordList = _commonFunctions.ConvertStringToList(wordListRaw);
            return wordList;
        }
    }
}
