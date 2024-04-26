using Microsoft.Extensions.DependencyInjection;
using ServerCore;

await using var services = new ServiceCollection()
    .AddSingleton<ConnectionsManager>()
    .AddSingleton<MessageHandler>()
    .AddSingleton<PlayersManager>()
    .AddSingleton<ServerController>()
    .BuildServiceProvider();

var connectionsManager = services.GetService<ConnectionsManager>();
services.GetService<MessageHandler>();
services.GetService<PlayersManager>();
services.GetService<ServerController>();

await connectionsManager!.Start();

Console.ReadLine();