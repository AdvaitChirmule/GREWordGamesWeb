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
        public int Id { get; set; }
        public string WordMeaning { get; set; }
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
}
