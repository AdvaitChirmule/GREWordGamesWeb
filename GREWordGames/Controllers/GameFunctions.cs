using GREWordGames.Models;

namespace GREWordGames.Controllers
{
    public class GameFunctions
    {
        private ISession _session;

        private string _token;
        private string _uid;

        private readonly FirebaseUserAPI _firebaseUserAPI;
        private readonly FirebaseGameRoomAPI _firebaseGameRoomAPI;
        private readonly CommonFunctions _commonFunctions;

        public GameFunctions(string token, string uid, ISession session)
        {
            _token = token;
            _uid = uid;
            _firebaseUserAPI = new FirebaseUserAPI(_token, _uid);
            _firebaseGameRoomAPI = new FirebaseGameRoomAPI(_token, _uid);
            _commonFunctions = new CommonFunctions();
            _session = session;
        }

        public async Task<DateTime> GetStartTime()
        {
            int roomNumber = _session.GetInt32("roomNumber") ?? -1;
            string startTime = await _firebaseGameRoomAPI.GetStartTime(roomNumber);
            DateTime dateTime = DateTime.Parse(startTime);
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
            return localTime;
        }

        public async Task<DateTime> GetStartTimeLocal()
        {
            int roomNumber = _session.GetInt32("roomNumber") ?? -1;
            string startTime = await _firebaseGameRoomAPI.GetStartTime(roomNumber);
            DateTime dateTime = DateTime.Parse(startTime);
            return dateTime;
        }

        public int GetRounds()
        {
            return _session.GetInt32("Rounds") ?? 5;
        }

        public bool GetWhetherPlayerFirstTurn()
        {
            if (_session.GetInt32("GamePlayer") == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetKthWord(int index)
        {
            string commonWordsRaw = _session.GetString("PlayerAllWords");
            List<string> commonWords = _commonFunctions.ConvertStringToList(commonWordsRaw);
            if (GetWhetherPlayerFirstTurn())
            {
                return commonWords[index * 2];
            }
            else
            {
                return commonWords[index * 2 + 1];
            }
        }

        public bool GuessWord(string word, int index)
        {
            string commonWordsRaw = _session.GetString("PlayerAllWords");
            List<string> commonWords = _commonFunctions.ConvertStringToList(commonWordsRaw);
            if (GetWhetherPlayerFirstTurn())
            {
                if (word == commonWords[index * 2 + 1])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (word == commonWords[index * 2])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
