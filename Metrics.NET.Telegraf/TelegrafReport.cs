namespace Metrics.NET.Telegraf
{
    using InfluxDB.LineProtocol.Payload;
    using Metrics.Reporters;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Telegraf / InfluxDB metrics reporter.
    /// </summary>
    internal class TelegrafReport : BaseReport, IDisposable
    {
        /// <summary>
        /// Global tags that are attached to all reported metrics.
        /// </summary>
        private readonly MetricTags tags;

        /// <summary>
        /// Telegraf address.
        /// </summary>
        private readonly Uri telegrafUri;

        /// <summary>
        /// Metrics namespace.
        /// </summary>
        private readonly string prefix;

        /// <summary>
        /// Metrics sender.
        /// </summary>
        private ISender sender;

        /// <summary>
        /// Metrics payload that should be sent to the backend.
        /// </summary>
        private LineProtocolPayload payload;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelegrafReport"/> class.
        /// </summary>
        /// <param name="uri">Telegraf address.</param>
        /// <param name="prefix">Prefix that will be used as a namespace for all reported metrics. No prefix is added by default.</param>
        /// <param name="tags">Optional list of tags that will be attached to all reported metrics.</param>
        public TelegrafReport(string uri, string prefix = "", string[] tags = null)
	    {
            this.tags = tags ?? MetricTags.None;
            this.telegrafUri = new Uri(uri);
            this.prefix = prefix;

            if (this.telegrafUri.Scheme == Uri.UriSchemeHttp)
            {
                this.sender = new HttpSender(this.telegrafUri);
            }
            else if (this.telegrafUri.Scheme == "udp")
            {
                this.sender = new UdpSender(this.telegrafUri);
            }
            else
            {
                throw new NotSupportedException(string.Format("Protocol '{0}' is not supported", this.telegrafUri.Scheme));
            }
	    }

        /// <summary>
        /// Start reporting a context. Is invoked before reporting metrics under specified context.
        /// </summary>
        /// <param name="contextName">Current context name.</param>
        protected override void StartContext(string contextName)
        {
            this.payload = new LineProtocolPayload();
        }

        /// <summary>
        /// Finish reporting the context. Is invoked after reporting metrics under specified context.
        /// </summary>
        /// <param name="contextName">Current context name.</param>
        protected override void EndContext(string contextName)
        {
            this.sender.Send(this.payload);
        }

        /// <summary>
        /// Reports a counter metric.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Value.</param>
        /// <param name="unit">Unit of measurement.</param>
        /// <param name="tags">Tags.</param>
        protected override void ReportCounter(string name, MetricData.CounterValue value, Unit unit, MetricTags tags)
        {
            if (value.Items.Length == 0)
            {
                AddValue(name, value.Count, tags);
            }
            else
            {
                AddValue(name + ".total", value.Count, tags);
            }

            foreach (var item in value.Items)
            {
                AddValue(name + "." + item.Item, item.Count, tags);
                AddValue(name + "." + item.Item + ".percent", item.Percent, tags);
            }
        }

        /// <summary>
        /// Reports a gauge metric.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Value.</param>
        /// <param name="unit">Unit of measurement.</param>
        /// <param name="tags">Tags.</param>
        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                AddValue(name, value, tags);
            }
        }

        /// <summary>
        /// Reports a histogram.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Value.</param>
        /// <param name="unit">Unit of measurement.</param>
        /// <param name="tags">Tags.</param>
        protected override void ReportHistogram(string name, MetricData.HistogramValue value, Unit unit, MetricTags tags)
        {
            AddValue(name + ".count", value.Count, tags);
            AddValue(name + ".last", value.LastValue, tags);
            AddValue(name + ".min", value.Min, tags);
            AddValue(name + ".avg", value.Mean, tags);
            AddValue(name + ".max", value.Max, tags);
            AddValue(name + ".stdDev", value.StdDev, tags);
            AddValue(name + ".median", value.Median, tags);

            AddValue(name + ".75percentile", value.Percentile75, tags);
            AddValue(name + ".95percentile", value.Percentile95, tags);
            AddValue(name + ".98percentile", value.Percentile98, tags);
            AddValue(name + ".99percentile", value.Percentile99, tags);
            AddValue(name + ".999percentile", value.Percentile999, tags);
        }

        /// <summary>
        /// Reports a meter.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Value.</param>
        /// <param name="unit">Unit of measurement for the count metric.</param>
        /// <param name="rateUnit">Unit of measurement for the rate metrics.</param>
        /// <param name="tags">Tags.</param>
        protected override void ReportMeter(string name, MetricData.MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            AddValue(name + ".count", value.Count, tags);
            AddValue(name + ".avg", value.MeanRate, tags);
            AddValue(name + ".1m", value.OneMinuteRate, tags);
            AddValue(name + ".5m", value.FiveMinuteRate, tags);
            AddValue(name + ".15m", value.FifteenMinuteRate, tags);

            foreach (var item in value.Items)
            {
                AddValue(name + "." + item.Item + ".count", item.Value.Count, tags);
                AddValue(name + "." + item.Item + ".percent", item.Percent, tags);
                AddValue(name + "." + item.Item + ".avg", item.Value.MeanRate, tags);
                AddValue(name + "." + item.Item + ".1m", item.Value.OneMinuteRate, tags);
                AddValue(name + "." + item.Item + ".5m", item.Value.FiveMinuteRate, tags);
                AddValue(name + "." + item.Item + ".15m", item.Value.FifteenMinuteRate, tags);
            }
        }

        /// <summary>
        /// Reports a timer.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Value.</param>
        /// <param name="unit">Unit of measurement.</param>
        /// <param name="rateUnit">Unit of measurement for rate.</param>
        /// <param name="durationUnit">Unit of measurement for the duration.</param>
        /// <param name="tags">Tags.</param>
        protected override void ReportTimer(string name, MetricData.TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            ReportMeter(name + ".rate", value.Rate, unit, rateUnit, tags);
            ReportHistogram(name, value.Histogram, unit, tags);

            AddValue(name + ".sessions", value.ActiveSessions, tags);
            AddValue(name + ".total", value.TotalTime, tags);
        }

        /// <summary>
        /// Does nothing, since Telegraf and Influx have no notion of health checks.
        /// </summary>
        /// <param name="status">Health status.</param>
        protected override void ReportHealth(HealthStatus status)
        {
            // Telegraf and InfluxDB do not support service health checks
        }

        /// <summary>
        /// Adds a metric value to current batch of metrics.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Value.</param>
        /// <param name="metricTags">Tags.</param>
        private void AddValue(string name, object value, MetricTags metricTags)
        {
            String[] tags = GetAllTagsAsArray(metricTags);

            var dataPoint = new LineProtocolPoint(
                this.GetFormattedMetricName(name),
                new Dictionary<string, object>
                {
                    { "value", value },
                },
                this.GetTagsAsDictionary(tags),
                DateTime.UtcNow);
            this.payload.Add(dataPoint);
        }

        /// <summary>
        /// Gets full metic name with namespace attached.
        /// </summary>
        /// <param name="name">Original metric name.</param>
        /// <returns>Full metric name.</returns>
        private string GetFormattedMetricName(string name)
        {
            return string.IsNullOrEmpty(this.prefix)
                ? name
                : prefix + "." + name;
        }

        /// <summary>
        /// Gets all tags that should be attached to a metric as an array.
        /// </summary>
        /// <param name="metricSpecificTags">Metric specific tags.</param>
        /// <returns>An array of tags.</returns>
        private string[] GetAllTagsAsArray(MetricTags metricSpecificTags)
        {
            string[] allTags = new string[this.tags.Tags.Length + metricSpecificTags.Tags.Length];

            // Append global tags to the current tag list
            this.tags.Tags.CopyTo(allTags, 0);
            metricSpecificTags.Tags.CopyTo(allTags, this.tags.Tags.Length);
            return allTags;
        }

        /// <summary>
        /// Parses tags into key/value pairs. Tags are expected (although don't have to)
        /// to follow the format 'key:value'.
        /// </summary>
        /// <param name="tags">An array of tags.</param>
        /// <returns>A dictionary of key/value pairs that represent tags.</returns>
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

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="TelegrafReport"/> class.
        /// </summary>
        public void Dispose()
        {
            if (this.sender is IDisposable)
            {
                ((IDisposable)this.sender).Dispose();
            }
        }
    }
}