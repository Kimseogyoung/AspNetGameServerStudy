namespace Server.Helper
{
    public static class Tolerance
    {
        public static bool AreEquals(double value1, double value2)
        {
            var diff = Math.Abs(value1 - value2);
            return diff < c_doubleTolerance;
        }

        public const double c_doubleTolerance = 0.001;
    }
}
