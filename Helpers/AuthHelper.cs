using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using QBackend.Models;
using Dapper;
using System.Data;
using QBackend.Dtos;

namespace QBackend.Helpers
{
    public class AuthHelper
    {

        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(config);
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.UTF8.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

        public string CreateToken(int userId, string username, string firstName, string lastName, string role)
        {
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString()),
                new Claim("username", username),
                new Claim("firstName", firstName),
                new Claim("lastName", lastName),
                new Claim("role", role)
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString ?? ""));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        public string CreateDefaultPassword(string FirstName, string LastName)
        {
            string password = FirstName + "." + LastName.Substring(0, 3) + "1234";
            return password;
        }

        public UserConfirmation GetUserPasswordInfo(string userId)
        {
            const string sql = "SELECT [PasswordHash], [PasswordSalt] FROM [Users] WHERE [UserId] = @UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", int.Parse(userId), DbType.Int32);
            return _dapper.LoadDataSingle<UserConfirmation>(sql, parameters);
        }

        public bool ValidateOldPassword(string oldPassword, UserConfirmation userInfo)
        {
            var oldPasswordHash = GetPasswordHash(oldPassword, userInfo.PasswordSalt);
            return oldPasswordHash.SequenceEqual(userInfo.PasswordHash);
        }

        public bool IsNewPasswordSameAsOld(string newPassword, UserConfirmation userInfo)
        {
            var newPasswordHash = GetPasswordHash(newPassword, userInfo.PasswordSalt);
            return newPasswordHash.SequenceEqual(userInfo.PasswordHash);
        }

        public (byte[] Hash, byte[] Salt) GenerateNewPasswordHashAndSalt(string newPassword)
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(salt);
            }
            var hash = GetPasswordHash(newPassword, salt);
            return (hash, salt);
        }

        public async Task<bool> UpdateUserPasswordAsync(string userId, byte[] passwordHash, byte[] passwordSalt)
        {
            const string sql = "UPDATE [Users] SET [PasswordHash] = @PasswordHash, [PasswordSalt] = @PasswordSalt WHERE [UserId] = @UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@PasswordHash", passwordHash, DbType.Binary);
            parameters.Add("@PasswordSalt", passwordSalt, DbType.Binary);
            parameters.Add("@UserId", int.Parse(userId), DbType.Int32);
            return await _dapper.ExecuteCommandAsync(sql, parameters);
        }
    }
}