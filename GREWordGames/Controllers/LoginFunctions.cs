using Firebase.Auth;
using Firebase.Database;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using GREWordGames.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace GREWordGames.Controllers
{
    public class LoginFunctions
    {
        private ISession _session;
        private readonly FirebaseAuthClient _firebaseAuth;
        public LoginFunctions(FirebaseAuthClient firebaseAuth, ISession session)
        {
            _firebaseAuth = firebaseAuth;
            _session = session;
        }

        public string CheckMessages(Object message)
        {
            if (message == null)
            {
                return "";
            }
            else
            {
                return (string)message;
            }
        }

        public LoginClass PrepareModel(string message)
        {
            var emptyUserDetails = new UserDetails { };
            var notificationMessage = new Message { NotificationMessage = message };

            var model = new LoginClass
            {
                UserDetails = emptyUserDetails,
                Message = notificationMessage
            };

            return model;
        }

        public bool CheckEmailNotEmpty(string email)
        {
            if (email == null) { 
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckPasswordNotEmpty(string password)
        {
            if (password == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckBothPasswordsSame(string password, string passwordReentered)
        {
            if (password == passwordReentered)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> VerifyLoginDetails(string email, string password)
        {
            LoginAPI _loginAPI = new LoginAPI(_firebaseAuth);
            var userCredentials = (UserCredential) await _loginAPI.LoginUser(email, password);

            if (userCredentials != null)
            {
                var token = await userCredentials.User.GetIdTokenAsync();
                var uid = userCredentials.User.Uid;
                var displayName = userCredentials.User.Info.DisplayName;

                _session.SetString("token", token);
                _session.SetString("uid", uid);
                _session.SetString("name", displayName);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> RegisterUser(UserDetails userDetails)
        {
            LoginAPI _loginAPI = new LoginAPI(_firebaseAuth);
            var userCredentials = (UserCredential) await _loginAPI.RegisterUser(userDetails.Email, userDetails.Password, userDetails.Name);
            if (userCredentials != null)
            {
                var token = await userCredentials.User.GetIdTokenAsync();
                var uid = userCredentials.User.Uid;
                _session.SetString("token", token);
                _session.SetString("uid", uid);
                _session.SetString("name", userDetails.Name);


                FirebaseUserAPI _firebaseAPI = new FirebaseUserAPI(token, uid);
                await _firebaseAPI.AddUser(userDetails.Name);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
