namespace AGVSystem.Service
{
    public class NotifyServiceHelper
    {
        public static event EventHandler<(string eventName, string message)> OnMessage;


        public static async Task ReloadMapNotify()
        {
            await NotifyAsync("Reload Map", "Reload");
        }

        public static async Task EquipmentMaintainingNotify(string eqName)
        {
            await NotifyAsync("Equipment_Maintain", eqName);
        }
        public static async Task NotifyAsync(string eventName, string message)
        {
            Task.Run(() =>
            {
                OnMessage?.Invoke("", (eventName, message));
            });
        }
    }
}
