using AGVSystem;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Models.WebsocketMiddleware;
using AGVSystem.Static;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.HttpTools;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Microservices;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.User;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

LOG.ShowClassName = false;
LOG.SetLogFolderName("AGVS LOG");
LOG.INFO("AGVS System Start");
AGVSConfigulator.Init();
TaskManager.Initialize();
EQTransferTaskManager.Initialize();
AGVSMapManager.Initialize();
HotRunScriptManager.Initialize();
ScheduleMeasureManager.Initialize();

_ = StaEQPManagager.InitializeAsync(new clsEQManagementConfigs
{
    UseEqEmu = AGVSConfigulator.SysConfigs.EQManagementConfigs.UseEQEmu,
    EQConfigPath = $"{AGVSConfigulator.SysConfigs.EQManagementConfigs.EquipmentManagementConfigFolder}//EQConfigs.json",
    WIPConfigPath = $"{AGVSConfigulator.SysConfigs.EQManagementConfigs.EquipmentManagementConfigFolder}//WIPConfigs.json",
    ChargeStationConfigPath = $"{AGVSConfigulator.SysConfigs.EQManagementConfigs.EquipmentManagementConfigFolder}//ChargStationConfigs.json",
});
EndPointDeviceAbstract.OnEQDisconnected += EQDeviceEventsHandler.HandleDeviceDisconnected;
EndPointDeviceAbstract.OnEQConnected += EQDeviceEventsHandler.HandleDeviceReconnected;
clsEQ.OnIOStateChanged += EQDeviceEventsHandler.HandleEQIOStateChanged;

AGVSSocketHost agvs_host = new AGVSSocketHost();
agvs_host.Start();
AlarmManagerCenter.Initialize();
VMSDataStore.Initialize();
VMSSerivces.OnVMSReconnected += async (sender, e) => await VMSSerivces.RunModeSwitch(SystemModes.RunMode);
VMSSerivces.AgvStateFetchWorker();
VMSSerivces.AliveCheckWorker();
VMSSerivces.RunModeSwitch(AGVSystemCommonNet6.AGVDispatch.RunMode.RUN_MODE.MAINTAIN);

var builder = WebApplication.CreateBuilder(args);
string DBConnection = AGVSConfigulator.SysConfigs.DBConnection;
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddDirectoryBrowser();

builder.Services.AddDbContext<AGVSDbContext>(options => options.UseSqlServer(DBConnection));

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
        options => options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret_keysecret_keysecret_key11"))
        }
    );
var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        using (AGVSDbContext dbContext = scope.ServiceProvider.GetRequiredService<AGVSDbContext>())
        {
            dbContext.Database.EnsureCreated();
            dbContext.SaveChanges();
            if (dbContext.Database.GetPendingMigrations().Any())
            {

            }
            using (var ttra = dbContext.Database.BeginTransaction())
            {
                UserEntity? existingUser = dbContext.Users.FirstOrDefault(u => u.UserName == "dev");
                if (existingUser == null)
                {
                    dbContext.Users.Add(new UserEntity { UserName = "dev", Password = "12345678", Role = ERole.Developer });
                    dbContext.SaveChanges();
                }
                ttra.Commit();
            }
        }
    }
    catch (Exception ex)
    {
        LOG.ERROR(ex);
    }

}
WebsocketMiddleware.StartCollectWebUIUsingDatas();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(1) });

app.UseDefaultFiles(new DefaultFilesOptions()
{
});
app.UseStaticFiles();


//var imageFolder = Path.Combine(builder.Environment.WebRootPath, "images");
var imageFolder = @"C:\AGVS\Map";
Directory.CreateDirectory(imageFolder);
var fileProvider = new PhysicalFileProvider(imageFolder);
var requestPath = "/MapFiles";

// Enable displaying browser links.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});

app.UseVueRouterHistory();
app.UseAuthorization();
app.MapControllers();
app.Run();
