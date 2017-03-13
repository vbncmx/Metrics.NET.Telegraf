namespace Metrics.NET.Telegraf
{
    using Metrics.Reports;
    using System;

    /// <summary>
    /// Extensions to <see cref="Metrics.Reports.MetricsReports"/> class.
    /// </summary>
    public static class MetricsReportsExtensions
    {
        /// <summary>
        /// Configures metrics reporting to Telegraf.
        /// </summary>
        /// <param name="reports">An instance of <see cref="Metrics.Reports.MetricsReports"/> class.</param>
        /// <param name="dogStatsDUri">Telegraf address.</param>
        /// <param name="prefix">Optional prefix that will be used as a namespace for all reported metrics. If empty, no prefix will be attached.</param>
        /// <param name="interval">Reporting interval.</param>
        /// <param name="tags">Optional list of tags that will be attached to all reported metrics.</param>
        /// <returns>This.</returns>
        public static MetricsReports WithTelegraf(this MetricsReports reports, string uri, string prefix, TimeSpan interval, String[] tags = null)
        {
            return reports.WithReport(new TelegrafReport(uri, prefix, tags), interval);
        }
    }
}