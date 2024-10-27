
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Equipment;
using AutoMapper;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;
using Microsoft.EntityFrameworkCore;
using static AGVSystemCommonNet6.clsEnums;

namespace AGVSystem.Service
{
    public class EquipmentsCollectBackgroundService : BackgroundService
    {
        AGVSDbContext dbContext;
        public EquipmentsCollectBackgroundService(IServiceScopeFactory scopeFactory)
        {
            this.dbContext = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AGVSDbContext>();
            //dbContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 配置 AutoMapper
            MapperConfiguration mainEqStatusMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<MainEQStatus, MainEQStatus>());
            IMapper mainEqStatusMapper = mainEqStatusMapperConfig.CreateMapper();

            MapperConfiguration rackStatusMapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<RackStatus, RackStatus>());
            IMapper rackStatusMapper = rackStatusMapperConfig.CreateMapper();


            await Task.Delay(1000).ContinueWith(async tk =>
            {

                while (true)
                {
                    try
                    {

                        await Task.Delay(1000);

                        EquipmentManagment.Manager.StaEQPManagager.MainEQList.ForEach(eq =>
                        {
                            MainEQStatus mainEqStatus = CreateMainEQStatus(eq);
                            MainEQStatus mainEqStatusInDB = dbContext.EQStatus_MainEQ.FirstOrDefault(_entry => _entry.Name == mainEqStatus.Name);

                            if (mainEqStatusInDB == null)
                                dbContext.EQStatus_MainEQ.Add(mainEqStatus);
                            else
                            {
                                mainEqStatusMapper.Map(mainEqStatus, mainEqStatusInDB);
                                dbContext.Entry(mainEqStatusInDB).State = EntityState.Modified;
                            }

                        });


                        EquipmentManagment.Manager.StaEQPManagager.RacksList.ForEach(rack =>
                        {

                            RackStatus rackStatus = CreateRackStatus(rack);
                            RackStatus rackStatusInDB = dbContext.EQStatus_Rack.FirstOrDefault(_entry => _entry.Name == rackStatus.Name);
                            if (rackStatusInDB == null)
                                dbContext.EQStatus_Rack.Add(rackStatus);
                            else
                            {
                                rackStatusMapper.Map(rackStatus, rackStatusInDB);
                                dbContext.Entry(rackStatusInDB).State = EntityState.Modified;
                            }

                        });

                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }

        private RackStatus CreateRackStatus(clsRack rack)
        {
            RackStatus status = new RackStatus();
            status.Name = rack.EQName;
            return status;
        }

        private MainEQStatus CreateMainEQStatus(clsEQ eq)
        {
            var status = new MainEQStatus();
            status.Name = eq.EQName;
            return status;
        }




    }
}