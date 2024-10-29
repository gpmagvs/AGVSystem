using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AGVSystem.ViewModel.WebOperate
{
    public class MapOperateLogViewModel
    {
        public enum FEATURE_TYPE
        {
            STATION = 0,
            PATH = 1,
            AGV = 2,
            OTHERS = 999
        }

        public FEATURE_TYPE FeatureType { get; set; } = FEATURE_TYPE.OTHERS;
        public string FeatureTypeString => FeatureType.ToString();
        public int TagNumber { get; set; } = -1;
        public string Display { get; set; } = "";
    }
}
