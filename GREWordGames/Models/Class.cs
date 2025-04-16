namespace GREWordGames.Models
{
    public class UserDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordReentered {  get; set; }

    }

    public class UserAuthenticated
    {
        public bool Condition { get; set; }
        public string Name { get; set; }
    }

    public class WordMetadata
    {
        public int id { get; set; }
        public string wordMeaning { get; set; }
    }

    public class UserMetadata
    {
        public string username { get; set; } = "";
        public int wordCount { get; set; } = 0;
        public string words { get; set; } = "[]";
        public string dateAdded { get; set; } = "[]";
        public string proficiency { get; set; } = "[]";
        public string lastAccessedDevice { get; set; } = "";
        public bool stateLoggedIn { get; set; } = true;
        public bool statePlaying { get; set; } = false;
        public bool stateWaitingRoom { get; set; } = false;
        public int stateRoomNumber { get; set; } = -1;
    }

    public class UserClass
    {
        public List<string> wordList { get; set; }
        public List<string> dateAddedList { get; set; }
        public List<string> proficiencyList { get; set; }
    }

    public class AddWord
    {
        public string Word { get; set; }
    }

    public class Message
    {
        public string NotificationMessage { get; set; }
    }

    public class WordMuseAPIJSON
    {
        public string word { get; set; }
        public int score { get; set; }
        public int numSyllabus { get; set; }
        public List<string> defs { get; set; }
    }

    public class WordAndWordMeaning
    {
        public string word { get; set; }
        public string wordMeaning { get; set; }
        public int wordIndex { get; set; }
    }

    public class AllWords
    {
        public List<string> words { get; set; }
        public List<string> outcome { get; set; }
    }

    public class WordViewModel
    {
        public UserMetadata UserMetadata { get; set; }
        public AddWord AddWord { get; set; }
        public Message Message { get; set; }
    }

    public class LoginClass
    {
        public UserDetails UserDetails { get; set; }
        public Message Message { get; set; }
    }

    public class RoomDetails
    {
        public required int GameRoom { get; set; }
        public string Password { get; set; }
    }

    public class FirebaseRoomDetails
    {
        public bool occupied { get; set; }
        public bool player2JoinFlag { get; set; }
        public bool startFlag { get; set; }
        public string password { get; set; }
        public string player1 { get; set; }
        public string player2 { get; set; }
    }
}
