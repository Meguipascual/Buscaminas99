using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public bool IsClient { get; set; }
    public bool IsServer => !IsClient;
}
