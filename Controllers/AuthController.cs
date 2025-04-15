using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using QBackend.Dtos;
using QBackend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Dapper;
using QBackend.Models;

namespace QBackend.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Signup")]
        public IActionResult Signup(SignupDto user)
        {
            string sqlCheckExistingUser = "SELECT Username FROM [Users] WHERE [Username] = @Username";

            string password = _authHelper.CreateDefaultPassword(user.FirstName, user.LastName);

            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckExistingUser, user);

            if (existingUsers.Count() == 0)
            {
                byte[] passwordSalt = new byte[128 / 8];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }

                byte[] passwordHash = _authHelper.GetPasswordHash(password, passwordSalt);

                string sqlInsertAuth = "EXEC sp_UserSignup @Username, @PasswordHash, @PasswordSalt, @FirstName, @LastName";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@PasswordSalt", passwordSalt, DbType.Binary);
                parameters.Add("@PasswordHash", passwordHash, DbType.Binary);
                parameters.Add("@Username", user.Username, DbType.String);
                parameters.Add("@FirstName", user.FirstName, DbType.String);
                parameters.Add("@LastName", user.LastName, DbType.String);

                if (_dapper.ExecuteCommand(sqlInsertAuth, parameters))
                {
                    return HttpResponseHelper.Success(new { username = user.Username }, "User created successfully");
                }

                return HttpResponseHelper.Error("Error creating user", 500);
            }

            return HttpResponseHelper.Error("Username is already registered", 400);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(SigninDto user)
        {
            string sqlForHasAndSalt = "SELECT [PasswordHash], [PasswordSalt] FROM [Users] WHERE [Username] = '" + user.Username + "'";

            UserConfirmation userLoginConfirmation = _dapper.LoadDataSingle<UserConfirmation>(sqlForHasAndSalt);

            if (userLoginConfirmation == null)
            {
                return HttpResponseHelper.Error("User not found", 404);
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(user.Password, userLoginConfirmation.PasswordSalt);

            if (!passwordHash.SequenceEqual(userLoginConfirmation.PasswordHash))
            {
                return HttpResponseHelper.Error("Invalid Password", 401);
            }

            int userId = _dapper.LoadDataSingle<int>("SELECT [UserId] FROM [Users] WHERE [Username] = '" + user.Username + "'");

            return HttpResponseHelper.Success(new { token = _authHelper.CreateToken(userId) }, "User logged in successfully");
        }

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            string sql = "SELECT [UserId], [Username], [FirstName], [LastName], [Role], [AssignedCounterID] FROM [Users]";

            IEnumerable<User> users = _dapper.LoadData<User>(sql);

            return HttpResponseHelper.Success(users, "Users found");
        }

        [HttpPut("ChangePassword")]
        public IActionResult ChangePassword(ChangePasswordDto user)
        {
            var userId = User.FindFirst("userId")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            if (user.NewPassword != user.ConfirmPassword)
            {
                return HttpResponseHelper.Error("New password does not match confirm password", 400);
            }

            var userInformation = _authHelper.GetUserPasswordInfo(userId);
            if (userInformation == null)
            {
                return HttpResponseHelper.Error("User not found", 404);
            }

            if (!_authHelper.ValidateOldPassword(user.OldPassword, userInformation))
            {
                return HttpResponseHelper.Error("Old password is incorrect", 400);
            }

            if (_authHelper.IsNewPasswordSameAsOld(user.NewPassword, userInformation))
            {
                return HttpResponseHelper.Error("New password cannot be the same as the old password", 400);
            }

            var (newHash, newSalt) = _authHelper.GenerateNewPasswordHashAndSalt(user.NewPassword);
            
            if (_authHelper.UpdateUserPassword(userId, newHash, newSalt))
            {
                return HttpResponseHelper.Success(new { }, "Password changed successfully");
            }

            return HttpResponseHelper.Error("Failed to change password", 400);
        }

        [HttpGet("GetMeInformation")]
        public IActionResult GetMeInformation()
        {
            string userId = User.FindFirst("userId")?.Value + "";
            string sqlGetInfo = @"SELECT [UserId] ,[Username], [FirstName], [LastName], [Role], [AssignedCounterID] FROM [Users] WHERE [UserId] = @UserId";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@UserId", int.Parse(userId), DbType.Int32);

            User user = _dapper.LoadDataSingle<User>(sqlGetInfo, parameters);

            if (user == null)
            {
                return HttpResponseHelper.Error("User not found", 404);
            }

            return HttpResponseHelper.Success(user, "User found");
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";

            return Ok(new Dictionary<string, string>(){
                {"token", _authHelper.CreateToken(int.Parse(userId))}
            });
        }
    }
}