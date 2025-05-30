using Firebase.Auth;
using FirebaseAdmin.Auth;
using GREWordGames.Models;

namespace GREWordGames.Controllers
{
    public class LoginAPI
    {
        private readonly FirebaseAuthClient _firebaseAuth;
        public LoginAPI(FirebaseAuthClient firebaseAuth)
        {
            _firebaseAuth = firebaseAuth;
        }

        public async Task<object> LoginUser(string email, string password)
        {
            try
            {
                var userCredentials = await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                return userCredentials;
            }
            catch
            {
                return null;
            }
        }

        public async Task<object> RegisterUser(string email, string password, string name)
        {
            try
            {
                var userCredentials = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password, name);
                return userCredentials;
            }
            catch
            {
                return null;
            }
        }
    }
}
