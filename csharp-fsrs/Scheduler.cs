using System;
using System.Collections.Generic;

namespace csharp_fsrs
{
    public class SchedulingCard
    {
        /// <summary>
        /// Scheduled Card after graded as "Again" 
        /// </summary>
        public Card Again;
        
        /// <summary>
        /// Scheduled Card after graded as "Hard" 
        /// </summary>
        public Card Hard;
        
        /// <summary>
        /// Scheduled Card after graded as "Good" 
        /// </summary>
        public Card Good;
        
        /// <summary>
        /// Scheduled Card after graded as "Easy" 
        /// </summary>
        public Card Easy;
        
        /// <summary>
        /// <c>LastReview</c> of the Original card
        /// </summary>
        private DateTime _prevLastReview;
        
        /// <summary>
        /// <c>ElapsedDays</c> of the Original card
        /// </summary>
        private ulong _prevElapsedDays;

        /// <summary>
        /// init Scheduled Cards corresponding to all Grade.
        /// </summary>
        /// <param name="card">Original Card</param>
        /// <param name="now">Review Date</param>
        public SchedulingCard(Card card, DateTime now)
        {
            _prevLastReview = !card.LastReview.Equals(default(DateTime)) ? card.LastReview : card.Due;
            _prevElapsedDays = card.ElapsedDays;
            card.ElapsedDays = card.State == State.New ? 0 : (ulong)now.Subtract(card.LastReview).Days;
            card.LastReview = now;
            card.Reps += 1;
            Again = new Card(card);
            Hard = new Card(card);
            Good = new Card(card);
            Easy = new Card(card);
        }
        
        /// <summary>
        /// Update all Scheduled Cards' state according to their grade
        /// <code>
        /// |     S\G    |    Again   |    Hard    |    Good    |    Easy    |
        /// | :--------: | :--------: | :--------: | :--------: | :--------: |
        /// |     New    |  Learning  |  Learning  |  Learning  |  Learning  |
        /// |  Learning  |  Learning  |  Learning  |   Review   |   Review   |
        /// |   Review   | Relearning |   Review   |   Review   |   Review   |
        /// | Relearning | Relearning | Relearning |   Review   |   Review   |
        /// </code>
        /// </summary>
        /// <param name="state">The current state of the Card being scheduled.</param>
        /// <returns>This instance in which all Scheduled Cards' state have been updated.</returns>
        public SchedulingCard UpdateState(State state)
        {
            switch (state)
            {
                case State.New:
                    Again.State = State.Learning;
                    Hard.State = State.Learning;
                    Good.State = State.Learning;
                    Easy.State = State.Review;
                    break;
                case State.Learning:
                case State.Relearning:
                    Again.State = state;
                    Hard.State = state;
                    Good.State = State.Review;
                    Easy.State = State.Review;
                    break;
                case State.Review:
                    Again.State = State.Relearning;
                    Hard.State = State.Review;
                    Good.State = State.Review;
                    Easy.State = State.Review;
                    break;
                default:
                    throw new Exception("Unknown State");
            }
            return this;
        }

        /// <summary>
        /// Update all Scheduled Cards' <c>ScheduledDays</c> and <c>Due</c>
        /// </summary>
        /// <param name="now">Review Date</param>
        /// <param name="hardInterval">Interval if the rating is hard</param>
        /// <param name="goodInterval">Interval if the rating is good</param>
        /// <param name="easyInterval">Interval if the rating is easy</param>
        /// <returns>This instance in which all Scheduled Cards' <c>ScheduledDays</c> and <c>Due</c> have been updated.</returns>
        public SchedulingCard Schedule(DateTime now, ulong hardInterval, ulong goodInterval, ulong easyInterval)
        {
            Again.ScheduledDays = 0;
            Hard.ScheduledDays = hardInterval;
            Good.ScheduledDays = goodInterval;
            Easy.ScheduledDays = easyInterval;
            
            Again.Due = now.AddMinutes(5);
            Hard.Due = hardInterval > 0 ? now.AddDays(hardInterval) : now.AddMinutes(10);
            Good.Due = now.AddDays(goodInterval);
            Easy.Due = now.AddDays(easyInterval);
            return this;
        }

        /// <summary>
        /// Record Log for each rating
        /// </summary>
        /// <param name="card">Original Card</param>
        /// <param name="now">Review Date</param>
        /// <returns>Dictionary with keys of all Grade and corresponding RecordLogItem as value</returns>
        public Dictionary<Grade, RecordLogItem> RecordLog(Card card, DateTime now)
        {
            return new Dictionary<Grade, RecordLogItem>
            {
                {
                    Grade.Again, new RecordLogItem
                    {
                        Card = Again,
                        Log = new ReviewLog
                        {
                            Rating = Rating.Again,
                            PrevDue = card.Due,
                            PrevStability = card.Stability,
                            PrevDifficulty = card.Difficulty,
                            PrevElapsedDays = _prevElapsedDays,
                            PrevScheduledDays = card.ScheduledDays,
                            PrevState = card.State,
                            PrevLastReview = _prevLastReview,
                            ElapsedDays = card.ElapsedDays,
                            Review = now,
                        }
                    }
                },
                {
                    Grade.Hard, new RecordLogItem
                    {
                        Card = Hard,
                        Log = new ReviewLog
                        {
                            Rating = Rating.Hard,
                            PrevDue = card.Due,
                            PrevStability = card.Stability,
                            PrevDifficulty = card.Difficulty,
                            PrevElapsedDays = _prevElapsedDays,
                            PrevScheduledDays = card.ScheduledDays,
                            PrevState = card.State,
                            PrevLastReview = _prevLastReview,
                            ElapsedDays = card.ElapsedDays,
                            Review = now,
                        }
                    }
                },
                {
                    Grade.Good, new RecordLogItem
                    {
                        Card = Good,
                        Log = new ReviewLog
                        {
                            Rating = Rating.Good,
                            PrevDue = card.Due,
                            PrevStability = card.Stability,
                            PrevDifficulty = card.Difficulty,
                            PrevElapsedDays = _prevElapsedDays,
                            PrevScheduledDays = card.ScheduledDays,
                            PrevState = card.State,
                            PrevLastReview = _prevLastReview,
                            ElapsedDays = card.ElapsedDays,
                            Review = now,
                        }
                    }
                },
                {
                    Grade.Easy, new RecordLogItem
                    {
                        Card = Easy,
                        Log = new ReviewLog
                        {
                            Rating = Rating.Easy,
                            PrevDue = card.Due,
                            PrevStability = card.Stability,
                            PrevDifficulty = card.Difficulty,
                            PrevElapsedDays = _prevElapsedDays,
                            PrevScheduledDays = card.ScheduledDays,
                            PrevState = card.State,
                            PrevLastReview = _prevLastReview,
                            ElapsedDays = card.ElapsedDays,
                            Review = now,
                        }
                    }
                },
            };
        }
    }
}