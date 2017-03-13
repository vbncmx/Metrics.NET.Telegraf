namespace Metrics.NET.Telegraf
{
    using InfluxDB.LineProtocol.Payload;

    /// <summary>
    /// Represents a basic metrics sender.
    /// </summary>
    internal interface ISender
    {
        /// <summary>
        /// Sends metrics encoded using line protocol to the backend.
        /// </summary>
        /// <param name="payload">Metrics to be sent.</param>
        void Send(LineProtocolPayload payload);
    }
}
