// // AccountsController.cs (API Controller)
// using Microsoft.AspNetCore.MVC;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Text;

// namespace CruiseControl.Controllers
// {
//     [ApiController]
//     [Route("api/accounts")]
//     public class AccountsController : ControllerBase
//     {
//         private static List<Account> accounts = new List<Account>(); // Replace with a database

//         [HttpPost("create")]
//         public IActionResult CreateAccount([FromBody] Account account)
//         {
//             if (accounts.Any(a => a.Username == account.Username))
//             {
//                 return BadRequest("Username already exists.");
//             }

//             account.Password = HashPassword(account.Password); // Hash the password
//             accounts.Add(account);

//             return Ok();
//         }

//         [HttpPost("signin")]
//         public IActionResult SignIn([FromBody] Account credentials)
//         {
//             var account = accounts.FirstOrDefault(a => a.Username == credentials.Username);

//             if (account != null && VerifyPassword(credentials.Password, account.Password))
//             {
//                 return Ok(new UserInfo { Name = account.Name, Username = account.Username });
//             }

//             return Unauthorized();
//         }

//         private string HashPassword(string password)
//         {
//             using (var sha256 = SHA256.Create())
//             {
//                 var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
//                 return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
//             }
//         }

//         private bool VerifyPassword(string enteredPassword, string storedHash)
//         {
//             string enteredHash = HashPassword(enteredPassword);
//             return string.Equals(enteredHash, storedHash, StringComparison.OrdinalIgnoreCase);
//         }

        
//     }
// }