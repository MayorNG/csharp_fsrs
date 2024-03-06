using System;

namespace csharp_fsrs
{
    public class Card
    {
        /// <summary>
        /// Due date of next review.
        /// <para>(The date when R≈90%)</para>
        /// </summary>
        public DateTime Due { get; set; }

        /// <summary>
        /// Stability (interval when R=90%)
        /// </summary>
        public double Stability { get; set; }

        /// <summary>
        /// Difficulty level
        /// </summary>
        public double Difficulty { get; set; }

        /// <summary>
        /// The <c>ElapsedDays</c> of the last review.
        /// <para>Which represents the time elapsed from the review prior to the last review to the last review.</para>
        /// </summary>
        public ulong ElapsedDays { get; set; }

        /// <summary>
        /// Number of days scheduled for next review
        /// </summary>
        public ulong ScheduledDays { get; set; }

        /// <summary>
        /// Repetition count
        /// </summary>
        public ulong Reps { get; set; }

        /// <summary>
        /// Number of lapses or mistakes (State.Review -> State.Relearning)
        /// </summary>
        public ulong Lapses { get; set; }

        /// <summary>
        /// Card's state (New, Learning, Review, Relearning)
        /// </summary>
        public State State { get; set; }

        /// <summary>
        /// The date of the last review of this card
        /// </summary>
        public DateTime LastReview { get; set; }

        public Card(DateTime? now = null)
        {
            Due = now ?? DateTime.Now;
            Stability = 0;
            Difficulty = 0;
            ElapsedDays = 0;
            ScheduledDays = 0;
            Reps = 0;
            Lapses = 0;
            State = State.New;
            LastReview = default(DateTime);
        }
        
        public Card(Card card)
        {
            Due = card.Due;
            Stability = card.Stability;
            Difficulty = card.Difficulty;
            ElapsedDays = card.ElapsedDays;
            ScheduledDays = card.ScheduledDays;
            Reps = card.Reps;
            Lapses = card.Lapses;
            State = card.State;
            LastReview = card.LastReview;
        }
    }
}