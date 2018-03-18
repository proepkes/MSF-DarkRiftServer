using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DarkRift;
using DarkRift.Server;
using TundraNetServerPluginData;

namespace TundraServerPlugins
{
    public class SecurityPlugin : Plugin
    {

        public SecurityPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {

            ClientManager.ClientConnected += ClientManagerOnClientConnected;
            ClientManager.ClientDisconnected += ClientManagerOnClientDisconnected;
        }

        private void ClientManagerOnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += ClientOnMessageReceived;
        }

        private void ClientOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case TundraNetTags.RequestAesKey:
                        if (_encryptionData.ContainsKey(e.Client))
                        {
                            e.Client.SendMessage(Message.Create(), )
                        }
                        HandleAesKeyRequest(message);
                        break;
                }
            }
        }



      

        //public void GetAesKey(Action<string> callback, IClient connection)
        //{
        //    _encryptionData.TryGetValue(connection, out EncryptionData data);

        //    if (data == null)
        //    {
        //        data = new EncryptionData();
        //        _encryptionData[connection] = data;

        //        data.ClientsCsp = new RSACryptoServiceProvider(RsaKeySize);

        //        // Generate keys
        //        data.ClientsPublicKey = data.ClientsCsp.ExportParameters(false);
        //    }

        //    if (data.ClientAesKey != null)
        //    {
        //        // We already have an aes generated for this connection
        //        callback.Invoke(data.ClientAesKey);
        //        return;
        //    }

        //    // Serialize public key
        //    var sw = new StringWriter();
        //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        //    xs.Serialize(sw, data.ClientsPublicKey);


        //    using (DarkRiftWriter writer = DarkRiftWriter.Create())
        //    {
        //        writer.Write(transform.position.x);
        //        writer.Write(transform.position.y);

        //        using (Message message = Message.Create(TundraNetServerPluginData.TundraNetTags..MovePlayerTag, writer))
        //            Client.SendMessage(message, SendMode.Unreliable);
        //    }
        //    // Send the request
        //    connection.SendMessage((short)MsfOpCodes.AesKeyRequest, sw.ToString(), (status, response) =>
        //    {
        //        if (data.ClientAesKey != null)
        //        {
        //            // Aes is already decrypted.
        //            callback.Invoke(data.ClientAesKey);
        //            return;
        //        }

        //        if (status != ResponseStatus.Success)
        //        {
        //            // Failed to get an aes key
        //            callback.Invoke(null);
        //            return;
        //        }

        //        var decrypted = data.ClientsCsp.Decrypt(response.AsBytes(), false);
        //        data.ClientAesKey = Encoding.Unicode.GetString(decrypted);

        //        callback.Invoke(data.ClientAesKey);
        //    });
        //}
    }
}
