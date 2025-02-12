using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Microservices.AudioPlay;
using AGVSystemCommonNet6.Microservices.MCS;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using static AGVSystemCommonNet6.Microservices.MCS.MCSCIMService.ZoneData;

namespace AGVSystem.Models.EQDevices
{
    public class ZoneCapacityStatusMonitor : IDisposable
    {
        private MCSCIMService.ZoneData zoneData;
        private bool disposedValue;
        private IHubContext<FrontEndDataHub> hubContext;

        clsZoneUsableCarrierOptions options;
        public static Dictionary<string, clsZoneUsableCarrierOptions> lowLevelMonitorOptionsOfZones = new Dictionary<string, clsZoneUsableCarrierOptions>();
        public static ConcurrentDictionary<string, DateTime> NotifyingZoneNames = new();
        static string audioFilePath => Path.Combine(Environment.CurrentDirectory, $"Audios/shelf_available_materials_insufficient.mp3");

        private static string ThresholdStoreFileName =>
            Path.Combine(AGVSConfigulator.SysConfigs.PATHES_STORE[SystemConfigs.PATH_ENUMS.EQ_CONFIGS_FOLDER_PATH], "ZoneCarriersNotEnoughMonitor.json");

        public ZoneCapacityStatusMonitor(MCSCIMService.ZoneData zoneData)
        {
            this.zoneData = zoneData;
            options = GetOptionOfZone(zoneData.ZoneName);
        }

        public ZoneCapacityStatusMonitor(MCSCIMService.ZoneData zoneData, IHubContext<FrontEndDataHub> hubContext) : this(zoneData)
        {
            this.hubContext = hubContext;
        }

        internal async void TryNotifyCapacityCarrierNotEnough()
        {
            int availabeNumber = GetNumberOfUsableCarriers();
            if (availabeNumber >= this.options.ThresHoldValue)
            {
                NotifyingZoneNames.TryRemove(zoneData.ZoneName, out _);

                if (!NotifyingZoneNames.Any())
                    StopAudio();

                return;
            }
            NotifyingZoneNames.TryAdd(this.zoneData.ZoneName, DateTime.Now);
            string notifyMessage = $"{this.options.DisplayZoneName}({zoneData.ZoneName}) {GenerateNotiftMessage(this.options.NotifyMessageTemplate, availabeNumber)}";
            if (this.hubContext != null)
                this.hubContext.Clients.All.SendAsync("ZoneUsableCarrierNotEnoughNotify", notifyMessage);
            PlayAudio();
        }

        private string GenerateNotiftMessage(string notifyMessageTemplate, int availabeNumber)
        {
            if (string.IsNullOrEmpty(notifyMessageTemplate))
                return $"剩餘可用的 Carriers 已快用盡，請補料! 當前可用數量 = [{availabeNumber}]"; // default message
            else
            {
                //{RackName}{ZoneID} 可用的Tray盤數即將不足，請補空Tray. 當前數量:{AvailableNumber}
                string messageReplaced = notifyMessageTemplate.Replace("{RackName}", this.options.DisplayZoneName)
                                            .Replace("{ZoneID}", zoneData.ZoneName)
                                            .Replace("{AvailableNumber}", availabeNumber.ToString());

                //檢查是否有未取代的變數，若有則把變數取代成空字串
                messageReplaced = Regex.Replace(messageReplaced, @"\{.*?\}", "");
                //檢查若有()則移除
                messageReplaced = messageReplaced.Replace("()", "");
                return messageReplaced;
            }
        }

        private static async Task StopAudio()
        {
            await AudioPlayService.StopSpecficAudio(audioFilePath);
        }
        private static async Task PlayAudio()
        {
            await StopAudio();
            await Task.Delay(1000);
            await AudioPlayService.AddAudioToPlayQueue(audioFilePath);
        }

        private int GetNumberOfUsableCarriers()
        {
            return zoneData.LocationStatusList.Count(p => p.DisabledStatus == 0 &&
                                                          p.IsCargoExist &&
                                                         !string.IsNullOrEmpty(p.CarrierID) &&
                                                         !p.CarrierID.ToLower().Contains("un") &&
                                                         !p.CarrierID.ToLower().Contains("du") &&
                                                         !p.CarrierID.ToLower().Contains("mi")
                                                   );
        }

        private static clsZoneUsableCarrierOptions GetOptionOfZone(string zoneName)
        {
            if (lowLevelMonitorOptionsOfZones.TryGetValue(zoneName, out clsZoneUsableCarrierOptions result))
                return result;
            else
            {
                lowLevelMonitorOptionsOfZones = LoadThresholdSettingFromJsonFile();
                if (!lowLevelMonitorOptionsOfZones.ContainsKey(zoneName))
                {
                    clsZoneUsableCarrierOptions _default = new() { DisplayZoneName = zoneName, ThresHoldValue = 0 };
                    lowLevelMonitorOptionsOfZones.Add(zoneName, _default);
                    SaveThresholdSettingToJsonFile();
                    return _default;
                }
                else
                    return lowLevelMonitorOptionsOfZones[zoneName];
            }
        }

        internal static Dictionary<string, clsZoneUsableCarrierOptions> LoadThresholdSettingFromJsonFile()
        {
            if (File.Exists(ThresholdStoreFileName))
            {
                string json = File.ReadAllText(ThresholdStoreFileName);
                return JsonConvert.DeserializeObject<Dictionary<string, clsZoneUsableCarrierOptions>>(json);
            }
            else
                return new Dictionary<string, clsZoneUsableCarrierOptions>();
        }

        internal static void SaveThresholdSettingToJsonFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ThresholdStoreFileName));
            try
            {
                string json = JsonConvert.SerializeObject(lowLevelMonitorOptionsOfZones, Formatting.Indented);
                File.WriteAllText(ThresholdStoreFileName, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~ZoneCapacityStatusMonitor()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public class clsZoneUsableCarrierOptions
        {
            public string DisplayZoneName { get; set; } = "";
            public int ThresHoldValue { get; set; } = 0;

            /// <summary>
            /// 當可用的Tray盤數即將不足時，通知的訊息模板,RackName, ZoneID, AvailableNumber 為變數
            /// </summary>
            public string NotifyMessageTemplate { get; set; } = "{RackName}{ZoneID} 可用的Tray盤數即將不足，請補空Tray. 當前數量:{AvailableNumber}";
        }
    }
}
