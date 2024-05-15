using AGVSystem.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {

        public EventController()
        {
        }
        [HttpGet]
        public async Task GetEventsAsync()
        {
            //try
            //{
            //    HttpContext.Response.ContentType = "text/event-stream";
            //    var tcs = new TaskCompletionSource<object>();

            //    NotifyServiceHelper.OnMessage += OnMessageRecieved;
            //    HttpContext.RequestAborted.Register(() =>
            //    {
            //        NotifyServiceHelper.OnMessage -= OnMessageRecieved;
            //        tcs.TrySetCanceled();
            //    });
            //    await tcs.Task;
            //}
            //catch (TaskCanceledException ex)
            //{
            //    Console.WriteLine($"Notify register is disconnect.");
            //}
            //async void OnMessageRecieved(object sender, (string eventName, string message) e)
            //{
            //    try
            //    {
            //        await Response.WriteAsync($"event:{e.eventName}\n");
            //        await Response.WriteAsync($"data:{e.message}\n\n");
            //        await Response.Body.FlushAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}
        }
    }
}
