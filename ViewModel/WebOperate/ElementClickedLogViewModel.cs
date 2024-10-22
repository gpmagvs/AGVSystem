using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AGVSystem.ViewModel
{
    public class ElementClickedLogViewModel
    {
        public string Name { get; set; } = "";
        public string Source { get; set; } = "";
    }
}
