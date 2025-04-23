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
    public class QueueController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;

        public QueueController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpPost("CreateQueue")]
        public async Task<IActionResult> CreateQueue(CreateQueueDto queue)
        {
            try
            {
                string sql = @"EXEC sp_CreateQueue @ServiceID, @QueueID OUTPUT, @QueueCode OUTPUT";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@ServiceID", queue.ServiceID, DbType.Int32);
                parameters.Add("@QueueID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@QueueCode", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);

                QueuePrintInfo? result = await _dapper.LoadDataSingleOrDefaultAsync<QueuePrintInfo?>(sql, parameters);

                return HttpResponseHelper.Success(result, "Queue created");
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to create queue: {ex.Message}", 500);
            }
        }
        [HttpPost("CallQueue")]
        public async Task<IActionResult> CallQueue(CallQueueDto queue)
        {
            try
            {
                string sql = @"EXEC sp_CallQueue @QueueID, @CounterID";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@QueueID", queue.QueueID, DbType.Int32);
                parameters.Add("@CounterID", queue.CounterID, DbType.Int32);
                
                if (await _dapper.ExecuteCommandAsync(sql, parameters))
                {
                    return HttpResponseHelper.Success(new { queueId = queue.QueueID, counterId = queue.CounterID }, "Queue called");
                }

                return HttpResponseHelper.Error("Failed to call queue", 500);
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to call queue: {ex.Message}", 500);
            }
        }

        [HttpPost("SkipQueue")]
        public async Task<IActionResult> SkipQueue(SkipQueueDto queue)
        {
            try
            {
                string sql = @"EXEC sp_SkipQueue @QueueID, @CounterID";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@QueueID", queue.QueueID, DbType.Int32);
                parameters.Add("@CounterID", queue.CounterID, DbType.Int32);
                
                if (await _dapper.ExecuteCommandAsync(sql, parameters))
                {
                    return HttpResponseHelper.Success(new { queueId = queue.QueueID, counterId = queue.CounterID }, "Queue skipped");
                }

                return HttpResponseHelper.Error("Failed to skip queue", 500);
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to skip queue: {ex.Message}", 500);
            }
        }

        [HttpPost("CompleteQueue")]
        public async Task<IActionResult> CompleteQueue(CompleteQueueDto queue)
        {
            try
            {
                string sql = @"EXEC sp_CompleteQueue @QueueID, @CounterID";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@QueueID", queue.QueueID, DbType.Int32);
                parameters.Add("@CounterID", queue.CounterID, DbType.Int32);
                
                if (await _dapper.ExecuteCommandAsync(sql, parameters))
                {
                    return HttpResponseHelper.Success(new { queueId = queue.QueueID, counterId = queue.CounterID }, "Queue completed");
                }

                return HttpResponseHelper.Error("Failed to complete queue", 500);
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to complete queue: {ex.Message}", 500);
            }
        }

        [HttpPost("InProgressQueue")]
        public async Task<IActionResult> InProgressQueue(InProgressQueueDto queue)
        {
            try
            {
                string sql = @"EXEC sp_InProgressQueue @QueueID, @CounterID";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@QueueID", queue.QueueID, DbType.Int32);
                parameters.Add("@CounterID", queue.CounterID, DbType.Int32);
                
                if (await _dapper.ExecuteCommandAsync(sql, parameters))
                {
                    return HttpResponseHelper.Success(new { queueId = queue.QueueID, counterId = queue.CounterID }, "Queue in progress");
                }

                return HttpResponseHelper.Error("Failed to set queue in progress", 500);
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to set queue in progress: {ex.Message}", 500);
            }
        }

        [HttpGet("GetQueues")]
        public async Task<IActionResult> GetQueues([FromQuery] GetQueueParams queueParams)
        {
            try
            {
                string sql = @"EXEC sp_GetQueue @ServiceID = @ServiceID, @Status = @Status, @QueueCode = @QueueCode, @StartDate = @StartDate, @EndDate = @EndDate";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@ServiceID", queueParams.ServiceID, DbType.Int32);
                parameters.Add("@Status", queueParams.Status, DbType.String);
                parameters.Add("@QueueCode", queueParams.QueueCode, DbType.String);
                parameters.Add("@StartDate", queueParams.StartDate, DbType.DateTime);
                parameters.Add("@EndDate", queueParams.EndDate, DbType.DateTime);

                IEnumerable<GetQueues> result = await _dapper.LoadDataAsync<GetQueues>(sql, parameters);

                return HttpResponseHelper.Success(result, "Queues retrieved");
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to get queues: {ex.Message}", 500);
            }
        }

        [HttpGet("ResetQueue")]
        public async Task<IActionResult> ResetQueue()
        {
            try
            {
                string sql = @"EXEC sp_ResetQueue";

                if (await _dapper.ExecuteCommandAsync(sql))
                {
                    return HttpResponseHelper.Success("Queue reset");
                }

                return HttpResponseHelper.Error("Failed to reset queue", 500);
            }
            catch (Exception ex)
            {
                return HttpResponseHelper.Error($"Failed to reset queue: {ex.Message}", 500);
            }
        }
    }
}