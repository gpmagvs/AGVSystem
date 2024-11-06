using AGVSystemCommonNet6.MAP;

namespace AGVSystem.Models.Map
{
    public static class Extensions
    {

        private static AGVSystemCommonNet6.MAP.Map currentMap => AGVSMapManager.CurrentMap;

        public static MapPoint GetMapPoint(this int tag)
        {
            return currentMap.Points.Values.FirstOrDefault(p => p.TagNumber == tag);
        }
    }
}
