namespace Server.Helper
{
    public static class Tolerance
    {
        public static bool AreEquals(double value1, double value2)
        {
            var diff = Math.Abs(value1 - value2);
            return diff < DoubleTolerance;
        }

        public const double DoubleTolerance = 0.001;
    }
}
