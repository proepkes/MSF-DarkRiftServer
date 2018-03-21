using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using DarkRift;
using DarkRift.Server;
using ServerPlugins.Database;
using ServerPlugins.Mail;
using Utils;
using Utils.Extensions;
using Utils.Messages.Requests;
using Utils.Messages.Responses;

namespace ServerPlugins.Authentication
{
    public class AuthenticationPlugin : Plugin
    {
        private readonly Dictionary<IClient, EncryptionData> _encryptionData;

        public readonly Dictionary<IClient, SqlAccountData> LoggedInClients;
        private MySQLPlugin _database;

        private MailPlugin _mailPlugin;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public int EMailMaxChars { get; set; }
        public int EMailMinChars { get; set; }
        public string PasswordResetEmailBody { get; set; }
        public string ConfirmEmailBody { get; set; }

        public AuthenticationPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            LoggedInClients = new Dictionary<IClient, SqlAccountData>();
            _encryptionData = new Dictionary<IClient, EncryptionData>();

            EMailMinChars = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(EMailMinChars)));
            EMailMaxChars = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(EMailMaxChars)));
            PasswordResetEmailBody = pluginLoadData.Settings.Get(nameof(PasswordResetEmailBody));
            ConfirmEmailBody = pluginLoadData.Settings.Get(nameof(ConfirmEmailBody));

            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            _mailPlugin = PluginManager.GetPluginByType<MailPlugin>();
            _database = PluginManager.GetPluginByType<MySQLPlugin>();
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += ClientOnMessageReceived;
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _encryptionData.Remove(e.Client);
            if (LoggedInClients.ContainsKey(e.Client)) LoggedInClients.Remove(e.Client);
        }

        private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case MessageTags.RequestAesKey:
                        HandleRequestAesKey(e.Client, message);
                        break;
                    case MessageTags.RequestPermissionLevel:
                        HandleRequestPermissionLevel(e.Client, message);
                        break;
                    case MessageTags.LogIn:
                        HandleLogin(e.Client, message);
                        break;
                    case MessageTags.RegisterAccount:
                        HandleRegisterAccount(e.Client, message);
                        break;
                    case MessageTags.RequestPasswordResetCode:
                        HandleRequestPasswordResetCode(message);
                        break;
                    case MessageTags.ResetPassword:
                        HandleRequestPasswordReset(e.Client, message);
                        break;
                    case MessageTags.ConfirmEmail:
                        HandleConfirmEmail(e.Client, message);
                        break;
                    case MessageTags.RequestNewEmailConfirmationCode:
                        HandleRequestNewEmailConfirmationCode(message);
                        break;
                }
            }
        }

        private void HandleRequestPermissionLevel(IClient client, Message message)
        {
            throw new NotImplementedException();
        }

        private void HandleRequestNewEmailConfirmationCode(Message message)
        {
            var data = message.Deserialize<RequestFromUserMessage>();
            if (data != null)
            {
                var account = _database.GetAccount(data.EMail);

                //Only send to registered accounts
                if (account == null || account.IsEmailConfirmed)
                    return;

                SendEmailConfirmationCode(account);
            }
        }

        private void HandleConfirmEmail(IClient client, Message message)
        {
            var data = message.Deserialize<ConfirmEmailMessage>();
            if (data != null)
            {
                var code = data.Code;

                var account = _database.GetAccount(data.EMail);
                if (account == null)
                {
                    client.SendMessage(
                        Message.Create(MessageTags.ConfirmEmailFailed,
                            new FailedMessage
                            {
                                Status = ResponseStatus.Error,
                                Reason = "Invalid"
                            }), SendMode.Reliable);
                    return;
                }

                if (account.IsEmailConfirmed)
                {
                    client.SendMessage(Message.CreateEmpty(MessageTags.ConfirmEmailSuccess), SendMode.Reliable);
                    return;
                }

                var requiredCode = _database.GetEmailConfirmationCode(data.EMail);
                if (requiredCode != code)
                {
                    client.SendMessage(
                        Message.Create(MessageTags.ConfirmEmailFailed,
                            new FailedMessage
                            {
                                Status = ResponseStatus.Error,
                                Reason = "Invalid"
                            }), SendMode.Reliable);
                    return;
                }

                // Confirm e-mail
                account.IsEmailConfirmed = true;

                // Update account
                _database.UpdateAccount(account);

                // Respond with success
                client.SendMessage(Message.CreateEmpty(MessageTags.ConfirmEmailSuccess), SendMode.Reliable);
            }
        }

        private void HandleRequestPasswordReset(IClient client, Message message)
        {
            var data = message.Deserialize<ResetPasswordMessage>();
            if (string.IsNullOrEmpty(data?.EMail) || string.IsNullOrEmpty(data.Code) ||
                string.IsNullOrEmpty(data.NewPassword))
            {
                client.SendMessage(
                    Message.Create(MessageTags.ResetPasswordFailed,
                        new FailedMessage
                        {
                            Status = ResponseStatus.Unauthorized,
                            Reason = "Invalid request"
                        }), SendMode.Reliable);
                return;
            }

            var resetData = _database.GetPasswordResetData(data.EMail);
            if (resetData?.Code == null || !resetData.Code.Equals(data.Code))
            {
                client.SendMessage(
                    Message.Create(MessageTags.ResetPasswordFailed,
                        new FailedMessage
                        {
                            Status = ResponseStatus.Unauthorized,
                            Reason = "Invalid code"
                        }), SendMode.Reliable);
                return;
            }

            var account = _database.GetAccount(data.EMail);

            // Delete (overwrite) code used
            _database.SavePasswordResetCode(account, null);

            account.Password = Security.CreateHash(data.NewPassword);
            _database.UpdateAccount(account);
            client.SendMessage(Message.CreateEmpty(MessageTags.ResetPasswordSuccess), SendMode.Reliable);
        }

        private void HandleRequestPasswordResetCode(Message message)
        {
            var data = message.Deserialize<RequestFromUserMessage>();
            if (data != null)
            {
                var email = data.EMail;

                var account = _database.GetAccount(email);

                if (account == null) return;

                var code = Security.CreateRandomString(4);

                _database.SavePasswordResetCode(account, code);

                _mailPlugin.SendMail(account.Email, "Password Reset Code", string.Format(PasswordResetEmailBody, code));
            }
        }

        private void HandleRegisterAccount(IClient client, Message message)
        {
            if (!_encryptionData.ContainsKey(client) || _encryptionData[client] == null)
            {
                client.SendMessage(
                    Message.Create(MessageTags.RegisterAccountFailed,
                        new FailedMessage {Status = ResponseStatus.Error, Reason = "Insecure request"}),
                    SendMode.Reliable);
                return;
            }

            using (var reader = message.GetReader())
            {
                var aesKey = _encryptionData[client].AesKey;
                var encryptedData = reader.ReadBytes();
                var decrypted = Security.DecryptAES(encryptedData, aesKey);
                var data = new Dictionary<string, string>().FromBytes(decrypted);

                if (!data.ContainsKey("email") || !data.ContainsKey("password"))
                {
                    client.SendMessage(
                        Message.Create(MessageTags.RegisterAccountFailed,
                            new FailedMessage
                            {
                                Status = ResponseStatus.Error,
                                Reason = "Invalid registration request"
                            }), SendMode.Reliable);
                    return;
                }

                var email = data["email"].ToLower();
                var password = data["password"];

                if (!IsEmailValid(email))
                {
                    client.SendMessage(
                        Message.Create(MessageTags.RegisterAccountFailed,
                            new FailedMessage
                            {
                                Status = ResponseStatus.Error,
                                Reason = "Invalid or forbidden words in email"
                            }), SendMode.Reliable);
                    return;
                }

                if (email.Length < EMailMinChars || email.Length > EMailMaxChars)
                {
                    // Check if username length is good
                    client.SendMessage(
                        Message.Create(MessageTags.RegisterAccountFailed,
                            new FailedMessage {Status = ResponseStatus.Error, Reason = "Invalid email length"}),
                        SendMode.Reliable);
                    return;
                }

                var account = _database.CreateAccountObject();
                account.Email = email;
                account.Password = Security.CreateHash(password);

                try
                {
                    _database.InsertNewAccount(account);
                    SendEmailConfirmationCode(account);

                    client.SendMessage(Message.CreateEmpty(MessageTags.RegisterAccountSuccess), SendMode.Reliable);
                }
                catch (Exception e)
                {
                    WriteEvent("Account already registered", LogType.Error, e);
                    client.SendMessage(
                        Message.Create(MessageTags.RegisterAccountFailed,
                            new FailedMessage {Status = ResponseStatus.Error, Reason = "Already registered"}),
                        SendMode.Reliable);
                }
            }
        }

        protected virtual void HandleRequestAesKey(IClient client, Message message)
        {
            if (_encryptionData.ContainsKey(client))
            {
                WriteEvent("Existing AesKeyRequested", LogType.Info);
                using (var writer = DarkRiftWriter.Create())
                {
                    writer.Write(_encryptionData[client].AesKeyEncrypted);
                    client.SendMessage(Message.Create(MessageTags.RequestAesKeyResponse, writer), SendMode.Reliable);
                }
            }
            else
            {
                using (var reader = message.GetReader())
                {
                    WriteEvent("Generated new AES key for " + client.RemoteTcpEndPoint, LogType.Info);
                    // Generate a random key
                    var aesKey = Security.CreateRandomString(8);

                    var clientsPublicKeyXml = reader.ReadString(Encoding.Unicode);

                    // Deserialize public key
                    var sr = new StringReader(clientsPublicKeyXml);
                    var xs = new XmlSerializer(typeof(RSAParameters));
                    var clientsPublicKey = (RSAParameters) xs.Deserialize(sr);

                    using (var csp = new RSACryptoServiceProvider())
                    {
                        csp.ImportParameters(clientsPublicKey);
                        var encryptedAes = csp.Encrypt(Encoding.Unicode.GetBytes(aesKey), false);

                        _encryptionData[client] = new EncryptionData {AesKey = aesKey, AesKeyEncrypted = encryptedAes};

                        using (var writer = DarkRiftWriter.Create())
                        {
                            writer.Write(encryptedAes);
                            client.SendMessage(Message.Create(MessageTags.RequestAesKeyResponse, writer),
                                SendMode.Reliable);
                        }
                    }
                }
            }
        }

        private void HandleLogin(IClient client, Message message)
        {
            if (LoggedInClients.ContainsKey(client))
            {
                client.SendMessage(
                    Message.Create(MessageTags.LoginFailedResponse,
                        new FailedMessage {Status = ResponseStatus.Unauthorized, Reason = "Already logged in"}),
                    SendMode.Reliable);
                return;
            }

            if (!_encryptionData.ContainsKey(client) || _encryptionData[client] == null)
            {
                client.SendMessage(
                    Message.Create(MessageTags.LoginFailedResponse,
                        new FailedMessage {Status = ResponseStatus.Unauthorized, Reason = "Insecure request"}),
                    SendMode.Reliable);
                return;
            }

            using (var reader = message.GetReader())
            {
                var aesKey = _encryptionData[client].AesKey;
                var encryptedData = reader.ReadBytes();
                var decrypted = Security.DecryptAES(encryptedData, aesKey);
                var data = new Dictionary<string, string>().FromBytes(decrypted);

                SqlAccountData accountData;
                //// ----------------------------------------------
                //// Username / Password authentication

                if (data.ContainsKey("email") && data.ContainsKey("password"))
                {
                    var email = data["email"];
                    var password = data["password"];

                    accountData = _database.GetAccount(email);
                    if (accountData == null)
                    {
                        client.SendMessage(
                            Message.Create(MessageTags.LoginFailedResponse,
                                new FailedMessage
                                {
                                    Status = ResponseStatus.Unauthorized,
                                    Reason = "Invalid credentials"
                                }), SendMode.Reliable);
                        return;
                    }

                    if (!Security.ValidatePassword(password, accountData.Password))
                    {
                        client.SendMessage(
                            Message.Create(MessageTags.LoginFailedResponse,
                                new FailedMessage
                                {
                                    Status = ResponseStatus.Unauthorized,
                                    Reason = "Invalid credentials"
                                }), SendMode.Reliable);
                        return;
                    }

                    if (!accountData.IsEmailConfirmed)
                    {
                        client.SendMessage(
                            Message.Create(MessageTags.LoginFailedResponse,
                                new FailedMessage
                                {
                                    Status = ResponseStatus.Unconfirmed,
                                    Reason = "EMail not confirmed"
                                }), SendMode.Reliable);
                        return;
                    }

                    var other = LoggedInClients.FirstOrDefault(user => user.Value.Email.Equals(accountData.Email));
                    other.Key?.Disconnect();

                    LoggedInClients.Add(client, accountData);

                    client.SendMessage(
                        Message.Create(MessageTags.LoginSuccessResponse,
                            new LoginSuccessMessage
                            {
                                Status = ResponseStatus.Success,
                                IsAdmin = accountData.IsAdmin,
                                IsGuest = accountData.IsGuest
                            }), SendMode.Reliable);
                }
                else
                {
                    client.SendMessage(
                        Message.Create(MessageTags.LoginFailedResponse,
                            new FailedMessage
                            {
                                Status = ResponseStatus.Unauthorized,
                                Reason = "Invalid request"
                            }), SendMode.Reliable);
                }
            }
        }

        private void SendEmailConfirmationCode(SqlAccountData accountData)
        {
            var code = Security.CreateRandomString(6);

            _database.SaveEmailConfirmationCode(accountData.Email, code);

            _mailPlugin.SendMail(accountData.Email, "E-mail confirmation", string.Format(ConfirmEmailBody, code));
        }

        protected virtual bool IsEmailValid(string email)
        {
            return !string.IsNullOrEmpty(email) && // If username is empty
                   email == email.Replace(" ", "") &&
                   !BadWordFilter.ContainsBadWords(email) && email.Contains("@") &&
                   email.Contains("."); // If username contains spaces
        }
    }
}