using System;
using Utils;

namespace Tundra
{
    public interface ITundraClient
    {
        event Action LoggedIn;
        event Action<ResponseStatus, string> LogInFailed;
        event Action Registered;
        event Action LoggedOut;
        bool Connected { get; }
        bool IsLoggedIn { get; }
        void RequestAesKey();
        void LogIn(string email, string password);
        void LogOut();
        void Register(string email, string password);
        void RequestPasswordReset(string eMail, string code, string newPassword);
        void RequestPasswordResetCode(string eMail);
        void ConfirmEmail(string email, string code);
        void RequestNewEmailConfirmationCode(string email);
        void RequestSpawn(string region);
    }
}