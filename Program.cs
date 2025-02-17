using AGVSystem;
using AGVSystem.Hubs;
using AGVSystem.Models.Automation;
using AGVSystem.Models.EQDevices;
using AGVSystem.Models.Map;
using AGVSystem.Models.Sys;
using AGVSystem.Models.TaskAllocation.HotRun;
using AGVSystem.Service;
using AGVSystem.Service.Aggregates;
using AGVSystem.Service.MCS;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.Configuration;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.DATABASE.BackgroundServices;
using AGVSystemCommonNet6.Log;
using AGVSystemCommonNet6.Material;
using AGVSystemCommonNet6.Microservices.MCS;
using AGVSystemCommonNet6.Microservices.VMS;
using AGVSystemCommonNet6.Notify;
using AGVSystemCommonNet6.Sys;
using AGVSystemCommonNet6.Vehicle_Control.VCS_ALARM;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using KGSWebAGVSystemAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using static AGVSystemCommonNet6.MAP.Map;
using Task = System.Threading.Tasks.Task;

public class Program
{
    static bool hotRunEnabled = false;
    static string hotRunScriptName = string.Empty;
    public static void Main(string[] args)
    {
        if (ProcessTools.IsProcessRunning("AGVSystem", out List<int> pids))
        {
            Console.WriteLine($"AGVSystem Program is already running({string.Join(",", pids)})");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        Console.WriteLine("args:" + string.Join(",", args));
        foreach (var arg in args)
        {
            if (arg.Equals("--hotrun", StringComparison.OrdinalIgnoreCase))
            {
                hotRunEnabled = true;
                Console.WriteLine("Auto HotRun enabled");
            }
            else if (arg.StartsWith("--script=", StringComparison.OrdinalIgnoreCase))
            {
                hotRunScriptName = arg.Substring("--script=".Length);
                Console.WriteLine("hotRunScriptName = " + hotRunScriptName);
            }
        }

        string appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.Title = $"GPM-AGV系統(AGVs)-v{appVersion}";
        Logger logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        EnvironmentVariables.AddUserVariable("AGVSystemInstall", Environment.CurrentDirectory);
        try
        {
            LOG.SetLogFolderName("AGVS LOG");
            logger.Info("AGVS Start");

            WebApplicationBuilder builder = SystemInitializer.Initialize(args,logger);
            WebAppInitializer.ConfigureBuilder(builder);

            var app = builder.Build();

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                logger.Error($"Ctrl+c triggered 應用程式正在關閉中...");
            });
            lifetime.ApplicationStopped.Register(() =>
            {
                logger.Error($"Ctrl+c triggered 應用程式已關閉");
            });

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                logger.Error($"Close action botton triggered.  應用程式正在關閉中...");
            };

            WebAppInitializer.ConfigureApp(builder, app);

            if (hotRunEnabled)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    (bool, string) result = HotRunScriptManager.Run(hotRunScriptName);
                    logger.Info($"HotRunScriptManager.Run({hotRunScriptName}) result: {result.Item1}, {result.Item2}");
                });
            }
            SystemInitializer.InitSysStatusDBStoreWithAppVersionAsync(builder, appVersion);
            app.Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex);
            Environment.Exit(ex.GetHashCode());
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}

public static class SystemInitializer
{
    public static WebApplicationBuilder  Initialize(string[] args,Logger logger)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        string testAppsettingJsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "appsettings.Test.json");
        builder.Configuration.AddJsonFile(testAppsettingJsonFilePath, optional: true, true);//嘗試注入測試用   

        string configRootFolder = builder.Configuration.GetValue<string>("AGVSConfigFolder");
        configRootFolder = string.IsNullOrEmpty(configRootFolder) ? @"C:\AGVS" : configRootFolder;
        logger.Debug($"派車系統參數檔資料夾路徑={configRootFolder}");

        AGVSConfigulator.Init(configRootFolder);
        InitializeDatabase(logger);

        EQTransferTaskManager.Initialize();
        AGVSMapManager.Initialize();
        HotRunScriptManager.Initialize();
        ScheduleMeasureManager.Initialize();
        EQDeviceEventsHandler.Initialize();
        AlarmManagerCenter.InitializeAsync().GetAwaiter().GetResult();
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

        return builder;
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

    public static async Task InitSysStatusDBStoreWithAppVersionAsync(WebApplicationBuilder build, string appVersion)
    {
        await build.Services.BuildServiceProvider().GetRequiredService<SystemStatusDbStoreService>().InitSysStatusWithAppVersion(appVersion);

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
        ServicesInjection(builder);
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
        if (AGVSConfigulator.SysConfigs.BaseOnKGSWebAGVSystem)
        {
            builder.Services.AddDbContext<WebAGVSystemContext>(options => options.UseSqlServer(AGVSConfigulator.SysConfigs.KGSWebAGVSystemDBConnection));
        }
        EnvironmentVariables.AddUserVariable("AGVSDatabaseConnection", AGVSConfigulator.SysConfigs.DBConnection);
    }

    private static void ServicesInjection(WebApplicationBuilder builder)
    {
        SECSConfigsService _secsConfigsService = new SECSConfigsService(Path.Combine(AGVSConfigulator.SysConfigs.CONFIGS_ROOT_FOLDER, "SECSConfigs"));
        _secsConfigsService.InitializeAsync();

        EQIOStatusMonitorBackgroundService qIOStatusMonitorBackgroundService = new EQIOStatusMonitorBackgroundService();

        if (!Debugger.IsAttached)
            builder.Services.AddHostedService<ThirdPartyProgramStartService>();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<MeanTimeQueryService>();
        builder.Services.AddScoped<LogDownlodService>();
        builder.Services.AddScoped<StationSelectService>();
        builder.Services.AddScoped<SystemStatusDbStoreService>();
        builder.Services.AddScoped<RackService>();
        builder.Services.AddScoped<MCSService>();
        builder.Services.AddScoped<DatabaseMigrateService>();
        builder.Services.AddScoped<SECSConfigsService>(service => _secsConfigsService);
        builder.Services.AddScoped<TrafficStateDataQueryService>();
        builder.Services.AddScoped<SystemModesAggregateService>();
        builder.Services.AddSingleton<DBDataService>();
        builder.Services.AddSingleton<EQIOStatusMonitorBackgroundService>(provider => qIOStatusMonitorBackgroundService);
        builder.Services.AddHostedService<DatabaseBackgroundService>();
        builder.Services.AddHostedService<VehicleLocationMonitorBackgroundService>();
        builder.Services.AddHostedService<FrontEndDataBrocastService>();
        builder.Services.AddHostedService<PCPerformanceService>();
        builder.Services.AddHostedService<EquipmentInitStartupService>();
        builder.Services.AddHostedService<EquipmentsCollectBackgroundService>();
        builder.Services.AddHostedService<RackPortDoubleIDMonitor>();
        builder.Services.AddHostedService<TaskManagerInitService>();
        builder.Services.AddHostedService<EQIOStatusMonitorBackgroundService>(provider => qIOStatusMonitorBackgroundService);
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {

        // 動態獲取本機所有 IP 地址
        List<string>? localIPs = Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .Select(address => address.ToString())
                            .ToList();

        List<string> allowedOrigins = new List<string> {
            "http://localhost:8080",
            "http://127.0.0.1:8080",
            "http://127.0.0.1:7107"
        };
        foreach (var ip in localIPs)
        {
            allowedOrigins.Add($"http://{ip}:5216");
            allowedOrigins.Add($"http://{ip}:5036");
            allowedOrigins.Add($"http://{ip}:8080");
            allowedOrigins.Add($"http://{ip}:8081");
            allowedOrigins.Add($"http://{ip}:8082");
            allowedOrigins.Add($"http://{ip}:8083");
            allowedOrigins.Add($"http://{ip}:8084");
            allowedOrigins.Add($"http://{ip}:7107");
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

    public static void ConfigureApp(WebApplicationBuilder builder, WebApplication app)
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
        AutomationManager.InitialzeDefaultTasks();
        AutomationManager.StartAllAutomationTasks();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors("AllowAll");
        app.UseWebSockets();

        string frontendRootFolder = builder.Configuration.GetValue<string>("FrontendRootFolder");
        PhysicalFileProvider frontendFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, frontendRootFolder ?? "wwwroot"));
        app.UseDefaultFiles(new DefaultFilesOptions()
        {
            FileProvider = frontendFileProvider,
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = frontendFileProvider,
        });

        StaticFileInitializer.Initialize(app);

        app.UseVueRouterHistory();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<FrontEndDataHub>("/FrontEndDataHub");
        app.MapHub<SecsPlatformHub>("/SecsPlatform");
    }

}

public static class StaticFileInitializer
{
    public static void Initialize(WebApplication app)
    {

        string configRootFolder = app.Configuration.GetValue<string>("AGVSConfigFolder");
        configRootFolder = string.IsNullOrEmpty(configRootFolder) ? @"C:\AGVS" : configRootFolder;
        try
        {
            string mapFileFolderRelativePath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:FolderPath");
            string mapFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:MapFile:RequestPath");
            mapFileRequestPath = mapFileRequestPath ?? "/MapFiles";
            string agvImageFileFolderRelativePath = app.Configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:FolderPath");
            agvImageFileFolderRelativePath = agvImageFileFolderRelativePath ?? "/AGVImages";
            string agvImageFileRequestPath = app.Configuration.GetValue<string>("StaticFileOptions:AGVImageStoreFile:RequestPath");
            agvImageFileRequestPath = agvImageFileRequestPath ?? "/AGVImages";

            string mapFileFolderPath = Path.Combine(configRootFolder, mapFileRequestPath.Trim('/'));
            string agvImageFileFolderPath = Path.Combine(configRootFolder, agvImageFileFolderRelativePath.Trim('/'));

            Directory.CreateDirectory(mapFileFolderPath);
            Directory.CreateDirectory(agvImageFileFolderPath);

            var mapFileProvider = new PhysicalFileProvider(mapFileFolderPath);
            var agvImageFileProvider = new PhysicalFileProvider(agvImageFileFolderPath);

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = mapFileProvider,
                RequestPath = mapFileRequestPath
            });
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

            CreateDefaultAGVImage(agvImageFileFolderPath);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }

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
            Console.WriteLine(ex.Message + ex.StackTrace);
        }

        try
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetTempPath()),
                RequestPath = "/Download"
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(Path.GetTempPath()),
                RequestPath = "/Download"
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }

        try
        {
            PhysicalFileProvider resourcesFileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "Resources"));

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = resourcesFileProvider,
                RequestPath = "/Resources"
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = resourcesFileProvider,
                RequestPath = "/Resources"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
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
