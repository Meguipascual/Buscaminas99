using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NetworkMessageTypes
{
    Unknown = 0,
    Seed = 1,
    CellId = 2,
    RivalCellId = 3,
    RivalSeed = 4,
}
