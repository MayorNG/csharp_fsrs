// ReSharper disable ConvertToPrimaryConstructor

using System;
using System.Collections.Generic;

namespace csharp_fsrs
{
    public class FSRS : FSRSAlgorithm
    {
        public FSRS(FSRSParameter param) : base(param) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<Grade, RecordLogItem> Repeat(Card card, DateTime now)
        {
            SchedulingCard s = new SchedulingCard(card, now).UpdateState(card.State);
            Seed = (now.ToString() + card.Reps);
            ulong easyInterval, goodInterval, hardInterval;
            switch (card.State)
            {
                case State.New:
                    InitDS(s);
                    s.Again.Due = now.AddMinutes(1);
                    s.Hard.Due = now.AddMinutes(5);
                    s.Good.Due = now.AddMinutes(10);
                    easyInterval = NextInterval(s.Easy.Stability);
                    s.Easy.ScheduledDays = easyInterval;
                    s.Easy.Due = now.AddDays(easyInterval);
                    break;
                case State.Learning:
                case State.Relearning:
                    hardInterval = 0;
                    goodInterval = NextInterval(s.Good.Stability);
                    easyInterval = Math.Max(NextInterval(s.Easy.Stability), goodInterval + 1);
                    s.Schedule(now, hardInterval, goodInterval, easyInterval);
                    break;
                case State.Review:
                    ulong elapsedDays = card.ElapsedDays;
                    double lastD = card.Difficulty;
                    double lastS = card.Stability;
                    double retrievability = ForgettingCurve(elapsedDays, lastS);
                    NextDS(s, lastD, lastS, retrievability);
                    hardInterval = NextInterval(s.Hard.Stability);
                    goodInterval = NextInterval(s.Good.Stability);
                    hardInterval = Math.Min(hardInterval, goodInterval);
                    goodInterval = Math.Max(goodInterval, hardInterval + 1);
                    easyInterval = Math.Max(NextInterval(s.Easy.Stability), goodInterval + 1);
                    s.Schedule(now, hardInterval, goodInterval, easyInterval);
                    break;
                default:
                    throw new Exception("Unknown State");
            }
            
            return s.RecordLog(card, now);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public string GetRetrievability(Card card, DateTime now)
        {
            if (card.State != State.Review) return "";
            ulong t = (ulong)Math.Max(now.Subtract(card.LastReview).Days, 0);
            return Math.Round(ForgettingCurve(t, card.Stability) * 100, 2) + "%";
        }
        
        public Card RollBack(Card card, ReviewLog log)
        {
            if (log.Rating == Rating.Manual)
                throw new Exception("Cannot rollback a manual rating");
            
            DateTime prevDue, prevLastReview;
            ulong prevLapses;
            switch (log.PrevState)
            {
                case State.New:
                    prevDue = log.PrevLastReview;
                    prevLastReview = default(DateTime);
                    prevLapses = 0;
                    break;
                case State.Learning:
                case State.Relearning:
                case State.Review:
                    prevDue = log.PrevDue;
                    prevLastReview = log.PrevLastReview;
                    prevLapses = 
                        card.Lapses - 
                        (ulong)(log.Rating == Rating.Again && log.PrevState == State.Review ? 1 : 0);
                    break;
                default:
                    throw new Exception("Unknown State");
            }

            return new Card(card)
            {
                Due = prevDue,
                Stability = log.PrevStability,
                Difficulty = log.PrevDifficulty,
                ElapsedDays = log.PrevElapsedDays,
                ScheduledDays = log.PrevScheduledDays,
                Reps = Math.Max(0, card.Reps - 1),
                Lapses = Math.Max(0, prevLapses),
                State = log.PrevState,
                LastReview = prevLastReview,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card">C</param>
        /// <param name="now"></param>
        /// <param name="resetCount"></param>
        /// <returns></returns>
        public RecordLogItem Forget(Card card, DateTime now, bool resetCount = false)
        {
            ulong scheduledDays =
                (ulong)(card.State == State.New
                    ? 0
                    : now.Subtract(card.LastReview).Days);

            ReviewLog forgetLog = new ReviewLog
            {
                Rating = Rating.Manual,
                PrevDue = card.Due,
                PrevStability = card.Stability,
                PrevDifficulty = card.Difficulty,
                PrevElapsedDays = card.ElapsedDays,
                PrevScheduledDays = scheduledDays,
                PrevState = card.State,
                PrevLastReview = card.LastReview,
                ElapsedDays = 0,
                Review = now,
            };

            Card forgetCard = new Card(card)
            {
                Due = now,
                Stability = 0,
                Difficulty = 0,
                ElapsedDays = 0,
                ScheduledDays = 0,
                Reps = resetCount ? 0 : card.Reps,
                Lapses = resetCount ? 0 : card.Lapses,
                State = State.New,
                LastReview = card.LastReview,
            };

            return new RecordLogItem()
            {
                Card = forgetCard,
                Log = forgetLog,
            };
        }
    }
}