using System;

[Serializable]
public enum NetworkMessageTypes
{
    Unknown = 0,
    Seed = 1,
    CellId = 2,
    RivalCellId = 3,
    RivalSeed = 4,
    ResetGameWarning = 5,
    ResetGame = 6,
    ResetServer = 7,
    UndoPlay = 8,
    ConnectionACK = 9,
    NewPlayerConnected = 10,
    GameStarted = 11,
    UndoCommand = 12,
    BoardFinished = 13,
    ScoreUpdated = 14,
    GameEnded = 15,
    PlayerEliminated = 16,
}
