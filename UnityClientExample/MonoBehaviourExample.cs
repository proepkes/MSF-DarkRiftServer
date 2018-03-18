//using System;
//using System.Collections;
//using DarkRift;
//using DarkRift.Client;
//using DarkRift.Client.Unity;
//using Tundra;
//using UnityClientExample;
//using UnityEngine;

//public class TundraNetClient : MonoBehaviour
//{
//    private class TundraNetClientAdapter : IDarkRiftUnityClient
//    {
//        private readonly UnityClient _client;

//        public event EventHandler<DisconnectedEventArgs> Disconnected;
//        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

//        public TundraNetClientAdapter(UnityClient client)
//        {
//            _client = client;
//            _client.MessageReceived += (sender, args) =>
//            {
//                if (MessageReceived != null)
//                    MessageReceived.Invoke(sender, args);
//            };
//            _client.Disconnected += (sender, args) =>
//            {
//                if (Disconnected != null)
//                    Disconnected.Invoke(sender, args);
//            };
//        }
//        public bool SendMessage(Message create, SendMode reliable)
//        {
//            return _client.SendMessage(create, reliable);
//        }

//        public bool Disconnect()
//        {
//            return _client.Disconnect();
//        }

//        public bool Connected
//        {
//            get
//            {
//                return _client.Connected;
//            }
//        }
//    }

//    public bool RequestAESKeyOnStart;
//    public UnityClient UnityClient;
//    public ITundraClient Client;


//    private void Awake()
//    {
//        DontDestroyOnLoad(transform.gameObject);
//    }

//    void Start()
//    {
//        Client = TundraClientFactory.Create(new TundraNetClientAdapter(UnityClient));

//        if (RequestAESKeyOnStart)
//            StartCoroutine(GetAesKey());
//    }

//    #region AUTH

//    private IEnumerator GetAesKey()
//    {
//        while (!Client.Connected)
//        {
//            yield return new WaitForSeconds(.2f);
//        }

//        if (Client.Connected)
//        {
//            Client.RequestAesKey();
//        }
//    }

//    public void LogIn(string email, string password)
//    {
//        Client.LogIn(email, password);
//    }

//    public void LogOut()
//    {
//        Client.LogOut();
//    }

//    public void Register(string email, string password)
//    {
//        Client.Register(email, password);
//    }

//    public void RequestPasswordReset(string eMail, string code, string newPassword)
//    {
//        Client.RequestPasswordReset(eMail, code, newPassword);
//    }

//    public void RequestPasswordResetCode(string eMail)
//    {
//        Client.RequestPasswordResetCode(eMail);
//    }

//    public void ConfirmEmail(string email, string code)
//    {
//        Client.ConfirmEmail(email, code);
//    }

//    public void RequestNewEmailConfirmationCode(string email)
//    {
//        Client.RequestNewEmailConfirmationCode(email);
//    }

//    #endregion

//    #region ROOM

//    #endregion
//}
