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

        public (bool success, string message) StoreAccount(string username, string password, string firstName, string lastName)
        {
            List<Account> accounts = LoadAccounts();

            if (accounts.Any(a => a.Username == username))
            {
                return (false, "Username already exists.");
            }

            accounts.Add(new Account { Username = username, Password = password, PhotoPath = null, FirstName = firstName, LastName = lastName});

            string jsonString = JsonSerializer.Serialize(accounts);
            File.WriteAllText(filePath, jsonString);

            return (true, "Account created successfully.");
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
            public string PhotoPath { get; set; }
            public int Points { get; set; }
            public List<string> Followers { get; set; }
            public List<string> Following { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

        }

        public void UpdatePhotoPath(string username, string photoPath)
        {
            List<Account> accounts = LoadAccounts();
            Account account = accounts.FirstOrDefault(a => a.Username == username);

            if (account != null)
            {
                account.PhotoPath = photoPath;
                string jsonString = JsonSerializer.Serialize(accounts);
                File.WriteAllText(filePath, jsonString);
            }
        }
        public void FollowUser(string followerUsername, string followingUsername)
        {
            List<Account> accounts = LoadAccounts();
            Account follower = accounts.FirstOrDefault(a => a.Username == followerUsername);
            Account following = accounts.FirstOrDefault(a => a.Username == followingUsername);

            if (follower != null && following != null)
            {
                if (follower.Following == null)
                {
                    follower.Following = new List<string>();
                }
                if (following.Followers == null)
                {
                    following.Followers = new List<string>();
                }

                if (!follower.Following.Contains(followingUsername))
                {
                    follower.Following.Add(followingUsername);
                    following.Followers.Add(followerUsername);
                    string jsonString = JsonSerializer.Serialize(accounts);
                    File.WriteAllText(filePath, jsonString);
                }
            }
        }

        public void UnfollowUser(string followerUsername, string followingUsername)
        {
            List<Account> accounts = LoadAccounts();
            Account follower = accounts.FirstOrDefault(a => a.Username == followerUsername);
            Account following = accounts.FirstOrDefault(a => a.Username == followingUsername);

            if (follower != null && following != null)
            {
                if (follower.Following != null && follower.Following.Contains(followingUsername))
                {
                    follower.Following.Remove(followingUsername);
                    following.Followers.Remove(followerUsername);
                    string jsonString = JsonSerializer.Serialize(accounts);
                    File.WriteAllText(filePath, jsonString);
                }
            }
        }

        public Account GetAccount(string username)
        {
            List<Account> accounts = LoadAccounts();
            return accounts.FirstOrDefault(a => a.Username == username);
        }

        public int GetPoints(string username)
        {
            Account account = GetAccount(username);
            return account.Points;
        }

        public void SetPoints(string username, int points)
        {
            List<Account> accounts = LoadAccounts();
            Account account = accounts.FirstOrDefault(a => a.Username == username);

            if (account != null)
            {
                account.Points = points;
                string jsonString = JsonSerializer.Serialize(accounts);
                File.WriteAllText(filePath, jsonString);
            }
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