using MhozaifaA.OtpVerification;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOtpVerification(o=>o.UseInMemoryCache());
builder.Services.AddOtpVerification();//redis




builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders(); // Remove existing logging providers
    logging.AddConsole(options =>
    {
        // Customize console output
        options.IncludeScopes = true;
        options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
        options.DisableColors = false; // Enable console colors
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

//app.MapControllers();

app.UseEndpoints(e=> { e.MapControllers();
    e.MapOtpVerification();
});

app.Run();
