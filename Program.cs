using AGVSystem;
using AGVSystem.TaskManagers;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.UserManagers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

AGVSSocketHost agvs_host = new AGVSSocketHost();
agvs_host.Start();

var builder = WebApplication.CreateBuilder(args);

string  DBConnection = builder.Configuration.GetConnectionString("DefaultConnection");
Directory.CreateDirectory(Path.GetDirectoryName(DBConnection.Split('=')[1]));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddDbContext<AGVSDbContext>(options => options.UseSqlite(DBConnection));

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

using (var scope = app.Services.CreateScope())
{

    AGVSDbContext TaskDbContext = scope.ServiceProvider.GetRequiredService<AGVSDbContext>();
    TaskDbContext.Database.EnsureCreated();

    var dbContext = scope.ServiceProvider.GetRequiredService<AGVSDbContext>();
    dbContext.Database.EnsureCreated();
    var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "dev");
    if (existingUser == null)
    {
        await dbContext.Users.AddAsync(new UserEntity { Username = "dev", Password = "12345678", Role = UserEntity.USER_ROLE.DEVELOPER });
        dbContext.SaveChanges();
        dbContext.Dispose();
    }

}


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
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
