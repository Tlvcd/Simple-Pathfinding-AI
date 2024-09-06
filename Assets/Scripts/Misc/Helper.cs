
public static class Helper
{
    public static bool InBetween(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }
}
