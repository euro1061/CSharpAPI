using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using QBackend.Models;
using QBackend.Dtos;
using QBackend.Helpers;

namespace QBackend.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CounterController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;

        public CounterController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpPost("CreateCounter")]
        public async Task<IActionResult> CreateCounter(CreateCounterDto counter)
        {
            try
            {
                string sql = @"EXEC sp_CreateCounter @CounterName, @IsActive";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CounterName", counter.CounterName, DbType.String);
                parameters.Add("@IsActive", counter.IsActive, DbType.Boolean);

                Counter? result = await _dapper.LoadDataSingleOrDefaultAsync<Counter?>(sql, parameters);

                if (result == null)
                {
                    return HttpResponseHelper.Error("Failed to create counter", 500);
                }

                return HttpResponseHelper.Success(result, "Counter created");
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to create counter: {ex.Message}", 500);
            }
        }

        [HttpPut("UpdateCounter")]
        public async Task<IActionResult> UpdateCounter(UpdateCounterDto counter)
        {
            try
            {
                string sql = @"EXEC sp_UpdateCounter @CounterID, @CounterName, @IsActive";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CounterID", counter.CounterID, DbType.Int32);
                parameters.Add("@CounterName", counter.CounterName, DbType.String);
                parameters.Add("@IsActive", counter.IsActive, DbType.Boolean);

                Counter? result = await _dapper.LoadDataSingleOrDefaultAsync<Counter?>(sql, parameters);

                if (result == null)
                {
                    return HttpResponseHelper.Error("Counter not found", 404);
                }

                return HttpResponseHelper.Success(result, "Counter updated");
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to update counter: {ex.Message}", 500);
            }
        }

        [HttpDelete("DeleteCounter/{id}")]
        public async Task<IActionResult> DeleteCounter(int id)
        {
            try
            {
                string sql = @"EXEC sp_DeleteCounter @CounterID";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CounterID", id, DbType.Int32);

                if (await _dapper.ExecuteCommandAsync(sql, parameters))
                {
                    return HttpResponseHelper.Success(new { id }, "Counter deleted");
                }

                return HttpResponseHelper.Error("Failed to delete counter", 500);
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to delete counter: {ex.Message}", 500);
            }
        }

        [HttpGet("GetAllCounters")]
        public async Task<IActionResult> GetAllCounters()
        {
            try
            {
                string sql = @"EXEC sp_GetAllCounters";

                IEnumerable<Counter> result = await _dapper.LoadDataAsync<Counter>(sql);

                return HttpResponseHelper.Success(result, "Counters retrieved");
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to retrieve counters: {ex.Message}", 500);
            }
        }

        [HttpGet("GetCounterById/{id}")]
        public async Task<IActionResult> GetCounterById(int id)
        {
            try
            {
                string sql = @"EXEC sp_GetCounterById @CounterID";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@CounterID", id, DbType.Int32);

                Counter? result = await _dapper.LoadDataSingleOrDefaultAsync<Counter?>(sql, parameters);

                if (result == null)
                {
                    return HttpResponseHelper.Error("Counter not found", 404);
                }

                return HttpResponseHelper.Success(result, "Counter retrieved");
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to retrieve counter: {ex.Message}", 500);
            }
        }
    }
}