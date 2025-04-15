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
    public class ServicesController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;

        public ServicesController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpGet("GetServices")]
        public IActionResult GetServices()
        {
            string sql = @"SELECT 
                [ServiceID],
                [ServiceName],
                [ServicePrefix],
                [Description],
                [CreatedAt]
            FROM dbo.Services ORDER BY ServiceName";
            IEnumerable<ServiceDto> services = _dapper.LoadData<ServiceDto>(sql);
            return HttpResponseHelper.Success(services, "Services found");
        }

        [HttpGet("GetService/{id}")]
        public IActionResult GetService(int id)
        {
            string sql = @"SELECT 
                [ServiceID],
                [ServiceName],
                [ServicePrefix],
                [Description],
                [CreatedAt]
            FROM dbo.Services WHERE ServiceID = @ServiceId";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@ServiceId", id, DbType.Int32);

            ServiceDto service = _dapper.LoadDataSingle<ServiceDto>(sql, parameters);
            return HttpResponseHelper.Success(service, "Service found");
        }

        [HttpPost("CreateService")]
        public IActionResult CreateService(CreateServiceDto service) {
            string sql = "INSERT INTO dbo.Services (ServiceName, ServicePrefix, Description) VALUES (@ServiceName, @ServicePrefix, @Description)";
            
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@ServiceName", service.ServiceName, DbType.String);
            parameters.Add("@ServicePrefix", service.ServicePrefix, DbType.String);
            parameters.Add("@Description", service.Description, DbType.String);

            if(_dapper.ExecuteCommand(sql, parameters)){
                return HttpResponseHelper.Success(service, "Service created");
            }else {
                return HttpResponseHelper.Error("Failed to create service", 500);
            }
        }

        [HttpPut("UpdateService/{id}")]
        public IActionResult UpdateService(int id, UpdateServiceDto service) {
            string sql = "UPDATE dbo.Services SET ServiceName = @ServiceName, ServicePrefix = @ServicePrefix, Description = @Description WHERE ServiceID = @ServiceId";
            
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@ServiceName", service.ServiceName, DbType.String);
            parameters.Add("@ServicePrefix", service.ServicePrefix, DbType.String);
            parameters.Add("@Description", service.Description, DbType.String);
            parameters.Add("@ServiceId", id, DbType.Int32);

            if(_dapper.ExecuteCommand(sql, parameters)){
                return HttpResponseHelper.Success(service, "Service updated");
            }else {
                return HttpResponseHelper.Error("Failed to update service", 500);
            }
        }

        [HttpDelete("DeleteService/{id}")]
        public IActionResult DeleteService(int id) {
            string sql = "DELETE FROM dbo.Services WHERE ServiceID = @ServiceId";
            
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@ServiceId", id, DbType.Int32);

            if(_dapper.ExecuteCommand(sql, parameters)){
                return HttpResponseHelper.Success(new {}, "Service deleted");
            }else {
                return HttpResponseHelper.Error("Failed to delete service", 500);
            }
        }
    }
}