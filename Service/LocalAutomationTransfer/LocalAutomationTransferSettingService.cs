using AGVSystem.Models.Automation;
using Microsoft.Extensions.Options;

namespace AGVSystem.Service.LocalAutomationTransfer
{
    public class LocalAutomationTransferSettingService
    {
        public readonly AutoTransferSettings.AutoTransferSetting settings;
        public LocalAutomationTransferSettingService(IOptions<AutoTransferSettings.AutoTransferSetting> options)
        {
            settings = options.Value;
        }
    }
}
