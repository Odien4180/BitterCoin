
using UnityEngine;

public struct HitArgs
{
    public Vector3 FirePosition;
    public int Damage;
    public Transform Target;
}

public interface Hitable
{
    void Hit(HitArgs args);
}
