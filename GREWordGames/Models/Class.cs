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
        public string username { get; set; }
        public int wordCount { get; set; }
        public string words { get; set; }
        public string dateAdded { get; set; }
        public string proficiency { get; set; }
        public string lastAccessedDevice { get; set; }
        public bool stateLoggedIn { get; set; }
        public bool statePlaying { get; set; }
        public bool stateWaitingRoom { get; set; }
        public int stateRoomNumber { get; set; }
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

    public class WordViewModel
    {
        public UserMetadata UserMetadata { get; set; }
        public AddWord AddWord { get; set; }
        public Message Message { get; set; }
    }
}
