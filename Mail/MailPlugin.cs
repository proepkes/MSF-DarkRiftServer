using DarkRift;
using DarkRift.Server;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Mail
{
    public class MailPlugin : Plugin
    {
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);

        private SmtpClient SmtpClient;

        public string SmtpPassword { get; set; }

        public string SmtpUsername { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpHost { get; set; }
        public string SenderDisplayName { get; set; }

        public string EmailFrom { get; set; }

        public MailPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            SmtpHost = pluginLoadData.Settings.Get(nameof(SmtpHost));
            SmtpPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(SmtpPort)));
            SmtpUsername = pluginLoadData.Settings.Get(nameof(SmtpUsername));
            SmtpPassword = pluginLoadData.Settings.Get(nameof(SmtpPassword));
            SenderDisplayName = pluginLoadData.Settings.Get(nameof(SenderDisplayName));
            EmailFrom = pluginLoadData.Settings.Get(nameof(EmailFrom));
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);

            SetupSmtpClient();
        }

        protected virtual void SetupSmtpClient()
        {
            // Configure mail client
            SmtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = new NetworkCredential(SmtpUsername, SmtpPassword) as ICredentialsByHost,
                EnableSsl = true
            };

            // set the network credentials

            SmtpClient.SendCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    WriteEvent("EMail send error:" + args.Error, LogType.Fatal);
                }
            };

            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
        }

        public void SendMail(string to, string subject, string body)
        {
            // Create the mail message (from, to, subject, body)
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(EmailFrom, SenderDisplayName);
            mailMessage.To.Add(to);

            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.High;


            // send the mail
            SmtpClient.SendAsync(mailMessage, "");
        }
    }
}
