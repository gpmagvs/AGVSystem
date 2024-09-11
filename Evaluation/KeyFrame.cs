using EquipmentManagment.MainEquipment;

namespace AGVSystem.Evaluation
{
    public class KeyFrame
    {
        public TimeSpan Time { get; set; } = TimeSpan.Zero;
        public List<EQControl> EQControls { get; set; } = new List<EQControl>();
        public KeyFrame()
        {
        }

        public KeyFrame(TimeSpan time)
        {
            Time = time;
        }
    }

    public class EQControl
    {
        public enum LDULD_STATE
        {
            NO_REQUEST,
            LOAD,
            UNLOAD
        }
        public clsEQ EQ { get; set; }

        public LDULD_STATE LdUldState { get; set; } = LDULD_STATE.NO_REQUEST;
    }
}
