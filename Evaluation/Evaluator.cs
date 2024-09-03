using EquipmentManagment.Manager;
using NLog;

namespace AGVSystem.Evaluation
{
    public class Evaluator
    {
        public List<KeyFrame> EQStateKeyFrames { get; set; } = new();
        private Logger logger = LogManager.GetCurrentClassLogger();
        public Evaluator()
        {
        }

        public async Task Start()
        {
            //Use Task.Run to run the evaluation in a separate thread
            await Task.Run(async () =>
            {
                logger.Info($"Evaluation Process Start");
                //Loop through all the keyframes
                TimeSpan lastTime = TimeSpan.Zero;
                foreach (var _keyFrame in EQStateKeyFrames)
                {
                    TimeSpan timeDuration = (_keyFrame.Time - lastTime);
                    lastTime = _keyFrame.Time;
                    await Task.Delay(timeDuration);
                    string _log = $"[{_keyFrame.Time}]";
                    foreach (var eqControl in _keyFrame.EQControls)
                    {
                        EquipmentManagment.Emu.clsDIOModuleEmu eqEmulator = StaEQPEmulatorsManagager.GetEQEmuByName(eqControl.EQ.EndPointOptions.Name);
                        //Check the state of the EQControl
                        switch (eqControl.LdUldState)
                        {
                            case EQControl.LDULD_STATE.NO_REQUEST:
                                eqEmulator.SetStatusBUSY();
                                _log += $"\r\n--{eqControl.EQ.EndPointOptions.Name} is BUSY";
                                break;
                            case EQControl.LDULD_STATE.LOAD:
                                eqEmulator.SetStatusLoadable();
                                _log += $"\r\n--{eqControl.EQ.EndPointOptions.Name} is LOADABLE, ";
                                break;
                            case EQControl.LDULD_STATE.UNLOAD:
                                eqEmulator.SetStatusUnloadable();
                                _log += $"\r\n--{eqControl.EQ.EndPointOptions.Name} is UNLOADABLE";
                                break;
                            default:
                                break;
                        }
                    }
                    logger.Info(_log);
                }
            });
        }
    }
}