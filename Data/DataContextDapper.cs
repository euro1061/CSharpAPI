using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace QBackend {
    class DataContextDapper {
        private readonly IConfiguration _config;

        public DataContextDapper(IConfiguration config){
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.Query<T>(sql, parameters);
        }

        public T LoadDataSingle<T>(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.QuerySingle<T>(sql, parameters);
        }

        public bool ExecuteCommand(string sql, object? parameters = null) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.Execute(sql, parameters) > 0;
        }

        public int ExecuteCommandWithRowCount(string sql) {
            IDbConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return connection.Execute(sql);
        }

        public bool ExecuteCommandWithParameter(string sql, List<SqlParameter> parameters) {
            SqlCommand command = new SqlCommand(sql);

            foreach (SqlParameter parameter in parameters) {
                command.Parameters.Add(parameter);
            }
            
            SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            connection.Open();

            command.Connection = connection;

            int rowsAffected = command.ExecuteNonQuery();

            connection.Close();

            return rowsAffected > 0;
        }
    }
}