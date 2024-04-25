using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

internal class Player
{
    Stack<Play> Plays;
    public void SavePlay(CellIdNetworkMessage cellIdNetworkMessage) 
    {
        var play = new Play() { CellId = cellIdNetworkMessage.CellId, DiscoverCellIds = cellIdNetworkMessage.DiscoverCellIds.ToList() };

        Plays.Push(play);
    }

}
