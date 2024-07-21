using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Client
{

    internal class Program : IDisposable
    {
        private static IDisposable _hostDisposable;


        private static async Task Main(string[] args)
        {
            var client = await StartHost();
            var game = new GuessGame(client);
            game.StartGameplay();

            await Task.Delay(-1);
        }


        private static async Task<IClusterClient> StartHost()
        {
            var host = Host.CreateDefaultBuilder()
               .UseOrleansClient(builder =>
               {
                   builder.UseLocalhostClustering();
                   builder.AddMemoryStreams("StreamProvider");
               })
               .Build();
            _hostDisposable = host;
            await host.StartAsync();

            var client = host.Services.GetRequiredService<IClusterClient>();
            return client;
        }

        public void Dispose()
        {
            _hostDisposable?.Dispose();
        }
    }
}