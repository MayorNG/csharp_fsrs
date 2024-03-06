namespace csharp_fsrs
{
    public enum State
    {
        New = 0,
        Learning = 1,
        Review = 2,
        Relearning = 3,
    }

    public enum Rating
    {
        Manual = 0,
        Again = 1,
        Hard = 2,
        Good = 3,
        Easy = 4,
    }

    public enum Grade
    {
        Again = 1,
        Hard = 2,
        Good = 3,
        Easy = 4,
    }

    public struct RecordLogItem
    {
        public Card Card ;
        public ReviewLog Log ;
    }
}

