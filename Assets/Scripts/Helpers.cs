using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static int ManhattanDistance(int x1, int x2, int y1, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }
}
