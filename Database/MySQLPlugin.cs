using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using MySql.Data.MySqlClient;

namespace Database
{
    public class MySQLPlugin : Plugin
    {
        private readonly bool _debug = true;
        private string _connectionString;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public MySQLPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);

            _connectionString = DatabaseManager.GetConnectionString("MySQL");
        }

        #region ErrorHandling

        //For calling from other plugins to help track down the issue to the database connection-- might not be needed
        public void DatabaseError(IClient client, ushort subject, byte code, Exception e)
        {
            if (_debug)
                WriteEvent("[MySqlConnector] Database Error: " + e.Message + " - " + e.StackTrace, LogType.Error);

            using (var writer = DarkRiftWriter.Create())
            {
                writer.Write(code);

                using (var message = Message.Create(subject, writer))
                {
                    client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        #endregion

        public SqlAccountData CreateAccountObject()
        {
            return new SqlAccountData();
        }

        public SqlAccountData GetAccountByToken(string token)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM accounts WHERE token = @token;";
                cmd.Parameters.AddWithValue("@token", token);

                var reader = cmd.ExecuteReader();

                // There's no such user
                if (!reader.HasRows)
                    return null;

                return ReadAccountData(reader, cmd);
            }
        }

        public SqlAccountData GetAccount(string email)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM accounts WHERE email = @email;";
                cmd.Parameters.AddWithValue("@email", email);

                var reader = cmd.ExecuteReader();

                // There's no such user
                if (!reader.HasRows)
                    return null;

                return ReadAccountData(reader, cmd);
            }
        }

        public void SavePasswordResetCode(SqlAccountData account, string code)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "INSERT INTO password_reset_codes (email, code) " +
                                  "VALUES(@email, @code) " +
                                  "ON DUPLICATE KEY UPDATE code = @code";

                cmd.Parameters.AddWithValue("@email", account.Email);
                cmd.Parameters.AddWithValue("@code", code);
                cmd.ExecuteNonQuery();
            }
        }

        public PasswordResetData GetPasswordResetData(string email)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM password_reset_codes " +
                                  "WHERE email = @email";
                cmd.Parameters.AddWithValue("@email", email);

                var reader = cmd.ExecuteReader();

                // There's no such user
                if (!reader.HasRows)
                    return null;

                // Read row
                reader.Read();

                return new PasswordResetData
                {
                    Code = reader["code"] as string,
                    Email = reader["email"] as string
                };
            }
        }

        public void SaveEmailConfirmationCode(string email, string code)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "INSERT INTO email_confirmation_codes (email, code) " +
                                  "VALUES(@email, @code) " +
                                  "ON DUPLICATE KEY UPDATE code = @code";

                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@code", code);
                cmd.ExecuteNonQuery();
            }

            WriteEvent("Should have inserted: " + email + " " + code, LogType.Info);
        }

        public string GetEmailConfirmationCode(string email)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM email_confirmation_codes " +
                                  "WHERE email = @email";
                cmd.Parameters.AddWithValue("@email", email);

                var reader = cmd.ExecuteReader();

                // There's no such user
                if (!reader.HasRows)
                    return null;

                // Read row
                reader.Read();

                return reader["code"] as string;
            }
        }

        public void UpdateAccount(SqlAccountData account)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "UPDATE accounts " +
                                  "SET password = @password, " +
                                  "email = @email, " +
                                  "is_admin = @is_admin, " +
                                  "is_guest = @is_guest, " +
                                  "is_email_confirmed = @is_email_confirmed " +
                                  "WHERE account_id = @account_id";
                cmd.Parameters.AddWithValue("@password", account.Password);
                cmd.Parameters.AddWithValue("@email", account.Email);
                cmd.Parameters.AddWithValue("@is_admin", account.IsAdmin ? 1 : 0);
                cmd.Parameters.AddWithValue("@is_guest", account.IsGuest ? 1 : 0);
                cmd.Parameters.AddWithValue("@is_email_confirmed", account.IsEmailConfirmed ? 1 : 0);
                cmd.Parameters.AddWithValue("@account_id", account.AccountId);

                cmd.ExecuteNonQuery();
            }
        }

        public void InsertNewAccount(SqlAccountData account)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "INSERT INTO accounts " +
                                  "SET email = @email, " +
                                  "password = @password, " +
                                  "is_admin = @is_admin, " +
                                  "is_guest = @is_guest, " +
                                  "is_email_confirmed = @is_email_confirmed ";

                cmd.Parameters.AddWithValue("@password", account.Password);
                cmd.Parameters.AddWithValue("@email", account.Email);
                cmd.Parameters.AddWithValue("@is_admin", account.IsAdmin ? 1 : 0);
                cmd.Parameters.AddWithValue("@is_guest", account.IsGuest ? 1 : 0);
                cmd.Parameters.AddWithValue("@is_email_confirmed", account.IsEmailConfirmed ? 1 : 0);
                cmd.ExecuteNonQuery();
                account.AccountId = (int) cmd.LastInsertedId;
            }
        }

        public void InsertToken(SqlAccountData account, string token)
        {
            using (var con = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand())
            {
                con.Open();

                cmd.Connection = con;
                cmd.CommandText = "UPDATE accounts " +
                                  "SET token = @token " +
                                  "WHERE account_id = @account_id";
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@account_id", account.AccountId);

                account.Token = token;

                cmd.ExecuteNonQuery();
            }
        }

        private SqlAccountData ReadAccountData(MySqlDataReader reader, MySqlCommand cmd)
        {
            SqlAccountData account = null;

            // Read primary account data
            while (reader.Read())
                account = new SqlAccountData
                {
                    AccountId = reader.GetInt32("account_id"),
                    Email = reader["email"] as string,
                    Password = reader["password"] as string,
                    IsAdmin = reader["is_admin"] as bool? ?? false,
                    IsGuest = reader["is_guest"] as bool? ?? false,
                    IsEmailConfirmed = reader["is_email_confirmed"] as bool? ?? false,
                    Properties = new Dictionary<string, string>(),
                    Token = reader["token"] as string
                };

            if (account == null)
                return null;

            // Read account values
            reader.Close();

            cmd.CommandText = "SELECT * FROM account_properties WHERE account_id = @account_id";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@account_id", account.AccountId);
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var key = reader["prop_key"] as string ?? "";
                var value = reader["prop_val"] as string ?? "";

                if (string.IsNullOrEmpty(key))
                    continue;

                account.Properties[key] = value;
            }

            return account;
        }
    }
}