﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Meta.Numerics.Functions;
using Meta.Numerics.Statistics;
using Meta.Numerics.Statistics.Distributions;

namespace Test {

    [TestClass]
    public class NullDistributionTests {

        [TestMethod]
        public void KolmogorovNullDistributionTest () {

            // The distribution is irrelevent; pick one at random 
            ContinuousDistribution sampleDistribution = new LognormalDistribution();

            // Loop over various sample sizes
            foreach (int n in TestUtilities.GenerateIntegerValues(4, 128, 8)) {

                // Create a sample to hold the KS statistics
                Sample testStatistics = new Sample();
                // and a variable to hold the claimed null distribution, which should be the same for each test
                ContinuousDistribution nullDistribution = null;

                // Create a bunch of samples, each with n data points
                for (int i = 0; i < 128; i++) {

                    // Just use n+i as a seed in order to get different points each time
                    Sample sample = TestUtilities.CreateSample(sampleDistribution, n, 512 * n + i + 1);

                    // Do a KS test of the sample against the distribution each time
                    TestResult r1 = sample.KolmogorovSmirnovTest(sampleDistribution);

                    // Record the test statistic value and the claimed null distribution
                    testStatistics.Add(r1.Statistic.Value);
                    nullDistribution = r1.Statistic.Distribution;

                }

                // Do a Kuiper test of our sample of KS statistics against the claimed null distribution
                // We could use a KS test here instead, which would be way cool and meta, but we picked Kuiper instead for variety
                TestResult r2 = testStatistics.KuiperTest(nullDistribution);
                Assert.IsTrue(r2.Probability > 0.01);

                // Test moment matches, too
                Assert.IsTrue(testStatistics.PopulationMean.ConfidenceInterval(0.99).ClosedContains(nullDistribution.Mean));
                Assert.IsTrue(testStatistics.PopulationStandardDeviation.ConfidenceInterval(0.99).ClosedContains(nullDistribution.StandardDeviation));

            }

        }

        [TestMethod]
        public void KuiperNullDistributionTest () {

            // The distribution is irrelevent; pick one at random 
            ContinuousDistribution sampleDistribution = new NormalDistribution();

            // Loop over various sample sizes
            foreach (int n in TestUtilities.GenerateIntegerValues(2, 128, 8)) {

                // Create a sample to hold the KS statistics
                Sample testStatistics = new Sample();
                // and a variable to hold the claimed null distribution, which should be the same for each test
                ContinuousDistribution nullDistribution = null;

                // Create a bunch of samples, each with n+1 data points
                // We pick n+1 instead of n just to have different sample size values than in the KS test case
                for (int i = 0; i < 256; i++) {

                    // Just use n+i as a seed in order to get different points each time
                    Sample sample = TestUtilities.CreateSample(sampleDistribution, n + 1, 512 * n + i + 2);

                    // Do a Kuiper test of the sample against the distribution each time
                    TestResult r1 = sample.KuiperTest(sampleDistribution);

                    // Record the test statistic value and the claimed null distribution
                    testStatistics.Add(r1.Statistic.Value);
                    nullDistribution = r1.Statistic.Distribution;

                }

                // Do a KS test of our sample of Kuiper statistics against the claimed null distribution
                // We could use a Kuiper test here instead, which would be way cool and meta, but we picked KS instead for variety
                TestResult r2 = testStatistics.KolmogorovSmirnovTest(nullDistribution);
                Assert.IsTrue(r2.Probability > 0.01);

                // Test moment matches, too
                Assert.IsTrue(testStatistics.PopulationMean.ConfidenceInterval(0.99).ClosedContains(nullDistribution.Mean));
                Assert.IsTrue(testStatistics.PopulationVariance.ConfidenceInterval(0.99).ClosedContains(nullDistribution.Variance));

            }

        }

        [TestMethod]
        public void TwoSampleKolmogorovNullDistributionTest () {

            Random rng = new Random(4);
            ContinuousDistribution population = new ExponentialDistribution();

            int[] sizes = new int[] { 23, 30, 175 };

            foreach (int na in sizes) {
                foreach (int nb in sizes) {

                    Sample d = new Sample();
                    ContinuousDistribution nullDistribution = null;
                    for (int i = 0; i < 128; i++) {
                        List<double> a = TestUtilities.CreateDataSample(rng, population, na).ToList();
                        List<double> b = TestUtilities.CreateDataSample(rng, population, nb).ToList();

                        TestResult r = Univariate.KolmogorovSmirnovTest(a, b);
                        d.Add(r.Statistic.Value);
                        nullDistribution = r.Statistic.Distribution;

                    }

                    // Only do full KS test if the number of bins is larger than the sample size, otherwise we are going to fail
                    // because the KS test detects the granularity of the distribution.
                    TestResult mr = d.KolmogorovSmirnovTest(nullDistribution);
                    if (AdvancedIntegerMath.LCM(na, nb) > d.Count) Assert.IsTrue(mr.Probability > 0.01);
                    // But always test that mean and standard deviation are as expected
                    Assert.IsTrue(d.PopulationMean.ConfidenceInterval(0.99).ClosedContains(nullDistribution.Mean));
                    Assert.IsTrue(d.PopulationStandardDeviation.ConfidenceInterval(0.99).ClosedContains(nullDistribution.StandardDeviation));
                    // This test is actually a bit sensitive, probably because the discrete-ness of the underlying distribution
                    // and the inaccuracy of the asymptotic approximation for intermediate sample size make strict comparisons iffy.
                }
            }

        }


        [TestMethod]
        public void StudentTNullDistributionTest () {

            ContinuousDistribution z = new NormalDistribution(-1.0, 2.0);
            Random rng = new Random(1);

            foreach (int n in TestUtilities.GenerateIntegerValues(2, 32, 4)) {

                Sample tSample = new Sample();
                ContinuousDistribution tDistribution = null;

                for (int j = 0; j < 128; j++) {

                    Sample a = new Sample();
                    Sample b = new Sample();
                    for (int i = 0; i < n; i++) {
                        a.Add(z.GetRandomValue(rng));
                        b.Add(z.GetRandomValue(rng));
                    }

                    TestResult tResult = Sample.StudentTTest(a, b);
                    tSample.Add(tResult.Statistic.Value);
                    tDistribution = tResult.Statistic.Distribution;

                }

                TestResult ks = tSample.KolmogorovSmirnovTest(tDistribution);
                Assert.IsTrue(ks.Probability > 0.01);

                Assert.IsTrue(tSample.PopulationMean.ConfidenceInterval(0.99).ClosedContains(tDistribution.Mean));
                Assert.IsTrue(tSample.PopulationStandardDeviation.ConfidenceInterval(0.99).ClosedContains(tDistribution.StandardDeviation));

            }

        }

        [TestMethod]
        public void PearsonRNullDistribution () {

            Random rng = new Random(1111111);

            // Pick some underlying distributions for the sample variables,
            // which must be normal but can have any parameters.
            NormalDistribution xDistribution = new NormalDistribution(1, 2);
            NormalDistribution yDistribution = new NormalDistribution(3, 4);

            // Try this for several sample sizes, all low so that we see the difference from the normal distribution
            // n = 3 maxima at ends; n = 4 uniform; n = 5 semi-circular "mound"; n = 6 parabolic "mound".
            foreach (int n in new int[] { 3, 4, 5, 6, 8 }) {

                // find r values
                Sample rSample = new Sample();
                ContinuousDistribution rDistribution = null;
                for (int i = 0; i < 128; i++) {

                    // to get each r value, construct a bivariate sample of the given size with no cross-correlation
                    BivariateSample xySample = new BivariateSample();
                    for (int j = 0; j < n; j++) {
                        xySample.Add(
                            xDistribution.GetRandomValue(rng),
                            yDistribution.GetRandomValue(rng)
                        );
                    }
                    TestResult rTest = xySample.PearsonRTest();
                    rSample.Add(rTest.Statistic.Value);
                    rDistribution = rTest.Statistic.Distribution;
                }

                // Check whether r is distributed as expected
                TestResult result = rSample.KuiperTest(new PearsonRDistribution(n));
                Assert.IsTrue(result.Probability > 0.01);

                Assert.IsTrue(rSample.PopulationMean.ConfidenceInterval(0.95).ClosedContains(rDistribution.Mean));
                Assert.IsTrue(rSample.PopulationStandardDeviation.ConfidenceInterval(0.95).ClosedContains(rDistribution.StandardDeviation));

            }


        }

        [TestMethod]
        public void SpearmanNullDistributionTest () {

            // Pick independent distributions for x and y, which needn't be normal and needn't be related.
            ContinuousDistribution xDistrubtion = new UniformDistribution();
            ContinuousDistribution yDistribution = new CauchyDistribution();
            Random rng = new Random(1);

            // Generate bivariate samples of various sizes
            foreach (int n in TestUtilities.GenerateIntegerValues(4, 64, 8)) {
 
                Sample testStatistics = new Sample();
                ContinuousDistribution testDistribution = null;

                for (int i = 0; i < 128; i++) {

                    BivariateSample sample = new BivariateSample();
                    for (int j = 0; j < n; j++) {
                        sample.Add(xDistrubtion.GetRandomValue(rng), yDistribution.GetRandomValue(rng));
                    }

                    TestResult result = sample.SpearmanRhoTest();
                    testStatistics.Add(result.Statistic.Value);
                    testDistribution = result.Statistic.Distribution;
                }

                TestResult r2 = testStatistics.KolmogorovSmirnovTest(testDistribution);
                Assert.IsTrue(r2.Probability > 0.05);

                Assert.IsTrue(testStatistics.PopulationMean.ConfidenceInterval(0.99).ClosedContains(testDistribution.Mean));
                Assert.IsTrue(testStatistics.PopulationVariance.ConfidenceInterval(0.99).ClosedContains(testDistribution.Variance));

            }

        }

        [TestMethod]
        public void KendallNullDistributionTest () {

            // Pick independent distributions for x and y, which needn't be normal and needn't be related.
            ContinuousDistribution xDistrubtion = new LogisticDistribution();
            ContinuousDistribution yDistribution = new ExponentialDistribution();
            Random rng = new Random(314159265);

            // Generate bivariate samples of various sizes
            foreach (int n in TestUtilities.GenerateIntegerValues(8, 64, 4)) {

                Sample testStatistics = new Sample();
                ContinuousDistribution testDistribution = null;

                for (int i = 0; i < 128; i++) {
                    BivariateSample sample = new BivariateSample();
                    for (int j = 0; j < n; j++) {
                        // x and y are uncorrelated
                        sample.Add(xDistrubtion.GetRandomValue(rng), yDistribution.GetRandomValue(rng));
                    }

                    TestResult result = sample.KendallTauTest();
                    testStatistics.Add(result.Statistic.Value);
                    testDistribution = result.Statistic.Distribution;
                }

                // We should find that they are uncorrelated.
                TestResult r2 = testStatistics.KolmogorovSmirnovTest(testDistribution);
                Assert.IsTrue(r2.Probability > 0.01);

                Assert.IsTrue(testStatistics.PopulationMean.ConfidenceInterval(0.99).ClosedContains(testDistribution.Mean));
                Assert.IsTrue(testStatistics.PopulationVariance.ConfidenceInterval(0.99).ClosedContains(testDistribution.Variance));
            }

        }

        [TestMethod]
        public void WilcoxonNullDistribution () {

            // Pick a very non-normal distribution
            ContinuousDistribution d = new ExponentialDistribution();

            Random rng = new Random(271828);
            foreach (int n in TestUtilities.GenerateIntegerValues(4, 64, 4)) {

                Sample wContinuousSample = new Sample();
                ContinuousDistribution wContinuousDistribution = null;

                List<int> wDiscreteSample = new List<int>();
                DiscreteDistribution wDiscreteDistribution = null;

                for (int i = 0; i < 256; i++) {
                    BivariateSample sample = new BivariateSample();
                    for (int j = 0; j < n; j++) {
                        double x = d.GetRandomValue(rng);
                        double y = d.GetRandomValue(rng);
                        sample.Add(x, y);
                    }
                    TestResult wilcoxon = sample.WilcoxonSignedRankTest();
                    if (wilcoxon.UnderlyingStatistic != null) {
                        wDiscreteSample.Add(wilcoxon.UnderlyingStatistic.Value);
                        wDiscreteDistribution = wilcoxon.UnderlyingStatistic.Distribution;
                    } else {
                        wContinuousSample.Add(wilcoxon.Statistic.Value);
                        wContinuousDistribution = wilcoxon.Statistic.Distribution;
                    }
                }

                if (wDiscreteDistribution != null) {
                    TestResult chi2 = wDiscreteSample.ChiSquaredTest(wDiscreteDistribution);
                    Assert.IsTrue(chi2.Probability > 0.01);
                } else {
                    TestResult ks = wContinuousSample.KolmogorovSmirnovTest(wContinuousDistribution);
                    Assert.IsTrue(ks.Probability > 0.01);
                    Assert.IsTrue(wContinuousSample.PopulationMean.ConfidenceInterval(0.99).ClosedContains(wContinuousDistribution.Mean));
                    Assert.IsTrue(wContinuousSample.PopulationStandardDeviation.ConfidenceInterval(0.99).ClosedContains(wContinuousDistribution.StandardDeviation));
                }


            }

        }

        [TestMethod]
        public void ShapiroFranciaNullDistribution () {

            Random rng = new Random(57721);
            foreach (int n in TestUtilities.GenerateIntegerValues(16, 128, 4)) {

                Sample vSample = new Sample();
                ContinuousDistribution vDistribution = null;
                NormalDistribution zDistribution = new NormalDistribution(-2.0, 3.0);
                for (int i = 0; i < 256; i++) {
                    Sample zSample = TestUtilities.CreateSample(zDistribution, n, i);
                    TestResult sf = zSample.ShapiroFranciaTest();
                    vSample.Add(sf.Statistic.Value);
                    vDistribution = sf.Statistic.Distribution;
                }

                TestResult ks = vSample.KolmogorovSmirnovTest(vDistribution);

                Assert.IsTrue(ks.Probability > 0.01);

                // The returned SF null distribution is approximate, so we can't
                // make arbitrarily stringent P demands for arbitrarily large samples.
            }
        }

    }
}
