using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.Microservices.AudioPlay;
using AGVSystemCommonNet6.Microservices.MCS;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using static AGVSystemCommonNet6.Microservices.MCS.MCSCIMService.ZoneData;

namespace AGVSystem.Models.EQDevices
{
    public class ZoneCapacityStatusMonitor : IDisposable
    {
        private MCSCIMService.ZoneData zoneData;
        private bool disposedValue;
        private IHubContext<FrontEndDataHub> hubContext;

        clsZoneUsableCarrierOptions options;
        public static Dictionary<string, clsZoneUsableCarrierOptions> thresholdOfZones = new Dictionary<string, clsZoneUsableCarrierOptions>();
        public static ConcurrentDictionary<string, DateTime> NotifyingZoneNames = new();
        static string audioFilePath => Path.Combine(Environment.CurrentDirectory, $"Audios/mcs_transfer_command_recieved.wav");

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
            if (this.hubContext != null)
            {
                string _message = $"{this.options.DisplayZoneName}({zoneData.ZoneName}) 剩餘可用的 Carriers 已快用盡，請補料! 當前可用數量 = [{availabeNumber}]";
                this.hubContext.Clients.All.SendAsync("ZoneUsableCarrierNotEnoughNotify", _message);
            }
            PlayAudio();
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
            if (thresholdOfZones.TryGetValue(zoneName, out clsZoneUsableCarrierOptions result))
                return result;
            else
            {
                thresholdOfZones = LoadThresholdSettingFromJsonFile();
                if (!thresholdOfZones.ContainsKey(zoneName))
                {
                    clsZoneUsableCarrierOptions _default = new() { DisplayZoneName = zoneName, ThresHoldValue = 0 };
                    thresholdOfZones.Add(zoneName, _default);
                    SaveThresholdSettingToJsonFile();
                    return _default;
                }
                else
                    return thresholdOfZones[zoneName];
            }
        }

        private static Dictionary<string, clsZoneUsableCarrierOptions> LoadThresholdSettingFromJsonFile()
        {
            if (File.Exists(ThresholdStoreFileName))
            {
                string json = File.ReadAllText(ThresholdStoreFileName);
                return JsonConvert.DeserializeObject<Dictionary<string, clsZoneUsableCarrierOptions>>(json);
            }
            else
                return new Dictionary<string, clsZoneUsableCarrierOptions>();
        }

        private static void SaveThresholdSettingToJsonFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ThresholdStoreFileName));
            try
            {
                string json = JsonConvert.SerializeObject(thresholdOfZones, Formatting.Indented);
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
        }
    }
}
