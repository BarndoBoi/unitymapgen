public static class FloatExtensions
{
    public static bool Approximately(this float a, float b, float tolerance = 0.01f)
    {
        return UnityEngine.Mathf.Abs(a - b) < tolerance; //Explicit call cause I don't feel like importing for one function :/
    }
}