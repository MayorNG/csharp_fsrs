namespace csharp_fsrs
{
    public static class Default
    {
        public static readonly double RequestRetention = 0.9;
        public static readonly double MaximumInterval = 36500;
        public static readonly double[] Weights =
       new double[] {
            0.4, 0.6, 2.4, 5.8, 4.93, 0.94, 0.86, 0.01, 1.49, 0.14, 0.94, 2.18, 0.05,
            0.34, 1.26, 0.29, 2.61
        };
        public static readonly bool EnableFuzz = false;
    }
}