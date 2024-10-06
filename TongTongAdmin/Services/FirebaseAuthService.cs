using Firebase.Auth;
using System.Threading.Tasks;

namespace TongTongAdmin.Services
{
    public class FirebaseAuthService
    {
        public static FirebaseAuthLink Auth { get; private set; }

        private static readonly string _firebaseApiKey = "AIzaSyDcGZSUAF5LDRV_bVPBoa_H1oUZuJfMXgg";

        public static async Task<string> SignIn(string oauthAccessToken)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(_firebaseApiKey));
            Auth = await authProvider.SignInWithOAuthAsync(FirebaseAuthType.Google, oauthAccessToken);

            return Auth.FirebaseToken;
        }
    }
}