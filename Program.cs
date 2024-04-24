using AGVSystem;
using AGVSystem.Models.Automation;
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
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
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
using System.Configuration;
using System.Reflection;
using System.Security.Policy;
using System.Text;
Console.Title = "GPM-AGV系統(AGVs)";
LOG.SetLogFolderName("AGVS LOG");
LOG.INFO("AGVS System Start");
AGVSConfigulator.Init();
try
{
    AGVSDatabase.Initialize().GetAwaiter().GetResult();
}
catch (Exception ex)
{
    Console.WriteLine($"資料庫初始化異常-請確認資料庫! {ex.Message}");
    Environment.Exit(4);
}

EQTransferTaskManager.Initialize();
AGVSMapManager.Initialize();
HotRunScriptManager.Initialize();
ScheduleMeasureManager.Initialize();

EQDeviceEventsHandler.Initialize();

clsEQ.OnIOStateChanged += EQDeviceEventsHandler.HandleEQIOStateChanged;

AGVSSocketHost agvs_host = new AGVSSocketHost();
agvs_host.Start();
AlarmManagerCenter.Initialize();
AlarmManager.LoadVCSTrobleShootings();
VMSDataStore.Initialize();
VMSSerivces.OnVMSReconnected += async (sender, e) => await VMSSerivces.RunModeSwitch(SystemModes.RunMode);
VMSSerivces.AgvStateFetchWorker();
VMSSerivces.AliveCheckWorker();
VMSSerivces.RunModeSwitch(AGVSystemCommonNet6.AGVDispatch.RunMode.RUN_MODE.MAINTAIN);


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opton =>
{
    opton.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GPM 派車系統 RESTFul API",
        Version = "V1"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        opton.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddDirectoryBrowser();


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
builder.Services.AddDbContext<AGVSDbContext>(options => options.UseSqlServer(AGVSConfigulator.SysConfigs.DBConnection));

var app = builder.Build();

_ = Task.Run(async () =>
{
    await Task.Delay(3000);
    try
    {
        StaEQPManagager.InitializeAsync(new clsEQManagementConfigs
        {
            EQConfigPath = $"{AGVSConfigulator.SysConfigs.EQManagementConfigs.EquipmentManagementConfigFolder}//EQConfigs.json",
            WIPConfigPath = $"{AGVSConfigulator.SysConfigs.EQManagementConfigs.EquipmentManagementConfigFolder}//WIPConfigs.json",
            ChargeStationConfigPath = $"{AGVSConfigulator.SysConfigs.EQManagementConfigs.EquipmentManagementConfigFolder}//ChargStationConfigs.json",
        });
    }
    catch (Exception ex)
    {
        AlarmManagerCenter.AddAlarmAsync(ALARMS.SYSTEM_EQP_MANAGEMENT_INITIALIZE_FAIL_WITH_EXCEPTION);
        LOG.Critical(ex);
    }
});

AGVSWebsocketServerMiddleware.Middleware.Initialize();
AutomationManager.InitialzeDefaultTasks();
AutomationManager.StartAllAutomationTasks();
app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(1) });

app.UseDefaultFiles(new DefaultFilesOptions());
app.UseStaticFiles();

// 加載配置文件
var mapFileFolderPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:FolderPath");
var mapFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:RequestPath");
Directory.CreateDirectory(mapFileFolderPath);
var mapFileProvider = new PhysicalFileProvider(mapFileFolderPath);

// Enable displaying browser links.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = mapFileProvider,
    RequestPath = mapFileRequestPath
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = mapFileProvider,
    RequestPath = mapFileRequestPath
});

var agvDisplayImageFolder = Path.Combine(app.Environment.WebRootPath, @"images\AGVDisplayImages");
Directory.CreateDirectory(agvDisplayImageFolder);

app.UseVueRouterHistory();
app.UseAuthorization();
app.MapControllers();
app.Run();
