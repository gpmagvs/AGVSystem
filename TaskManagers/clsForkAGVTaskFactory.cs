using AGVSystem.Models.TaskAllocation;
using AGVSytemCommon.AGVMessage;
using AGVSytemCommon.MAP;

namespace AGVSystem.TaskManagers
{
    public class clsForkAGVTaskFactory
    {
        private Map map;

        virtual public List<cls_0301_TaskDownloadHeader> CreateTaskLink(clsTaskBaseDto taskData, int currentTag, TaskAllocator.TASK_RECIEVE_SOURCE source)
        {
            LoadMapData();


            string task_comp_name = $"{source}-{DateTime.Now.ToString("yyyyMMdd_HHmmssfff")}";
            List<cls_0301_TaskDownloadHeader> task_links = new List<cls_0301_TaskDownloadHeader>();
            if (taskData.Action_Name == "move")
            {
                clsMoveTaskDto task_copy = (clsMoveTaskDto)taskData;
                cls_0301_TaskDownloadHeader task_slice = new cls_0301_TaskDownloadHeader()
                {
                    Task_Name = task_comp_name,
                    Task_Sequence = 0,
                    Task_Simplex = string.Format("{0}-{1}", task_comp_name, 0),
                    Action_Type = "move",
                    Station_Type = 0,
                    Time_Stamp = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    Destination = task_copy.To_Tag,
                    Trajectory = CreateTrajectory(currentTag, task_copy.To_Tag)
                };

                task_links.Add(task_slice);
            }

            return task_links;
        }

        private clsMapPoint[] CreateTrajectory(int currentTag, int to_Tag)
        {
            PathFinder finder = new PathFinder();
            PathFinder.clsPathInfo stations_path = finder.FindShortestPathByTagNumber(map.Points, currentTag, to_Tag);

            List<clsMapPoint> result = new List<clsMapPoint>();
            int index = 0;
            foreach (MapStation station in stations_path.stations)
            {
                result.Add(new clsMapPoint
                {
                    index = index,
                    Point_ID = station.TagNumber,
                    X = station.X,
                    Y = station.Y,
                    Theta = station.Direction,
                    Map_Name = map.Name,
                    Auto_Door = new clsAutoDoor
                    {
                        Key_Name = station.AutoDoor.KeyName,
                        Key_Password = station.AutoDoor.KeyPassword,
                    },
                    Control_Mode = new clsControlMode
                    {
                        Dodge = int.Parse(station.DodgeMode),
                        Spin = int.Parse(station.SpinMode)
                    },
                    Laser = station.LsrMode,
                    Speed = station.Speed,

                });
                index++;
            }
            return result.ToArray();

        }

        virtual public void LoadMapData()
        {
            this.map = MapManager.LoadMapFromFile(@"D:\param\Map_UMTC_3F_Yellow.json");
        }
    }
}
