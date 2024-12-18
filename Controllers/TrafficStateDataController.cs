using AGVSystem.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AGVSystem.Service.TrafficStateDataQueryService;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrafficStateDataController : ControllerBase
    {
        TrafficStateDataQueryService trafficStateDataQueryService;
        public TrafficStateDataController(TrafficStateDataQueryService trafficStateDataQueryService)
        {
            this.trafficStateDataQueryService = trafficStateDataQueryService;
        }

        [HttpGet("PointStayStateData")]
        public async Task<Dictionary<int, clsStayData>> Data(DateTime StartTime, DateTime EndTime)
        {
            return await trafficStateDataQueryService.GetPointStayStateData(StartTime, EndTime);
        }
    }
}
