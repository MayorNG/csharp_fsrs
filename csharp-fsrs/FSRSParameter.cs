using System;

namespace csharp_fsrs
{
    public class FSRSParameter
    {
        public double RequestRetention { get; set; } 
        public double MaximumInterval { get; set; }
        public double[] Weights { get; set; }
        public bool EnableFuzz { get; set; }

        public FSRSParameter()
        {
            RequestRetention = Default.RequestRetention;
            MaximumInterval = Default.MaximumInterval;
            Weights = Default.Weights;
            EnableFuzz = Default.EnableFuzz;
            
        }

        public FSRSParameter(double? requestRetention, double? maximumInterval, double[] weights, bool? enableFuzz)
        {
            if (weights != null && weights.Length != 17)
            {
                throw new ArgumentException("Weights length must be 17.");
            }
            if (weights != null)
            {
                Weights = new double[17];
                Array.Copy(weights, Weights, 17);
            }
            else
            {
                Weights = Default.Weights;
            }
        
            RequestRetention = requestRetention ?? Default.RequestRetention;
            MaximumInterval = maximumInterval ?? Default.MaximumInterval;
            EnableFuzz = enableFuzz ?? Default.EnableFuzz;
        }
    }
}