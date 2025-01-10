using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.DATABASE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AGVSystemCommonNet6.DATABASE.DBDataService;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        DBDataService _dbDataService;
        public DatabaseController(DBDataService dbDataService)
        {
            _dbDataService = dbDataService;
        }

        [HttpGet]
        public async Task<OperationResult> Get()
        {
            return new OperationResult(true, "Success");
        }

        [HttpPost("AddTaskDto")]
        public async Task<OperationResult> AddTaskDto([FromBody] clsTaskDto taskDto)
        {
            return await _dbDataService.AddEntityToTableAsync(taskDto);
        }

        [HttpPost("ModifyTaskDto")]
        public async Task<OperationResult> ModifyTaskDto([FromBody] clsTaskDto taskDto)
        {
            return await _dbDataService.ModifyEntityInTableAsync(taskDto);
        }

        [HttpPost("DeleteTaskDto")]
        public async Task<OperationResult> DeleteTaskDto([FromBody] clsTaskDto taskDto)
        {
            return await _dbDataService.DeleteEntityFromTableAsync(taskDto);
        }
    }
}
