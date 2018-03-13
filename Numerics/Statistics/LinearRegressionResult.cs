﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta.Numerics.Statistics {

    /// <summary>
    /// Describes the result of a linear regression.
    /// </summary>
    public sealed class LinearRegressionResult : GeneralLinearRegressionResult {

        internal LinearRegressionResult (IReadOnlyList<double> x, IReadOnlyList<double> y) :
            base () {

            double yMean, xxSum, xySum, yySum;
            Bivariate.ComputeBivariateMomentsUpToTwo(x, y, out n, out xMean, out yMean, out xxSum, out yySum, out xySum);

            b = xySum / xxSum;
            a = yMean - b * xMean;

            residuals = new List<double>(n);
            SSR = 0.0;
            SSF = 0.0;
            for(int i = 0; i < n; i++) {
                double yi = y[i];
                double ypi = a + b * x[i];
                double zi = yi - ypi;
                residuals.Add(zi);
                SSR += zi * zi;
                SSF += MoreMath.Sqr(ypi - yMean);
            }
            SST = yySum;

            xVariance = xxSum / n;
            sigmaSquared = SSR / (n - 2);
            cbb = sigmaSquared / xVariance / n;
            cab = -xMean * cbb;
            caa = (xVariance + xMean * xMean) * cbb;

            rTest = new Lazy<TestResult>(() => {
                double r = xySum / Math.Sqrt(xxSum * yySum);
                TestResult rTest = new TestResult("r", r, TestType.TwoTailed, new Distributions.PearsonRDistribution(n));
                return (rTest);
            });

        }

        private readonly int n;

        private readonly double a, b;

        private readonly double cbb, cab, caa;

        private readonly double SST, SSF, SSR;

        private readonly double xMean, xVariance, sigmaSquared;

        private readonly List<double> residuals;

        private readonly Lazy<TestResult> rTest;

        /// <summary>
        /// Gets an estimate, with uncertainty, of the intercept of the line.
        /// </summary>
        public override UncertainValue Intercept {
            get {
                return (new UncertainValue(a, Math.Sqrt(caa)));
            }
        }

        /// <summary>
        /// Gets an estimate, with uncertainty, of the slope of the line.
        /// </summary>
        public UncertainValue Slope {
            get {
                return (new UncertainValue(b, Math.Sqrt(cbb)));
            }
        }

        /// <summary>
        /// Gets an estimate, with uncertainty, of the scatter of the data around the line.
        /// </summary>
        public UncertainValue Sigma {
            get {
                double sigma = Math.Sqrt(sigmaSquared);
                return (new UncertainValue(sigma, sigma / Math.Sqrt(n)));
            }
        }

        /// <summary>
        /// Predicts the Y value at a new X value.
        /// </summary>
        /// <param name="x">The new X value.</param>
        /// <returns>The predicted value of Y and its associated uncertainty.</returns>
        public UncertainValue Predict (double x) {
            double y = a + b * x;
            double yVariance = sigmaSquared * (1.0 + (MoreMath.Sqr(x - xMean) / xVariance + 1.0) / n);
            return (new UncertainValue(y, Math.Sqrt(yVariance)));
        }

        /// <inheritdoc/>
        public IReadOnlyList<double> Residuals {
            get {
                return (residuals);
            }
        }

        /// <summary>
        /// Gets the Pearson R test of linear correlation.
        /// </summary>
        public TestResult R {
            get {
                return (rTest.Value);
            }
        }

        internal override ParameterCollection CreateParameters () {
            ParameterCollection parameters = new ParameterCollection(
                nameof(Intercept), a, caa, nameof(Slope), b, cbb, cab
//                nameof(Slope), b, cbb, nameof(Intercept), a, caa, cab
            );
            return (parameters);
        }

        internal override OneWayAnovaResult CreateAnova () {
            AnovaRow fit = new AnovaRow(SSF, 1);
            AnovaRow residual = new AnovaRow(SSR, n - 2);
            AnovaRow total = new AnovaRow(SST, n - 1);
            OneWayAnovaResult anova = new OneWayAnovaResult(fit, residual, total);
            return (anova);
        }

    }

    /*
    /// <summary>
    /// Describes the result of a linear regression.
    /// </summary>
    public sealed class LinearRegressionResult : GeneralLinearRegressionResult {

        internal LinearRegressionResult (
            ParameterCollection parameters,
            TestResult rTest,
            OneWayAnovaResult anova,
            List<double> residuals,
            Func<double, UncertainValue> predict
        ) : base(parameters, anova, residuals) {
            this.rTest = rTest;
            this.predict = predict;
        }

        private readonly Func<double, UncertainValue> predict;

        private readonly TestResult rTest;

        /// <summary>
        /// Gets the best fit value of the intercept and its associated uncertainty.
        /// </summary>
        public override UncertainValue Intercept {
            get {
                return (this.Parameters[0].Estimate);
            }
        }

        /// <summary>
        /// Gets the best-fit value of the slope and its associated uncertainty.
        /// </summary>
        public UncertainValue Slope {
            get {
                return (this.Parameters[1].Estimate);
            }
        }

        /// <summary>
        /// Predicts the Y value at a new X value.
        /// </summary>
        /// <param name="x">The new X value.</param>
        /// <returns>The predicted value of Y and its associated uncertainty.</returns>
        public UncertainValue Predict (double x) {
            return (predict(x));
        }

        /// <summary>
        /// Gets the Pearson R test of linear correlation.
        /// </summary>
        public TestResult R {
            get {
                return (rTest);
            }
        }

    }
    */

}