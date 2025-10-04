using Serilog;

namespace WebCatWebBackend;

public static class Program
{

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("web-log.txt")
            .WriteTo.Console()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        var app = builder.Build();
        app.MapControllers();

        app.Run();
    }
}