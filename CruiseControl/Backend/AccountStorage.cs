// AccountStorage.cs (C#)

using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CruiseControl
{
    public class AccountStorage
    {
        private string filePath;
        private string loggedInUsernameFilePath;

        public AccountStorage(string filePath, string loggedInUsernameFilePath)
        {
            this.filePath = filePath;
            this.loggedInUsernameFilePath = loggedInUsernameFilePath;
        }

        public string GetCurrentLoggedInUsername()
        {
            if (File.Exists(loggedInUsernameFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(loggedInUsernameFilePath);
                    var loggedInUser = JsonSerializer.Deserialize<LoggedInUser>(jsonString);
                    return loggedInUser?.Username;
                }
                catch (JsonException)
                {
                    return null;
                }
            }
            return null;
        }

        public void SetLoggedInUsername(string username)
        {
            var loggedInUser = new LoggedInUser { Username = username };
            string jsonString = JsonSerializer.Serialize(loggedInUser);
            File.WriteAllText(loggedInUsernameFilePath, jsonString);
        }

        public void RemoveLoggedInUsername()
        {
            if (File.Exists(loggedInUsernameFilePath))
            {
                File.Delete(loggedInUsernameFilePath);
            }
        }

        public class LoggedInUser
        {
            public string Username { get; set; }
        }

        public void StoreAccount(string username, string password)
        {
            List<Account> accounts = LoadAccounts();

            string hashedPassword = HashPassword(password); // Hash the password
            accounts.Add(new Account { Username = username, Password = hashedPassword });

            string jsonString = JsonSerializer.Serialize(accounts);
            File.WriteAllText(filePath, jsonString);
        }

        public List<Account> LoadAccounts()
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                try
                {
                    List<Account> accounts = JsonSerializer.Deserialize<List<Account>>(jsonString) ?? new List<Account>();
                    return accounts;
                }
                catch (JsonException)
                {
                    return new List<Account>(); // Return empty list if json is invalid.
                }

            }

            return new List<Account>();
        }

        public class Account
        {
            public string Username { get; set; }
            public string Password { get; set; } // Store hashed password in production
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string enteredHash = HashPassword(enteredPassword);
            return string.Equals(enteredHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}