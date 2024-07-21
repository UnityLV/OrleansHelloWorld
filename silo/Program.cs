using Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime.Hosting;
using System.Reflection;

try
{
    await Host.CreateDefaultBuilder(args)
     .UseOrleans(builder =>
     {
         builder.UseLocalhostClustering();
         builder.AddMemoryStreams(Constans.StreamProvider);

         builder.ConfigureLogging(logging =>
         {

             logging.SetMinimumLevel(LogLevel.Debug);

             logging.AddFilter("Orleans", LogLevel.Information);
         });

         builder.ConfigureServices(services =>
         {
             services.AddGrainStorage(Constans.GrainStrorageName, (i, s) => new FileSystemGrainStorage(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
         });
     })
     .RunConsoleAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadLine();
}

