using Hazel;
using Hazel.Udp;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private UdpConnectionListener connectionListener;
    // Start is called before the first frame update
    void Start()
    {
        connectionListener = new UdpConnectionListener(new IPEndPoint(IPAddress.Any, 6500));
        connectionListener.NewConnection += PrintConnection;
        connectionListener.Start();
    }
    private void PrintConnection(NewConnectionEventArgs args)
    {
        Debug.Log(args.Connection.EndPoint.Address + ":" + args.Connection.EndPoint.Port);
    }
}
