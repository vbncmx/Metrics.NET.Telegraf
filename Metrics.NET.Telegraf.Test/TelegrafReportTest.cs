namespace Metrics.NET.Telegraf.Test
{
    using InfluxDB.LineProtocol.Payload;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using Metrics.MetricData;
    using System.Linq;

    [TestFixture]
    public class TelegrafReportTest
    {
        [Test]
        public void createReporter_httpScheme_usedHttpSender()
        {
            // Arrange
            TelegrafReport telegrafReport;

            // Act
            telegrafReport = new TelegrafReport("http://localhost:1234");

            // Assert
            Assert.IsInstanceOf<HttpSender>(GetSender(telegrafReport));
        }

        [Test]
        public void createReporter_udpScheme_usedUdpSender()
        {
            // Arrange
            TelegrafReport telegrafReport;

            // Act
            telegrafReport = new TelegrafReport("udp://localhost:1234");

            // Assert
            Assert.IsInstanceOf<UdpSender>(GetSender(telegrafReport));
        }

        [Test]
        public void createReporter_unsupportedScheme_threwException()
        {
            // Arrange
            TelegrafReport telegrafReport;

            Assert.That(
                // Act
                () => telegrafReport = new TelegrafReport("ftp://localhost:1234"), 
                // Assert
                Throws.Exception.TypeOf<NotSupportedException>()
                .With.Message.EqualTo("Protocol 'ftp' is not supported"));
        }

        [Test]
        public void reportCounter_simpleCounter_addedCounter()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportCounter", "testcounter", counter(1), Unit.Items, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(1, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testcounter", 1L, "")));
        }

        [Test]
        public void reportCounter_counterWithSetItems_addedSetItems()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportCounter", "testcounter", counter(1, new CounterValue.SetItem[] {
                new CounterValue.SetItem("testitem", 1, 100.0)
            }), Unit.Items, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(3, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testcounter.total", 1L, "")));
            Assert.IsTrue(PointsEqual(points[1], GetDataPoint("testcounter.testitem", 1L, "")));
            Assert.IsTrue(PointsEqual(points[2], GetDataPoint("testcounter.testitem.percent", 100.0, "")));
        }

        [Test]
        public void reportGauge_simpleGauge_addedGauge()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportGauge", "testgauge", gauge(1.0), Unit.Items, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(1, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testgauge", 1.0, "")));
        }

        [Test]
        public void reportMeter_simpleMeter_addedAllStats()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportMeter", "testmeter", meter(500, 1.0, 60.0, 300.0, 900.0), Unit.Items, TimeUnit.Seconds, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(5, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testmeter.count", 500L, "")));
            Assert.IsTrue(PointsEqual(points[1], GetDataPoint("testmeter.avg", 1.0, "")));
            Assert.IsTrue(PointsEqual(points[2], GetDataPoint("testmeter.1m", 60.0, "")));
            Assert.IsTrue(PointsEqual(points[3], GetDataPoint("testmeter.5m", 300.0, "")));
            Assert.IsTrue(PointsEqual(points[4], GetDataPoint("testmeter.15m", 900.0, "")));
        }

        [Test]
        public void reportMeter_meterWithSetItems_addedSetItems()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportMeter", "testmeter", meter(500, 1.0, 60.0, 300.0, 900.0, new MeterValue.SetItem[] {
                new MeterValue.SetItem("testitem", 60.0, meter(300, 1.0, 60.0, 300.0, 900.0))
            }), Unit.Items, TimeUnit.Seconds, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(11, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testmeter.count", 500L, "")));
            Assert.IsTrue(PointsEqual(points[1], GetDataPoint("testmeter.avg", 1.0, "")));
            Assert.IsTrue(PointsEqual(points[2], GetDataPoint("testmeter.1m", 60.0, "")));
            Assert.IsTrue(PointsEqual(points[3], GetDataPoint("testmeter.5m", 300.0, "")));
            Assert.IsTrue(PointsEqual(points[4], GetDataPoint("testmeter.15m", 900.0, "")));
            Assert.IsTrue(PointsEqual(points[5], GetDataPoint("testmeter.testitem.count", 300L, "")));
            Assert.IsTrue(PointsEqual(points[6], GetDataPoint("testmeter.testitem.percent", 60.0, "")));
            Assert.IsTrue(PointsEqual(points[7], GetDataPoint("testmeter.testitem.avg", 1.0, "")));
            Assert.IsTrue(PointsEqual(points[8], GetDataPoint("testmeter.testitem.1m", 60.0, "")));
            Assert.IsTrue(PointsEqual(points[9], GetDataPoint("testmeter.testitem.5m", 300.0, "")));
            Assert.IsTrue(PointsEqual(points[10], GetDataPoint("testmeter.testitem.15m", 900.0, "")));
        }

        [Test]
        public void reportHistogram_simpleHistogram_addedAllStats()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportHistogram", "testhistogram", histogram(1000, 1.0, "", 2.0, "", 1.1, 0.1, "", 0.2, 1.1, 1.3, 1.8, 1.9, 2.0, 2.0), Unit.Items, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(12, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testhistogram.count", 1000L, "")));
            Assert.IsTrue(PointsEqual(points[1], GetDataPoint("testhistogram.last", 1.0, "")));
            Assert.IsTrue(PointsEqual(points[2], GetDataPoint("testhistogram.min", 0.1, "")));
            Assert.IsTrue(PointsEqual(points[3], GetDataPoint("testhistogram.avg", 1.1, "")));
            Assert.IsTrue(PointsEqual(points[4], GetDataPoint("testhistogram.max", 2.0, "")));
            Assert.IsTrue(PointsEqual(points[5], GetDataPoint("testhistogram.stdDev", 0.2, "")));
            Assert.IsTrue(PointsEqual(points[6], GetDataPoint("testhistogram.median", 1.1, "")));
            Assert.IsTrue(PointsEqual(points[7], GetDataPoint("testhistogram.75percentile", 1.3, "")));
            Assert.IsTrue(PointsEqual(points[8], GetDataPoint("testhistogram.95percentile", 1.8, "")));
            Assert.IsTrue(PointsEqual(points[9], GetDataPoint("testhistogram.98percentile", 1.9, "")));
            Assert.IsTrue(PointsEqual(points[10], GetDataPoint("testhistogram.99percentile", 2.0, "")));
            Assert.IsTrue(PointsEqual(points[11], GetDataPoint("testhistogram.999percentile", 2.0, "")));
        }

        [Test]
        public void reportTimer_simpleTimer_addedAllStats()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");
            TimerValue timer = this.timer(meter(1000, 1.0, 60.0, 300.0, 900.0),
                histogram(1000, 1.0, "", 2.0, "", 1.1, 0.1, "", 0.2, 1.1, 1.3, 1.8, 1.9, 2.0, 2.0),
                5,
                1100);

            // Act
            telegrafReport.call("ReportTimer", "testtimer", timer, Unit.Items, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(19, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testtimer.rate.count", 1000L, "")));
            Assert.IsTrue(PointsEqual(points[1], GetDataPoint("testtimer.rate.avg", 1.0, "")));
            Assert.IsTrue(PointsEqual(points[2], GetDataPoint("testtimer.rate.1m", 60.0, "")));
            Assert.IsTrue(PointsEqual(points[3], GetDataPoint("testtimer.rate.5m", 300.0, "")));
            Assert.IsTrue(PointsEqual(points[4], GetDataPoint("testtimer.rate.15m", 900.0, "")));
            Assert.IsTrue(PointsEqual(points[5], GetDataPoint("testtimer.count", 1000L, "")));
            Assert.IsTrue(PointsEqual(points[6], GetDataPoint("testtimer.last", 1.0, "")));
            Assert.IsTrue(PointsEqual(points[7], GetDataPoint("testtimer.min", 0.1, "")));
            Assert.IsTrue(PointsEqual(points[8], GetDataPoint("testtimer.avg", 1.1, "")));
            Assert.IsTrue(PointsEqual(points[9], GetDataPoint("testtimer.max", 2.0, "")));
            Assert.IsTrue(PointsEqual(points[10], GetDataPoint("testtimer.stdDev", 0.2, "")));
            Assert.IsTrue(PointsEqual(points[11], GetDataPoint("testtimer.median", 1.1, "")));
            Assert.IsTrue(PointsEqual(points[12], GetDataPoint("testtimer.75percentile", 1.3, "")));
            Assert.IsTrue(PointsEqual(points[13], GetDataPoint("testtimer.95percentile", 1.8, "")));
            Assert.IsTrue(PointsEqual(points[14], GetDataPoint("testtimer.98percentile", 1.9, "")));
            Assert.IsTrue(PointsEqual(points[15], GetDataPoint("testtimer.99percentile", 2.0, "")));
            Assert.IsTrue(PointsEqual(points[16], GetDataPoint("testtimer.999percentile", 2.0, "")));
            Assert.IsTrue(PointsEqual(points[17], GetDataPoint("testtimer.sessions", 5L, "")));
            Assert.IsTrue(PointsEqual(points[18], GetDataPoint("testtimer.total", 1100L, "")));
        }

        [Test]
        public void reportGauge_gaugeWithTags_attachedTags()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportGauge", "testgauge", 1.0, Unit.Items, new MetricTags(new string[] { "testKey:testValue" }));

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(1, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testgauge", 1.0, "testKey:testValue")));
        }

        [Test]
        public void reportGauge_reporterWithGlobalTags_attachedGlobalTags()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234", "", new string[] { "testKey:testValue" });
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportGauge", "testgauge", 1.0, Unit.Items, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(1, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("testgauge", 1.0, "testKey:testValue")));
        }

        [Test]
        public void reportGauge_reporterWithPrefix_attachedPrefix()
        {
            // Arrange
            TelegrafReport telegrafReport = new TelegrafReport("udp://localhost:1234", "app");
            telegrafReport.call("StartContext", "");

            // Act
            telegrafReport.call("ReportGauge", "testgauge", 1.0, Unit.Items, new MetricTags());

            // Assert
            List<LineProtocolPoint> points = GetDataPoints(telegrafReport);

            Assert.AreEqual(1, points.Count);
            Assert.IsTrue(PointsEqual(points[0], GetDataPoint("app.testgauge", 1.0, "")));
        }

        private bool PointsEqual(LineProtocolPoint expected, LineProtocolPoint actual)
        {
            return actual.Measurement == expected.Measurement &&
                actual.Fields.OrderBy(item => item.Key).SequenceEqual(expected.Fields.OrderBy(item => item.Key)) &&
                actual.Tags.OrderBy(item => item.Key).SequenceEqual(expected.Tags.OrderBy(item => item.Key));
        }

        private LineProtocolPoint GetDataPoint(string name, object value, string tags)
        {
            return new LineProtocolPoint(name,
                new Dictionary<string, object>
                {
                    {"value", value}
                },
                GetTagsAsDictionary(string.IsNullOrEmpty(tags) ? new string[] { } : tags.Split(',')));
        }

        private IReadOnlyDictionary<string, string> GetTagsAsDictionary(string[] tags)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            for (int i = 0; i < tags.Length; i++) {
                string[] keyValue = tags[i].Split(new char[] { ':' }, 2);
                dictionary.Add(keyValue[0], keyValue.Length > 1
                    ? keyValue[1]
                    : null);
            }

            return dictionary;
        }

        private ISender GetSender(TelegrafReport telegrafReport)
        {
            var senderField = telegrafReport.GetType().GetField("sender", System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);

            return senderField.GetValue(telegrafReport) as ISender;
        }

        private List<LineProtocolPoint> GetDataPoints(TelegrafReport telegrafReport)
        {
            var payloadField = telegrafReport.GetType().GetField("payload", System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);

            LineProtocolPayload payload = (LineProtocolPayload)payloadField.GetValue(telegrafReport);

            var pointsField = payload.GetType().GetField("_points", System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);

            return pointsField.GetValue(payload) as List<LineProtocolPoint>;
        }

        private CounterValue counter(long count, CounterValue.SetItem[] setItems = null)
        {
            return new CounterValue(count, setItems ?? new CounterValue.SetItem[] { });
        }

        private double gauge(double value)
        {
            return value;
        }

        private MeterValue meter(long count, double meanRate, double oneMinuteRate, double fiveMinuteRate, double fifteenMinuteRate, MeterValue.SetItem[] setItems = null)
        {
            return new MeterValue(count, meanRate, oneMinuteRate, fiveMinuteRate, fifteenMinuteRate, TimeUnit.Seconds, setItems ?? new MeterValue.SetItem[] { });
        }

        private HistogramValue histogram(long count,
            double lastValue,
            string lastUserValue,
            double max,
            string maxUserValue,
            double mean,
            double min,
            string minUserValue,
            double stdDev,
            double median,
            double percentile75,
            double percentile95,
            double percentile98,
            double percentile99,
            double percentile999)
        {
            return new HistogramValue(count, lastValue, lastUserValue, max, maxUserValue, mean, min, minUserValue, stdDev, median, percentile75, percentile95, percentile98, percentile99, percentile999, 1000);
        }

        private TimerValue timer(MeterValue rate, HistogramValue distribution, long activeSessions, long totalTime)
        {
            return new TimerValue(rate, distribution, activeSessions, totalTime, TimeUnit.Seconds);
        }
    }
}