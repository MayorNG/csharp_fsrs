// ReSharper disable InconsistentNaming

using System;

namespace csharp_fsrs
{
    // Ref: https://github.com/open-spaced-repetition/fsrs4anki/wiki/The-Algorithm#fsrs-v4
    public class FSRSAlgorithm
    {
        private FSRSParameter Param;
        private readonly double _intervalModifier;
        protected string Seed;
        private readonly double DECAY = -0.5;
        private readonly double FACTOR;

        protected FSRSAlgorithm(FSRSParameter param)
        {
            Param = param;
            // Ref: https://github.com/open-spaced-repetition/fsrs4anki/wiki/The-Algorithm#fsrs-45
            // The formula used is : I(r,S) = S / FACTOR ∙ (r^{1 / DECAY} - 1)
            FACTOR = Math.Pow(0.9, 1 / this.DECAY) - 1;
            _intervalModifier = (Math.Pow(Param.RequestRetention, 1 / DECAY)) / FACTOR;
        }

        /// <summary>
        /// Init difficulty and stability after first rating.
        /// </summary>
        /// <param name="s">Scheduling Card</param>
        protected void InitDS(SchedulingCard s)
        {
            s.Again.Difficulty = this.InitDifficulty((Grade)Rating.Again);
            s.Again.Stability = this.InitStability((Grade)Rating.Again);
            s.Hard.Difficulty = this.InitDifficulty((Grade)Rating.Hard);
            s.Hard.Stability = this.InitStability((Grade)Rating.Hard);
            s.Good.Difficulty = this.InitDifficulty((Grade)Rating.Good);
            s.Good.Stability = this.InitStability((Grade)Rating.Good);
            s.Easy.Difficulty = this.InitDifficulty((Grade)Rating.Easy);
            s.Easy.Stability = this.InitStability((Grade)Rating.Easy);
        }

        /// <summary>
        /// Updates the difficulty and stability values of the scheduling card based on the last difficulty,
        /// last stability, and the current retrievability.
        /// </summary>
        /// <param name="s">Scheduling Card</param>
        /// <param name="lastD">Last Difficulty</param>
        /// <param name="lastS">Last Stability</param>
        /// <param name="retrievability">Current Retrievability</param>
        protected void NextDS(SchedulingCard s, double lastD, double lastS, double retrievability)
        {
            s.Again.Difficulty = this.NextDifficulty(lastD, (Grade)Rating.Again);
            s.Again.Stability = this.NextForgetStability(lastD, lastS, retrievability);
            s.Hard.Difficulty = this.NextDifficulty(lastD, (Grade)Rating.Hard);
            s.Hard.Stability = this.NextRecallStability(lastD, lastS, retrievability, (Grade)Rating.Hard);
            s.Good.Difficulty = this.NextDifficulty(lastD, (Grade)Rating.Good);
            s.Good.Stability = this.NextRecallStability(lastD, lastS, retrievability, (Grade)Rating.Good);
            s.Easy.Difficulty = this.NextDifficulty(lastD, (Grade)Rating.Easy);
            s.Easy.Stability = this.NextRecallStability(lastD, lastS, retrievability, (Grade)Rating.Easy);
        }

        /// <summary>
        /// Init stability for the first rating. 
        /// <para>The formula used is:</para>
        /// <para>S₀(G) = w₍G₋₁₎</para>
        /// <para>max{S₀,0.1}</para>
        /// </summary>
        /// <param name="g">Grade [1.again,2.hard,3.good,4.easy]</param>
        /// <returns>Stability (interval when R=90%)</returns>
        private double InitStability(Grade g)
        {
            return Math.Max(Param.Weights[(int)g - 1], 0.1);
        }

        /// <summary>
        /// Init difficulty for the first rating. 
        /// <para>The formula used is:</para>
        /// <para>D₀(G) = w₄ - w₅ ∙ (G - 3)</para>
        /// </summary>
        /// <param name="g">[1.again,2.hard,3.good,4.easy]</param>
        /// <returns>Difficulty D ∈ [1,10]</returns>
        private double InitDifficulty(Grade g)
        {
            return ConstrainDifficulty(
                Param.Weights[4] - Param.Weights[5] * ((double)g - 3)
            );
        }

        /// <summary>
        /// If fuzzing is disabled or <c>ivl</c> is less than 2.5, it returns the original interval.
        /// <para>As <c>ivl</c> increases, the fuzzed interval will be generated from a larger range.</para>
        /// </summary>
        /// <param name="ivl">The interval to be fuzzed.</param>
        /// <returns>The fuzzed interval.</returns>
        private double ApplyFuzz(double ivl)
        {
            if (!Param.EnableFuzz || ivl < 2.5 || Seed == null) return ivl;
            var generator = new Random(Seed.GetHashCode());
            var fuzzFactor = generator.NextDouble();
            ivl = Math.Round(ivl);
            var minIvl = Math.Max(2, Math.Round(ivl * 0.95 - 1));
            var maxIvl = Math.Round(ivl * 1.05 + 1);
            return Math.Floor(fuzzFactor * (maxIvl - minIvl + 1) + minIvl);
        }

        /// <summary>
        /// Calculates the exact interval days based on the stability
        /// <para>The formula used is :</para>
        /// <para>I(r,S) = S / FACTOR ∙ (r^{1 / DECAY} - 1)</para>
        /// </summary>
        /// <param name="s">Stability (interval when R=90%)</param>
        /// <returns>exact interval days ∈ [1,<c>MaximumInterval</c>]</returns>
        protected ulong NextInterval(double s)
        {
            var newInterval = ApplyFuzz(s * _intervalModifier);
            return (ulong)Math.Min(
                Math.Max(Math.Round(newInterval), 1),
                Param.MaximumInterval
            );
        }

        /// <summary>
        /// Calculates the next difficulty after a rating
        /// <para>The formula used is :</para>
        /// <para><c>nextD</c> = D - w₆ ∙ ( G - 3)</para>
        /// <para>D'(D,G) = w₇ ∙ D₀(3) + (1 - w₇) ∙ <c>nextD</c></para>
        /// </summary>
        /// <param name="d">Difficulty D ∈ [1,10]</param>
        /// <param name="g">Grade [1.again,2.hard,3.good,4.easy]</param>
        /// <returns></returns>
        private double NextDifficulty(double d, Grade g)
        {
            var nextD = d - Param.Weights[6] * ((double)g - 3);
            return ConstrainDifficulty(
                MeanReversion(Param.Weights[4], nextD)
            );
        }

        /// <summary>
        /// Constrains the difficulty value to be within the range of 1 to 10
        /// <para>The formula used is :</para>
        /// <para>min{ max{D,1} , 10 }</para>
        /// </summary>
        /// <param name="difficulty">difficulty</param>
        /// <returns>constrained difficulty</returns>
        private double ConstrainDifficulty(double difficulty)
        {
            return Math.Min(Math.Max(Math.Round(difficulty, 2), 1), 10);
        }

        /// <summary>
        /// Apply the mean reversion to avoid "ease hell"
        /// <para>The formula used is :</para>
        /// <para>w₇ ∙ <c>init</c> + (1 - w₇) ∙ <c>current</c></para>
        /// </summary>
        /// <param name="init">w₄ : D₀(3) = w₄ - w₅ ∙ ( 3 - 3 ) = w₄</param>
        /// <param name="current">D - w₆ ∙ ( G - 3 )</param>
        /// <returns>difficulty after mean reversion</returns>
        private double MeanReversion(double init, double current)
        {
            return Param.Weights[7] * init + (1 - Param.Weights[7]) * current;
        }

        /// <summary>
        /// Calculates the new stability after a successful review (the user pressed "Hard", "Good" or "Easy")
        /// <para>The formula used is :</para>
        /// <para>S'r(D,S,R,G) = S ∙ (1 + (e^{w₈} ∙ (11-D) ∙ S^{-w₉} ∙ (e^{w₁₀∙(1-R)} - 1) ∙ w₁₅(if G=2) ∙ w₁₆(if G=4))</para>
        /// </summary>
        /// <param name="d">Difficulty D ∈ [1,10]</param>
        /// <param name="s">Stability (interval when R=90%)</param>
        /// <param name="r">Retrievability (probability of recall)</param>
        /// <param name="g">Grade [1.again,2.hard,3.good,4.easy]</param>
        /// <returns>S'r new stability after recall</returns>
        private double NextRecallStability(double d, double s, double r, Grade g)
        {
            var hardPenalty = Rating.Hard == (Rating)g ? Param.Weights[15] : 1;
            var easyBonus = Rating.Easy == (Rating)g ? Param.Weights[15] : 1;
            return
                s *
                (
                    1 +
                    Math.Exp(Param.Weights[8]) *
                    (11 - d) *
                    Math.Pow(s, -Param.Weights[9]) *
                    (Math.Exp(Param.Weights[10] * (1 - r)) - 1) *
                    hardPenalty *
                    easyBonus
                );
        }

        /// <summary>
        /// Calculates the stability after forgetting (the user pressed "Again")
        /// <para>The formula used is :</para>
        /// <para>S'f(D,S,R) = w₁₁ ∙ D^{-w₁₂} ∙ ((S+1)^{w₁₃} - 1) ∙ e^{w₁₄∙(1-R)}</para>
        /// </summary>
        /// <param name="d">Difficulty D ∈ [1,10]</param>
        /// <param name="s">Stability (interval when R=90%)</param>
        /// <param name="r">Retrievability (probability of recall)</param>
        /// <returns></returns>
        private double NextForgetStability(double d, double s, double r)
        {
            return
                Math.Round
                (
                    Param.Weights[11] *
                    Math.Pow(d, -Param.Weights[12]) *
                    (Math.Pow(s + 1, Param.Weights[13]) - 1) *
                    Math.Exp((1 - r) * Param.Weights[14])
                    , 2
                );
        }

        /// <summary>
        /// The retrievability after <c>elapsedDays</c> since the last review
        /// <para>The formula used is :</para>
        /// <para>R(t,S) = (1 + FACTOR ∙ t / s)^{DECAY}</para>
        /// </summary>
        /// <param name="elapsedDays">t days since the last review</param>
        /// <param name="stability">Stability (interval when R=90%)</param>
        /// <returns></returns>
        protected double ForgettingCurve(ulong elapsedDays, double stability)
        {
            return Math.Pow(1 + FACTOR * elapsedDays / stability, DECAY);
        }
    }
}