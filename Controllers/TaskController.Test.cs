using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.Helpers;

using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    public partial class TaskController
    {
        TaskDatabaseHelper db = new TaskDatabaseHelper();
        [HttpGet("test/addtask")]
        public async Task<IActionResult> Add(string taskname)
        {

            db.Add(new clsTaskDto()
            {
                TaskName = taskname
            });
            return Ok();
        }

        [HttpGet("test/modify")]
        public async Task<IActionResult> modify(string taskname)
        {
            clsTaskDto taskDto = new clsTaskDto()
            {
                TaskName = taskname,
                FinishTime = DateTime.Now
            };
            db.Update(taskDto);
            return Ok();
        }
    }
}
