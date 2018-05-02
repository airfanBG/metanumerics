﻿using System;

using Meta.Numerics.Functions;

namespace Meta.Numerics.Statistics.Distributions {

    /// <summary>
    /// Represents the distribution of Pearsons's r statistic.
    /// </summary>
    public sealed class PearsonRDistribution : ContinuousDistribution {

        /// <summary>
        /// Initializes a new instance of the Pearson r distribution for the sample size.
        /// </summary>
        /// <param name="n">The number of pairs, which must be three or more.</param>
        public PearsonRDistribution (int n) {
            if (n < 3) throw new ArgumentOutOfRangeException(nameof(n));
            this.n = n;
        }

        private readonly int n;

        /// <inheritdoc />
        public override double ProbabilityDensity (double x) {
            if (Math.Abs(x) > 1.0) {
                return (0.0);
            } else {
                return (Math.Pow((1.0 - x) * (1.0 + x), (n - 4) / 2.0) / AdvancedMath.Beta(0.5, (n - 2) / 2.0));
            }
        }

        /// <inheritdoc />
        public override Interval Support {
            get {
                return (Interval.FromEndpoints(-1.0, 1.0));
            }
        }

        /// <inheritdoc />
        public override double Mean {
            get {
                return(0.0);
            }
        }

        /// <inheritdoc />
        public override double Median {
            get {
                return(0.0);
            }
        }

        /// <inheritdoc />
        public override double Variance {
            get {
                return (1.0 / (n - 1));
            }
        }

        /// <inheritdoc />
        public override double Skewness {
            get {
                return (0.0);
            }
        }

        /// <inheritdoc />
        public override double CentralMoment (int r) {
            if (r < 0) {
                throw new ArgumentOutOfRangeException(nameof(r));
            } else if (r % 2 != 0) {
                return (0.0);
            } else {
                double M = 1.0;
                for (int i = 0; i < r; i+= 2) {
                    M = M * (i + 1) / (n + i - 1);
                }
                return (M);
            }
        }

        /// <inheritdoc />
        public override double RawMoment (int r) {
            return (CentralMoment(r));
        }

        /// <inheritdoc />
        public override double LeftProbability (double x) {
            if (x <= -1.0) {
                return (0.0);
            } else if (x < 0.0) {
                return (AdvancedMath.Beta((n - 2) / 2.0, 0.5, (1.0 - x) * (1.0 + x)) / AdvancedMath.Beta((n-2) / 2.0, 0.5) / 2.0);
            } else if (x < 1.0) {
                return ((1.0 + AdvancedMath.Beta(0.5, (n - 2) / 2.0, x * x) / AdvancedMath.Beta(0.5, (n-2) / 2.0)) / 2.0);
            } else {
                return (1.0);
            }
        }

        /// <inheritdoc />
        public override double RightProbability (double x) {
            return (LeftProbability(-x));
        }

        // central probability between -x and x is I_{\sqrt{x}}(1/2, a+1)

    }
}
