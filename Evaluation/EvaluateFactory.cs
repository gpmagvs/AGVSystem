using EquipmentManagment.Manager;

namespace AGVSystem.Evaluation
{
    public static class EvaluateFactory
    {
        public static async Task StartTest()
        {
            EquipmentManagment.MainEquipment.clsEQ simEq = StaEQPManagager.MainEQList.Last();
            EquipmentManagment.MainEquipment.clsEQ simEq2 = StaEQPManagager.MainEQList.Where(eq => eq != simEq).Last();
            Evaluator evaluator = new Evaluator();
            evaluator.EQStateKeyFrames = new()
            {
                 new KeyFrame(TimeSpan.FromSeconds(2))
                 {
                      EQControls = new()
                      {
                           new EQControl
                           {
                                EQ = simEq,
                                 LdUldState = EQControl.LDULD_STATE.UNLOAD
                           },
                           new EQControl
                           {
                                EQ = simEq2,
                                 LdUldState = EQControl.LDULD_STATE.LOAD
                           }
                      }
                 },
                 new KeyFrame(TimeSpan.FromSeconds(5))
                 {
                      EQControls = new()
                      {
                           new EQControl
                           {
                                EQ = simEq,
                                 LdUldState = EQControl.LDULD_STATE.NO_REQUEST
                           },
                           new EQControl
                           {
                                EQ = simEq2,
                                 LdUldState = EQControl.LDULD_STATE.UNLOAD
                           }
                      }
                 }
            };

            await evaluator.Start();
        }
    }
}
