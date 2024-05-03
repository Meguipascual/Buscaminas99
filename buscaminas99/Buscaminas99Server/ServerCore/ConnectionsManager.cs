using System.Net;
using Hazel;
using Hazel.Udp;

namespace ServerCore; 

public sealed class ConnectionsManager : IDisposable {
    
    private const int Port = 6501;

    private UdpConnectionListener _connectionListener = null!;
    private int _nextConnectionId;
    private readonly Dictionary<int, Connection> _connectionsById = new();

    public delegate Task ConnectionCreatedDelegate(int connectionId);
    public event ConnectionCreatedDelegate OnConnectionCreated = null!;
    
    public delegate Task MessageReceivedDelegate(int messageSenderConnectionId, MessageReader messageReader);
    public event MessageReceivedDelegate OnMessageReceived = null!;

    public Task Start() {
        _connectionListener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, Port));
        _connectionListener.NewConnection += async newConnectionEventArgs => await HandleNewConnection(newConnectionEventArgs);
        _connectionListener.Start();
        return Task.CompletedTask;
    }

    public void Reset() {
        _nextConnectionId = 0;
        foreach (var kvp in _connectionsById) {
            kvp.Value.Disconnect("Reset game");
        }
        _connectionsById.Clear();
    }

    private async Task HandleNewConnection(NewConnectionEventArgs newConnectionEventArgs) {
        var newConnectionId = _nextConnectionId; 
        Console.WriteLine(newConnectionEventArgs.Connection.EndPoint.Address + ":" + newConnectionEventArgs.Connection.EndPoint.Port);
        newConnectionEventArgs.Connection.DataReceived += async dataReceivedEventArgs => await OnMessageReceived.Invoke(newConnectionId, dataReceivedEventArgs.Message.ReadMessage());
        _connectionsById[newConnectionId] = newConnectionEventArgs.Connection; 
        await OnConnectionCreated.Invoke(newConnectionId);
        
        Console.WriteLine($"Connection {newConnectionId} created");

        _nextConnectionId++;
    }

    public void SendMessageToConnection(int connectionId, INetworkMessage message) {
        var messageWriter = message.BuildMessageWriter();
        _connectionsById[connectionId].Send(messageWriter);
        message.BuildMessageWriter().Recycle();
    }

    public void SendMessageToAllConnectionsExceptOne(int connectionIdToExclude, INetworkMessage networkMessage) {
        var messageWriter = networkMessage.BuildMessageWriter();
        for (var i= 0; i < _nextConnectionId; i++)
        {
            if(connectionIdToExclude != i)
            {
                _connectionsById[i].Send(messageWriter);
            }
        }
        messageWriter.Recycle();
    }
    
    public void BroadcastMessage(INetworkMessage networkMessage) {
        var messageWriter = networkMessage.BuildMessageWriter();
        foreach (var (_, connection) in _connectionsById) {
            connection.Send(messageWriter);
        }
        messageWriter.Recycle();
    }
    
    public void BroadcastEmptyMessage(NetworkMessageTypes messageType) {
        var networkMessage = new EmptyNetworkMessage();
        networkMessage.SetNetworkMessageType(messageType);
        var messageWriter = networkMessage.BuildMessageWriter();
        foreach (var (_, connection) in _connectionsById) {
            connection.Send(messageWriter);
        }
        messageWriter.Recycle();
    }

    public void Dispose() {
        _connectionListener.Dispose();
    }
}