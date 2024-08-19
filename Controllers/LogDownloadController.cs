using AGVSystem.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogDownloadController : ControllerBase
    {
        LogDownlodService logDownlodService;
        public LogDownloadController(LogDownlodService logDownlodService)
        {
            this.logDownlodService = logDownlodService;
        }

        [HttpGet("Query")]
        public async Task<IActionResult> Get(DateTime from, DateTime to)
        {
            try
            {
                string localZipFilePath = await logDownlodService.CreateLogCompressedFile(from, to);
                var stream = new FileStream(localZipFilePath, FileMode.Open, FileAccess.Read);

                string fileName = Path.GetFileName(localZipFilePath);

                return File(stream, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
