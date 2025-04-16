using System.Collections.Specialized;
using System.Diagnostics;

namespace GREWordGames.Controllers
{
    public class RoomFunctions
    {
        private ISession _session;
        public int RoomNumber { get; set; } = -1;
        public string Password { get; set; }

        private readonly FirebaseFunctions _firebaseFunctions;

        public RoomFunctions(string password, string token, string uid, ISession session)
        {
            Password = password;
            _firebaseFunctions = new FirebaseFunctions(token, uid);
            _session = session;
        }
        public RoomFunctions(int roomNumber, string password, string token, string uid, ISession session)
        {
            RoomNumber = roomNumber;
            Password = password;
            _firebaseFunctions = new FirebaseFunctions(token, uid);
            _session = session;
        }

        public async Task SetRoomNumber(string password, string name)
        {
            RoomNumber = await _firebaseFunctions.GetNextFreeRoom(password, name);
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
            return await _firebaseFunctions.VerifyCredentials(roomNumber, password, name2);
        }
    }
}
