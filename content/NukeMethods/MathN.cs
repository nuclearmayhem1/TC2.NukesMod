namespace Nuke;

public static class MathN
{

    public static Vector2 VectorByAngleDeg(float angle)
    {
        return new Vector2((float)Math.Cos(angle * Maths.deg2rad), (float)Math.Sin(angle * Maths.deg2rad));
    }

    public static Vector2 VectorByAngleDeg(float angle, float length)
    {
        return new Vector2((float)Math.Cos(angle * Maths.deg2rad), (float)Math.Sin(angle * Maths.deg2rad)) * length;
    }

    public static Vector2 VectorByAngleRad(float angle)
    {
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }

    public static Vector2 VectorByAngleRad(float angle, float length)
    {
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * length;
    }
}