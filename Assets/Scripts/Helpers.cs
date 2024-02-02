using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static int ManhattanDistance(Vector2 point1, Vector2 point2)
    {
        return Mathf.Abs((int)point2.x - (int)point1.x) + Mathf.Abs((int)point2.y - (int)point1.y);
    }
}
