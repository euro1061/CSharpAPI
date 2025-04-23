using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace QBackend {
    public class DataContextDapper {
        private readonly IConfiguration _config;

        public DataContextDapper(IConfiguration config){
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.Query<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> LoadDataAsync<T>(string sql, object? parameters = null) {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public T LoadDataSingle<T>(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.QuerySingle<T>(sql, parameters);
        }

        public async Task<T> LoadDataSingleAsync<T>(string sql, object? parameters = null) {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return await connection.QuerySingleAsync<T>(sql, parameters);
        }

        public T? LoadDataSingleOrDefault<T>(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.QuerySingleOrDefault<T>(sql, parameters);
        }

        public async Task<T?> LoadDataSingleOrDefaultAsync<T>(string sql, object? parameters = null) {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public bool ExecuteCommand(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.Execute(sql, parameters) > 0;
        }

        public async Task<bool> ExecuteCommandAsync(string sql, object? parameters = null) {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return await connection.ExecuteAsync(sql, parameters) > 0;
        }

        public int ExecuteCommandWithRowCount(string sql) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.Execute(sql);
        }

        public async Task<int> ExecuteCommandWithRowCountAsync(string sql) {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return await connection.ExecuteAsync(sql);
        }
    }
}