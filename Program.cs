using AGVSystem;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6;
using AGVSystemCommonNet6.Alarm;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Microservices;
using AGVSystemCommonNet6.UserManagers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

AGVSSocketHost agvs_host = new AGVSSocketHost();
agvs_host.Start();

var builder = WebApplication.CreateBuilder(args);


string DBConnection = Configs.DBConnection;
Directory.CreateDirectory(Path.GetDirectoryName(DBConnection.Split('=')[1]));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

var connectionString = new SqliteConnectionStringBuilder(DBConnection)
{
    Mode = SqliteOpenMode.ReadWriteCreate,
}.ToString();

builder.Services.AddDbContext<AGVSDbContext>(options => options.UseSqlite(connectionString));

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
    using (AGVSDbContext dbContext = scope.ServiceProvider.GetRequiredService<AGVSDbContext>())
    {
        dbContext.Database.EnsureCreated();
        dbContext.SaveChanges();

        using (var ttra = dbContext.Database.BeginTransaction())
        {
            UserEntity? existingUser = dbContext.Users.FirstOrDefault(u => u.Username == "dev");
            if (existingUser == null)
            {
                dbContext.Users.Add(new UserEntity { Username = "dev", Password = "12345678", Role = UserEntity.USER_ROLE.DEVELOPER });
                dbContext.SaveChanges();
            }
            ttra.Commit();
        }
    }
}

AlarmManagerCenter.Initialize();

AliveChecker.VMSAliveCheckWorker();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(1) });

app.UseDefaultFiles(new DefaultFilesOptions()
{
});

app.UseStaticFiles();

//app.UseHttpsRedirection();
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthorization();

app.MapControllers();





app.Run();
