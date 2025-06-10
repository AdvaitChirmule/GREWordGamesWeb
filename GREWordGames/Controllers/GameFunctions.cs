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
    }
}
