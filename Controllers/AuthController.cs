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
        public async Task<IActionResult> Signup(SignupDto user)
        {
            string sqlCheckExistingUser = "EXEC sp_CheckUserExisting @Username";

            string password = _authHelper.CreateDefaultPassword(user.FirstName, user.LastName);

            DynamicParameters checkUserExistingParameters = new DynamicParameters();
            checkUserExistingParameters.Add("@Username", user.Username, DbType.String);

            int existingUsers = await _dapper.LoadDataSingleAsync<int>(sqlCheckExistingUser, checkUserExistingParameters);

            if (existingUsers == 0)
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

                if (await _dapper.ExecuteCommandAsync(sqlInsertAuth, parameters))
                {
                    return HttpResponseHelper.Success(new { username = user.Username }, "User created successfully");
                }

                return HttpResponseHelper.Error("Error creating user", 500);
            }

            return HttpResponseHelper.Error("Username is already registered", 400);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto user)
        {
            if(!ModelState.IsValid) {
                Console.WriteLine(ModelState);
            }

            string sqlForHasAndSalt = "EXEC sp_GetPasswordUserByUsername @Username";
            
            DynamicParameters getPasswordParameters = new DynamicParameters();
            getPasswordParameters.Add("@Username", user.Username, DbType.String);
            
            UserConfirmation? userLoginConfirmation = await _dapper.LoadDataSingleOrDefaultAsync<UserConfirmation?>(sqlForHasAndSalt, getPasswordParameters);
            
            if (userLoginConfirmation == null)
            {
                return HttpResponseHelper.Error("User not found", 404);
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(user.Password, userLoginConfirmation.PasswordSalt);

            if (!passwordHash.SequenceEqual(userLoginConfirmation.PasswordHash))
            {
                return HttpResponseHelper.Error("Invalid Password", 401);
            }

            string sqlGetUserProfile = @"EXEC sp_GetUserProfile @Username";

            DynamicParameters getUserProfileParameters = new DynamicParameters();
            getUserProfileParameters.Add("@Username", user.Username, DbType.String);

            UserProfile? userProfile = await _dapper.LoadDataSingleOrDefaultAsync<UserProfile>(sqlGetUserProfile, getUserProfileParameters);

            if (userProfile == null)
            {
                return HttpResponseHelper.Error("User not found", 404);
            }

            return HttpResponseHelper.Success(new { token = _authHelper.CreateToken(userProfile.UserId, userProfile.Username, userProfile.FirstName, userProfile.LastName, userProfile.Role) }, "User logged in successfully");
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            string sql = "SELECT [UserId], [Username], [FirstName], [LastName], [Role], [AssignedCounterID] FROM [Users]";

            IEnumerable<User> users = await _dapper.LoadDataAsync<User>(sql);

            return HttpResponseHelper.Success(users, "Users found");
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto user)
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
            
            if (await _authHelper.UpdateUserPasswordAsync(userId, newHash, newSalt))
            {
                return HttpResponseHelper.Success(new { }, "Password changed successfully");
            }

            return HttpResponseHelper.Error("Failed to change password", 400);
        }

        [HttpGet("GetMeInfomation")]
        public async Task<IActionResult> GetMeInformation()
        {
            string userId = User.FindFirst("userId")?.Value + "";
            string sqlGetInfo = @"EXEC sp_GetUserProfile '', @UserID";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@UserID", int.Parse(userId), DbType.Int32);

            UserProfile? user = await _dapper.LoadDataSingleOrDefaultAsync<UserProfile>(sqlGetInfo, parameters);

            if (user == null)
            {
                return HttpResponseHelper.Error("User not found", 404);
            }

            return HttpResponseHelper.Success(user, "User found");
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";
            string username = User.FindFirst("username")?.Value ?? "";
            string firstName = User.FindFirst("firstName")?.Value ?? "";
            string lastName = User.FindFirst("lastName")?.Value ?? "";
            string role = User.FindFirst("role")?.Value ?? "";

            var result = new Dictionary<string, string>(){
                {"token", _authHelper.CreateToken(int.Parse(userId), username, firstName, lastName, role)}
            };

            return await Task.FromResult(Ok(result));
        }
    }
}