using AGVSystem;
using AGVSystem.Models.Automation;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Service;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.BackgroundServices;
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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.WebSockets;
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
VMSSerivces.OnVMSReconnected += async (sender, e) => await VMSSerivces.RunModeSwitch(SystemModes.RunMode);
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
builder.Services.AddHostedService<DatabaseBackgroundService>();
builder.Services.AddHostedService<VehicleLocationMonitorBackgroundService>();
builder.Services.AddHostedService<FrontEndDataCollectionBackgroundService>();
builder.Services.AddScoped<MeanTimeQueryService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyMethod()
                    .AllowAnyHeader()
                     .SetIsOriginAllowed(origin => true) // 允许任何来源
                     .AllowCredentials(); // 允许凭据
    });
});
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(600);
});
builder.Services.AddSignalR().AddJsonProtocol(options => { options.PayloadSerializerOptions.PropertyNamingPolicy = null; }); ;

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
        AlarmManagerCenter.AddAlarmAsync(ALARMS.SYSTEM_EQP_MANAGEMENT_INITIALIZE_FAIL_WITH_EXCEPTION);//SYSTEM_EQP_MANAGEMENT_INITIALIZE_FAIL_WITH_EXCEPTION
        LOG.Critical(ex);
    }
});

AutomationManager.InitialzeDefaultTasks();
AutomationManager.StartAllAutomationTasks();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll"); app.UseWebSockets();
app.UseDefaultFiles(new DefaultFilesOptions());
app.UseStaticFiles();

// 加載配置文件
var mapFileFolderPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:FolderPath");
var mapFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:RequestPath");
var agvImageFileFolderPath = app.Configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:FolderPath");
var agvImageFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:RequestPath");

Directory.CreateDirectory(mapFileFolderPath);
Directory.CreateDirectory(agvImageFileFolderPath);

var mapFileProvider = new PhysicalFileProvider(mapFileFolderPath);
var agvImageFileProvider = new PhysicalFileProvider(agvImageFileFolderPath);

// Enable displaying browser links.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = mapFileProvider,
    RequestPath = mapFileRequestPath
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = agvImageFileProvider,
    RequestPath = agvImageFileRequestPath
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = mapFileProvider,
    RequestPath = mapFileRequestPath
});
CreateDefaultAGVImage();

void CreateDefaultAGVImage()
{
    try
    {
        var exist_fileNames = Directory.GetFiles(agvImageFileFolderPath).Select(file => Path.GetFileName(file)).ToList();

        for (int i = 0; i < 10; i++)
        {
            //AGV_003-Icon.png;
            string agvImageFileName = $"AGV_00{i}-Icon.png";
            string defaultImgFullFileName = "./Resources/AGVImages/default.png";
            string agvImageFullFileName = Path.Combine(agvImageFileFolderPath, agvImageFileName);
            if (!exist_fileNames.Contains(agvImageFileName))
            {
                File.Copy(defaultImgFullFileName, agvImageFullFileName, true);
            }
        }

    }
    catch (Exception ex)
    {
        LOG.ERROR($"Program-CreateDefaultAGVImage Error :{ex.Message}", ex);
    }
}


try
{
    Directory.CreateDirectory(AGVSConfigulator.SysConfigs.TrobleShootingFolder);
    var trobleshootingFileRequestPath = app.Configuration.GetValue<string>("TrobleShootingFileOptions:TrobleShootingFile:RequestPath");
    var trobleshootingFileProvider = new PhysicalFileProvider(AGVSConfigulator.SysConfigs.TrobleShootingFolder);

    // Enable displaying browser links.
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = trobleshootingFileProvider,
        RequestPath = trobleshootingFileRequestPath
    });

    app.UseDirectoryBrowser(new DirectoryBrowserOptions
    {
        FileProvider = trobleshootingFileProvider,
        RequestPath = trobleshootingFileRequestPath
    });
}
catch (Exception ex)
{
    LOG.ERROR(ex.Message, ex);
}


var agvDisplayImageFolder = Path.Combine(app.Environment.WebRootPath, @"images\AGVDisplayImages");
Directory.CreateDirectory(agvDisplayImageFolder);

app.UseVueRouterHistory();
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<FrontEndDataHub>("/FrontEndDataHub");
app.Run();
