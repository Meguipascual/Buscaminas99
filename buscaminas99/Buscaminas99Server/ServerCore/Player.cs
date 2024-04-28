using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public class Player 
{
    public int PlayerId {  get; set; }
    public int Score { get; set; }

    Stack<Play> Plays;

    public Player() 
    {
        Plays = new Stack<Play>();
    }

    public void PushPlay(CellIdNetworkMessage cellIdNetworkMessage) 
    {
        var play = new Play() { CellId = cellIdNetworkMessage.CellId, DiscoverCellIds = cellIdNetworkMessage.DiscoverCellIds.ToList() };
        Plays.Push(play);
    }

    public Play PopPlay() 
    {
        if(Plays.TryPop(out var result)) 
        {
            return result;
        }
        else
        {
            return null;
        }
    }
}
