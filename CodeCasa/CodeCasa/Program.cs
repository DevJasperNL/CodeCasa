using System.Reflection;

try
{
    // This ensures that the log files are written relative the application's launch folder.
    Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonAppSettings()
        // .UseNetDaemonDefaultLogging()
        .UseGingerLogging()
        .UseNetDaemonRuntime()
        .UseNetDaemonTextToSpeech()
        .ConfigureServices((_, services) =>
            services
                .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
                .AddNetDaemonStateManager()
                .AddNetDaemonScheduler()
                .AddHomeAssistantGenerated()
                .AddGinger()
        )
        .Build()
        .RunAsync()
        .ConfigureAwait(false);
}
catch (Exception e)
{
    Log.Logger.Error($"Failed to start host... {e}");
    throw;
}