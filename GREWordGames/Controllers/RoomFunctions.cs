using System.Collections.Specialized;
using System.Diagnostics;

namespace GREWordGames.Controllers
{
    public class RoomFunctions
    {
        private ISession _session;
        public int RoomNumber { get; set; } = -1;
        public string Password { get; set; }

        private string _token;
        private string _uid;

        private readonly FirebaseAPI _firebaseAPI;
        private readonly CommonFunctions _commonFunctions;

        public RoomFunctions(string token, string uid, ISession session)
        {
            _token = token;
            _uid = uid;
            _firebaseAPI = new FirebaseAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public RoomFunctions(int roomNumber, string token, string uid, ISession session)
        {
            RoomNumber = roomNumber;
            _token = token;
            _uid = uid;
            _firebaseAPI = new FirebaseAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public RoomFunctions(string password, string token, string uid, ISession session)
        {
            Password = password;
            _token = token;
            _uid = uid;
            _firebaseAPI = new FirebaseAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }
        public RoomFunctions(int roomNumber, string password, string token, string uid, ISession session)
        {
            RoomNumber = roomNumber;
            Password = password;
            _token = token;
            _uid = uid;
            _firebaseAPI = new FirebaseAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public async Task SetRoomNumber(string password, string name)
        {
            RoomNumber = await _firebaseAPI.GetNextFreeRoom(password, name);
            _session.SetInt32("roomNumber", RoomNumber);
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
            int maxRoomCount = await _firebaseAPI.GetTotalRoomCount();
            if ((roomNumber > maxRoomCount) || (roomNumber < 1))
            {
                return (false, "Invalid Room Number");
            }
            else
            {
                return await _firebaseAPI.VerifyCredentials(roomNumber, password, name2, _uid);
            }
        }

        public async Task<String> WaitForPlayers(int roomNumber)
        {
            return await _firebaseAPI.WaitForPlayers(roomNumber);
        }

        public async Task CloseRoom(int roomNumber)
        {
            await _firebaseAPI.CloseRoom(roomNumber);
        }

        public async Task<bool> WaitToStart(int roomNumber)
        {
            return await _firebaseAPI.WaitToStart(roomNumber);
        }

        public async Task<bool> StartGame(int rounds)
        {
            int roomNumber = GetRoomNumber();

            string wordListHostRaw = await _firebaseAPI.GetUserWordList();

            string guestUid = await _firebaseAPI.GetGuestUID(roomNumber);
            string wordListGuestRaw = await _firebaseAPI.GetUserWordList(guestUid);

            List<string> wordListHost = _commonFunctions.ConvertStringToList(wordListHostRaw);
            List<string> wordListGuest = _commonFunctions.ConvertStringToList(wordListGuestRaw);

            List<string> commonWords = _commonFunctions.FindCommonWordsForNRounds(wordListHost, wordListGuest, rounds);

            string commonWordsRaw = _commonFunctions.ConvertListToString(commonWords);

            return await _firebaseAPI.StartGame(roomNumber, commonWordsRaw);
        }

        public async Task<List<string>> GetWordList(int roomNumber)
        {
            string wordListRaw = await _firebaseAPI.GetGameWords(roomNumber);
            List<string> wordList = _commonFunctions.ConvertStringToList(wordListRaw);
            return wordList;
        }
    }
}
