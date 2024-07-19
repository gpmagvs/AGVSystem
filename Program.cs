using AGVSystem;
using AGVSystem.Models.Automation;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Service;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.BackgroundServices;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
using AGVSystemCommonNet6.Sys;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System;
using System.Net;
using System.Reflection;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.Title = $"GPM-AGV系統(AGVs)-v{appVersion}";
        Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        EnvironmentVariables.AddUserVariable("AGVSystemInstall", Environment.CurrentDirectory);
        try
        {
            LOG.SetLogFolderName("AGVS LOG");
            logger.Info("AGVS Start");

            SystemInitializer.Initialize(logger);

            var builder = WebApplication.CreateBuilder(args);
            WebAppInitializer.ConfigureBuilder(builder);

            var app = builder.Build();
            WebAppInitializer.ConfigureApp(app);

            app.Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}

public static class SystemInitializer
{
    public static void Initialize(Logger logger)
    {
        AGVSConfigulator.Init();
        InitializeDatabase(logger);

        EQTransferTaskManager.Initialize();
        AGVSMapManager.Initialize();
        HotRunScriptManager.Initialize();
        ScheduleMeasureManager.Initialize();
        EQDeviceEventsHandler.Initialize();
        clsEQ.OnIOStateChanged += EQDeviceEventsHandler.HandleEQIOStateChanged;

        AGVSSocketHost agvsHost = new AGVSSocketHost();
        agvsHost.Start();

        AlarmManagerCenter.Initialize();
        AlarmManager.LoadVCSTrobleShootings();
        VMSSerivces.OnVMSReconnected += async (sender, e) => await VMSSerivces.RunModeSwitch(SystemModes.RunMode);
        VMSSerivces.AliveCheckWorker();
        VMSSerivces.RunModeSwitch(AGVSystemCommonNet6.AGVDispatch.RunMode.RUN_MODE.MAINTAIN);

        NotifyServiceHelper.OnMessage += NotifyServiceHelper_OnMessage;
        void NotifyServiceHelper_OnMessage(object? sender, NotifyServiceHelper.NotifyMessage notifyMessage)
        {
            Logger _logger = LogManager.GetLogger("NotifierLog");
            Task.Run(() =>
            {
                string msg = notifyMessage.message;
                switch (notifyMessage.type)
                {
                    case NotifyServiceHelper.NotifyMessage.NOTIFY_TYPE.info:
                        _logger.Info(msg);
                        break;
                    case NotifyServiceHelper.NotifyMessage.NOTIFY_TYPE.warning:
                        _logger.Warn(msg);
                        break;
                    case NotifyServiceHelper.NotifyMessage.NOTIFY_TYPE.error:
                        _logger.Error(msg);
                        break;
                    case NotifyServiceHelper.NotifyMessage.NOTIFY_TYPE.success:
                        _logger.Info(msg);
                        break;
                    default:
                        break;
                }

                //
            });
        }
    }
    private static void InitializeDatabase(Logger logger)
    {
        try
        {
            logger.Info("Database Initialize...");
            AGVSDatabase.Initialize().GetAwaiter().GetResult();
            logger.Info("Database Initialize...Done");
        }
        catch (Exception ex)
        {
            logger.Fatal($"資料庫初始化異常-請確認資料庫! {ex.Message}");
            Environment.Exit(4);
        }
    }
}

public static class WebAppInitializer
{
    public static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.AddServerHeader = false;
        });
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        builder.Host.UseNLog();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "GPM 派車系統 RESTFul API",
                Version = "V1"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

        builder.Services.AddDirectoryBrowser();
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null;
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.WriteIndented = true;
        });

        ConfigureAuthentication(builder);
        ConfigureDatabase(builder);
        ConfigureHostedServices(builder);
        ConfigureCors(builder);
        ConfigureWebSockets(builder);
        ConfigureSignalR(builder);
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<UserValidationService>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
            options => options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secret_keysecret_keysecret_key11"))
            }
        );
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AGVSDbContext>(options => options.UseSqlServer(AGVSConfigulator.SysConfigs.DBConnection));
    }

    private static void ConfigureHostedServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<DatabaseBackgroundService>();
        builder.Services.AddHostedService<VehicleLocationMonitorBackgroundService>();
        builder.Services.AddHostedService<FrontEndDataBrocastService>();
        builder.Services.AddScoped<MeanTimeQueryService>();
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {

        // 動態獲取本機所有 IP 地址
        List<string>? localIPs = Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .Select(address => address.ToString())
                            .ToList();

        List<string> allowedOrigins = new List<string> { "http://localhost:8080" };
        foreach (var ip in localIPs)
        {
            allowedOrigins.Add($"{ip}:5216");
            allowedOrigins.Add($"{ip}:5036");
        }
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(allowedOrigins.ToArray())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
    }

    private static void ConfigureWebSockets(WebApplicationBuilder builder)
    {
        builder.Services.AddWebSockets(options =>
        {
            options.KeepAliveInterval = TimeSpan.FromSeconds(600);
        });
    }

    private static void ConfigureSignalR(WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = null;
        });
    }

    public static void ConfigureApp(WebApplication app)
    {

        // 動態獲取本機所有 IP 地址
        string connectSrc = "";
        List<string>? localIPs = Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .Select(address => address.ToString())
                            .ToList();
        foreach (var ip in localIPs)
        {
            connectSrc += $"ws://{ip}:5036 http://{ip}:5036 ws://{ip}:5216 http://{ip}:5216 ";
        }

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("Content-Security-Policy", $"");
            context.Response.Headers.Remove("Server");
            await next();
        });

        app.UseMiddleware<ApiLoggingMiddleware>();
        Task.Run(() => InitializeEquipmentManager());

        AutomationManager.InitialzeDefaultTasks();
        AutomationManager.StartAllAutomationTasks();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("AllowAll");
        app.UseWebSockets();
        app.UseDefaultFiles(new DefaultFilesOptions());
        app.UseStaticFiles();

        StaticFileInitializer.Initialize(app);

        app.UseVueRouterHistory();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<FrontEndDataHub>("/FrontEndDataHub");
    }

    private static async Task InitializeEquipmentManager()
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
    }
}

public static class StaticFileInitializer
{
    public static void Initialize(WebApplication app)
    {
        var mapFileFolderPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:FolderPath");
        var mapFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:RequestPath");
        var agvImageFileFolderPath = app.Configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:FolderPath");
        var agvImageFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:RequestPath");

        Directory.CreateDirectory(mapFileFolderPath);
        Directory.CreateDirectory(agvImageFileFolderPath);

        var mapFileProvider = new PhysicalFileProvider(mapFileFolderPath);
        var agvImageFileProvider = new PhysicalFileProvider(agvImageFileFolderPath);

        //app.UseDirectoryBrowser(new DirectoryBrowserOptions
        //{
        //    FileProvider = mapFileProvider,
        //    RequestPath = mapFileRequestPath
        //});
        //app.UseStaticFiles(new StaticFileOptions
        //{
        //    FileProvider = mapFileProvider,
        //    RequestPath = mapFileRequestPath
        //});

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = agvImageFileProvider,
            RequestPath = agvImageFileRequestPath
        });


        CreateDefaultAGVImage(agvImageFileFolderPath);

        try
        {
            Directory.CreateDirectory(AGVSConfigulator.SysConfigs.TrobleShootingFolder);
            var trobleshootingFileRequestPath = app.Configuration.GetValue<string>("TrobleShootingFileOptions:TrobleShootingFile:RequestPath");
            var trobleshootingFileProvider = new PhysicalFileProvider(AGVSConfigulator.SysConfigs.TrobleShootingFolder);

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
    }

    private static void CreateDefaultAGVImage(string agvImageFileFolderPath)
    {
        try
        {
            var existFileNames = Directory.GetFiles(agvImageFileFolderPath).Select(file => Path.GetFileName(file)).ToList();

            for (int i = 0; i < 10; i++)
            {
                string agvImageFileName = $"AGV_00{i}-Icon.png";
                string defaultImgFullFileName = "./Resources/AGVImages/default.png";
                string agvImageFullFileName = Path.Combine(agvImageFileFolderPath, agvImageFileName);

                if (!existFileNames.Contains(agvImageFileName))
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
}
