// See https://aka.ms/new-console-template for more information

using ServerCore;

Console.WriteLine("Starting server");

var connectionsManager = new ConnectionsManager();
var messageHandler = new MessageHandler(connectionsManager);
var playersManager = new PlayersManager(connectionsManager, messageHandler);

await connectionsManager.Start();

Console.ReadLine();

playersManager.Dispose();
messageHandler.Dispose();
connectionsManager.Dispose();