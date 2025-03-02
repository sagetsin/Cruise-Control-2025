// AccountStorage.cs (C#)

using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CruiseControl
{
    public class AccountStorage
    {
        private string filePath;

        public AccountStorage(string filePath)
        {
            this.filePath = filePath;
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