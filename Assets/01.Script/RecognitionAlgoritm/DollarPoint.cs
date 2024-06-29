using System;
using UnityEngine;

[Serializable]
public struct DollarPoint
{
    public Vector2 point;

    public DollarPoint(float x, float y)
    {
        point = new Vector2(x, y);
    }
}
