
namespace GREWordGames.Models
{
    public class UserDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordReentered { get; set; }

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

    public class HostRoomDetails
    {
        public int Rounds { get; set; } = 5;
    }

    public class FirebaseRoomDetails
    {
        public bool Occupied { get; set; } = false;
        public bool Player2JoinFlag { get; set; } = false;
        public bool StartFlag { get; set; } = false;
        public string Password { get; set; } = "";
        public string Player1 { get; set; } = "";
        public string Player2 { get; set; } = "";
        public string Player2WordList { get; set; } = "";
        public int Rounds { get; set; } = 0;
        public string WordList { get; set; } = "[]";

        public string StartTime { get; set; }
        public bool StartTimeP2Ack { get; set; } = false;

        public SaveRecord SaveRecord { get; set; } = new SaveRecord { Index = -1, Value = 0 };
        public DrawClass D0 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D1 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D2 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D3 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D4 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D5 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D6 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D7 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D8 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D9 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D10 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D11 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D12 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D13 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D14 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D15 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D16 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D17 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D18 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D19 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D20 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D21 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D22 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D23 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D24 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D25 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D26 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D27 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D28 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D29 { get; set; } = new DrawClass { Index = -1, Value = "" };
        public DrawClass D30 { get; set; } = new DrawClass { Index = -1, Value = "" };

    }

    public class SaveRecord
    {
        public int Index { get; set; }
        public int Value { get; set; }
    }

    public class DrawClass
    {
        public int Index { get; set; }
        public string Value { get; set; }
    }
}
