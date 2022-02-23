using Hazel.Udp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientManager : MonoBehaviour
{
    private UnityUdpClientConnection clientConnection;
    // Start is called before the first frame update
    void Start()
    {
        clientConnection = new UnityUdpClientConnection(new UnityLogger(true), new IPEndPoint(IPAddress.Loopback, 6500));
        clientConnection.Connect();
    }
}
