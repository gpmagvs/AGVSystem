using AGVSystem.Service;
using AGVSystem.ViewModel;
using AGVSystemCommonNet6.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderFromToController : ControllerBase
    {
        StationSelectService stationSelectService;
        public OrderFromToController(StationSelectService stationSelectService)
        {
            this.stationSelectService = stationSelectService;
        }

        [HttpGet("GetSourceStationOptions")]
        public async Task<List<StationSelectOption>> GetSourceStationOptions(ERole userRole)
        {
            return stationSelectService.GetSourceStationOptions(userRole);
        }

        [HttpGet("GetDestineStationOptions")]
        public async Task<List<StationSelectOption>> GetDestineStationOptions(ERole userRole)
        {
            return stationSelectService.GetDestineStationOptions(userRole);
        }
        [HttpGet("GetDestineStationOptionsWithDestineSelected")]
        public async Task<List<StationSelectOption>> GetDestineStationOptionsWithDestineSelected(ERole userRole, int sourceTag, int sourceSlot)
        {
            return stationSelectService.GetDestineStationOptions(userRole);
        }
    }
}
