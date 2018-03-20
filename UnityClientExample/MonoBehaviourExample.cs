//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;
//using DarkRift;
//using DarkRift.Client;
//using DarkRift.Client.Unity;
//using SpawnerHandler.Packets;
//using UnityEngine;
//using Utils;
//using Utils.Extensions;
//using Utils.Messages.Requests;
//using Utils.Messages.Response;
//using Security = Utils.Security;

//public class TundraNetClient : MonoBehaviour
//{
//    //Before any auth-method (login, register, ...) can be used, you have to call "RequestAESKey()" once
//    public bool RequestAESKeyOnStart;
//    public UnityClient Client;

//    public event Action LoggedIn;
//    public event Action<ResponseStatus, string> LogInFailed;
//    public event Action Registered;
//    public event Action LoggedOut;

//    private bool aeskeyPending;

//    public bool IsLoggedIn { get; protected set; }

//    private int RsaKeySize = 512;
//    private bool _isLoggingIn;
//    private RSACryptoServiceProvider _clientsCsp;
//    private RSAParameters _parameters;
//    private string _aesKey = string.Empty;

//    private void Awake()
//    {
//        DontDestroyOnLoad(transform.gameObject);
//    }

//    private void Start()
//    {
//        Client.Disconnected += ClientOnDisconnected;
//        Client.MessageReceived += ClientOnMessageReceived;
//    }

//    private void Update()
//    {
//        if (RequestAESKeyOnStart && !aeskeyPending)
//        {
//            if (Client.Connected)
//            {
//                aeskeyPending = true;
//                RequestAesKey();
//            }
//        }
//    }
//    private void ClientOnDisconnected(object sender, DisconnectedEventArgs disconnectedEventArgs)
//    {
//        IsLoggedIn = false;
//    }

//    private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs e)
//    {
//        using (var message = e.GetMessage())
//        {
//            switch (message.Tag)
//            {
//                case MessageTags.RequestAesKeyResponse:
//                    HandleRequestAesKeyResult(message);
//                    break;
//                case MessageTags.LoginSuccessResponse:
//                    HandleLoginSuccess(message);
//                    break;
//                case MessageTags.LoginFailedResponse:
//                    HandleLoginFailed(message);
//                    break;
//                case MessageTags.RegisterAccountSuccess:
//                    HandleRegisterAccountSuccess();
//                    break;
//                case MessageTags.RegisterAccountFailed:
//                    HandleRegisterAccountFailed(message);
//                    break;
//                case MessageTags.ResetPasswordSuccess:
//                    HandlePasswordResetSuccess();
//                    break;
//                case MessageTags.ResetPasswordFailed:
//                    HandlePasswordResetFailed(message);
//                    break;
//            }
//        }
//    }

//    #region AUTH
//    private void HandlePasswordResetFailed(Message message)
//    {
//        var data = message.Deserialize<RequestFailedMessage>();
//        if (data != null)
//        {
//            //Handle
//        }
//    }

//    private void HandlePasswordResetSuccess()
//    {
//        //Handle
//    }

//    private void HandleRegisterAccountFailed(Message message)
//    {
//        var data = message.Deserialize<RequestFailedMessage>();
//        if (data != null)
//        {
//            //Handle
//        }
//    }

//    private void HandleRegisterAccountSuccess()
//    {
//        if (Registered != null)
//        {
//            Registered.Invoke();
//        }

//        _isLoggingIn = false;
//    }

//    private void HandleLoginFailed(Message message)
//    {
//        var data = message.Deserialize<RequestFailedMessage>();
//        if (data != null)
//        {
//            if (LogInFailed != null)
//            {
//                LogInFailed.Invoke(data.Status, data.Reason);
//            }
//        }
//        _isLoggingIn = false;
//    }

//    private void HandleLoginSuccess(Message message)
//    {
//        var data = message.Deserialize<LoginSuccessMessage>();
//        if (data != null)
//        {
//            if (data.Status == ResponseStatus.Success)
//            {
//                IsLoggedIn = true;
//                if (LoggedIn != null)
//                {
//                    LoggedIn.Invoke();
//                }
//            }
//            _isLoggingIn = false;
//        }
//    }

//    private void HandleRequestAesKeyResult(Message message)
//    {
//        using (DarkRiftReader reader = message.GetReader())
//        {
//            var response = reader.ReadBytes();

//            if (response == null)
//            {
//                Console.WriteLine("Failed to receive aes key");
//                return;
//            }

//            var decrypted = _clientsCsp.Decrypt(response, false);
//            _aesKey = Encoding.Unicode.GetString(decrypted);
//        }
//    }

//    public void RequestAesKey()
//    {
//        if (Client.Connected)
//        {
//            //Request AES key
//            _clientsCsp = new RSACryptoServiceProvider(RsaKeySize);
//            _parameters = _clientsCsp.ExportParameters(false);

//            // Serialize public key
//            var sw = new StringWriter();
//            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
//            xs.Serialize(sw, _parameters);

//            using (var writer = DarkRiftWriter.Create(Encoding.Unicode))
//            {
//                writer.Write(sw.ToString());
//                Client.SendMessage(Message.Create(MessageTags.RequestAesKey, writer), SendMode.Reliable);
//            }
//        }
//    }

//    public void LogIn(string email, string password)
//    {
//        if (!Client.Connected)
//        {
//            return;
//        }

//        if (string.IsNullOrEmpty(_aesKey))
//        {
//            Debug.Log("You have to request an AES-Key before you can login");
//            return;
//        }

//        if (_isLoggingIn)
//        {
//            return;
//        }

//        _isLoggingIn = true;

//        var data = new Dictionary<string, string>
//            {
//                {"email", email},
//                {"password", password}
//            };

//        var encryptedData = Security.EncryptAES(data.ToBytes(), _aesKey);
//        using (var writer = DarkRiftWriter.Create())
//        {
//            writer.Write(encryptedData);
//            Client.SendMessage(Message.Create(MessageTags.LogIn, writer), SendMode.Reliable);
//        }
//    }

//    public void LogOut()
//    {
//        if (!Client.Connected)
//        {
//            return;
//        }

//        Client.Disconnect();
//        if (LoggedOut != null)
//        {
//            LoggedOut.Invoke();
//        }
//    }

//    public void Register(string email, string password)
//    {
//        var data = new Dictionary<string, string>
//            {
//                {"email", email},
//                {"password", password},
//            };

//        var encryptedData = Security.EncryptAES(data.ToBytes(), _aesKey);
//        using (var writer = DarkRiftWriter.Create())
//        {
//            writer.Write(encryptedData);
//            Client.SendMessage(Message.Create(MessageTags.RegisterAccount, writer), SendMode.Reliable);
//        }
//    }

//    public void RequestPasswordReset(string eMail, string code, string newPassword)
//    {
//        Client.SendMessage(
//            Message.Create(MessageTags.ResetPassword,
//                new RequestResetPasswordMessage { EMail = eMail, Code = code, NewPassword = newPassword }),
//            SendMode.Reliable);
//    }

//    public void RequestPasswordResetCode(string eMail)
//    {
//        //Fire & Forget
//        Client.SendMessage(
//            Message.Create(MessageTags.RequestPasswordResetCode, new RequestWithStringMessage { String = eMail }),
//            SendMode.Reliable);
//    }

//    public void ConfirmEmail(string email, string code)
//    {
//        Client.SendMessage(
//            Message.Create(MessageTags.ConfirmEmail, new RequestEmailConfirmationMessage { String = email, Code = code }),
//            SendMode.Reliable);
//    }

//    public void RequestNewEmailConfirmationCode(string email)
//    {
//        Client.SendMessage(
//            Message.Create(MessageTags.RequestNewEmailConfirmationCode, new RequestWithStringMessage { String = email }),
//            SendMode.Reliable);
//    }
//    #endregion

//    #region SPAWNING

//    public void RequestSpawn(string region = "")
//    {
//        Client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMaster, new RequestSpawnFromClientToMasterMessage
//        {
//            Region = region
//        }), SendMode.Reliable);
//    }

//    #endregion
//}

