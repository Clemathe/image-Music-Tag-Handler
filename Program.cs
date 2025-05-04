
using System.Diagnostics;
using musicTag;
using DotNetEnv;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var logger = Log.ForContext<CoverOptimizer>();

try
{
    logger.Information("🔍 Scan des dossiers en cours...");
    Stopwatch stopwatch = Stopwatch.StartNew();

    Env.Load();
    logger.Debug("Répertoire de travail actuel : {CurrentDirectory}", Environment.CurrentDirectory);

    string baseDirectory = Env.GetString("BASE_DIRECTORY");
    var optimizer = new CoverOptimizer(logger); 
    optimizer.ProcessMusicFolders(baseDirectory);

    stopwatch.Stop();
    logger.Information("⏱️ Programme terminée en : {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
}
catch (Exception ex)
{
    logger.Fatal(ex, "Une erreur fatale s'est produite.");
}
finally
{
    Log.CloseAndFlush();
}
