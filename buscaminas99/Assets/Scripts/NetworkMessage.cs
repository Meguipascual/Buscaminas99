using System;

[Serializable]
public class NetworkMessage 
{
    public string Text { get; set; }
    public int CellId { get; set; }
    public int ConnectionId { get; set; }
    public int? Seed { get; set; }
}
