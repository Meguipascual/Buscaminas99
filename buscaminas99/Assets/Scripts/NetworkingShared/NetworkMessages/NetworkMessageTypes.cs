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
}
