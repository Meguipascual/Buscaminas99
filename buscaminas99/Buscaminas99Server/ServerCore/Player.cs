using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public class Player 
{
    public int PlayerId {  get; set; }
    public int? Seed { get; private set; }
    public int Score { get; set; }
    public bool HasFinishedBoard { get; private set; }
    public bool IsEliminated { get; set; }

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

    public void TrackBoardFinished(int points) {
        HasFinishedBoard = true;
        Score += points;
        Console.WriteLine($"Player {PlayerId} finished their board");
    }

    public void RestartSeed(int seed) {
        Seed = seed;
        HasFinishedBoard = false;
    }
}
