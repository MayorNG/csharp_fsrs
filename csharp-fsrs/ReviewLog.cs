using System;

namespace csharp_fsrs
{
    public class ReviewLog
    {
        /// <summary>
        /// Rating of the logged review (Again, Hard, Good, Easy)
        /// </summary>
        public Rating Rating { get; set; }
        
        /// <summary>
        /// previous <c>Due</c> of the card
        /// </summary>
        public DateTime PrevDue { get; set; }

        /// <summary>
        /// previous <c>Stability</c> of the card
        /// </summary>
        public double PrevStability { get; set; }

        /// <summary>
        /// previous <c>Difficulty</c> of the card
        /// </summary>
        public double PrevDifficulty { get; set; }

        /// <summary>
        /// previous <c>ElapsedDays</c> of the card
        /// </summary>
        public ulong PrevElapsedDays { get; set; }

        /// <summary>
        /// previous <c>ScheduledDays</c> of the card
        /// </summary>
        public ulong PrevScheduledDays { get; set; }

        /// <summary>
        /// previous <c>State</c> of the card
        /// </summary>
        public State PrevState { get; set; }

        /// <summary>
        /// previous <c>LastReview</c> of the card
        /// </summary>
        public DateTime PrevLastReview { get; set; }

        /// <summary>
        /// Number of days elapsed since the review prior to the logged review of the card
        /// </summary>
        public ulong ElapsedDays { get; set; }

        /// <summary>
        /// Date of this review
        /// </summary>
        public DateTime Review { get; set; }
    }
}