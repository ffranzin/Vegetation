using UnityEngine;

public static class VectorSwizzleExtension
{
    public static Vector3 zyx(this Vector3 v)
    {
        return new Vector3(v.z, v.y, v.x);
    }
    public static Vector3 zxy(this Vector3 v)
    {
        return new Vector3(v.z, v.x, v.y);
    }
    public static Vector3 yxz(this Vector3 v)
    {
        return new Vector3(v.y, v.x, v.z);
    }
    public static Vector3 yzx(this Vector3 v)
    {
        return new Vector3(v.y, v.z, v.x);
    }
    public static Vector3 xzy(this Vector3 v)
    {
        return new Vector3(v.x, v.z, v.y);
    }
    public static Vector3 xyz(this Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
    public static Vector3 x0z(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }
}

public static class Vector2Swizzle
{
    public static Vector2 xy(this Vector2 v)
    {
        return new Vector2(v.x, v.y);
    }
    public static Vector2 yx(this Vector2 v)
    {
        return new Vector2(v.y, v.x);
    }
    
}
