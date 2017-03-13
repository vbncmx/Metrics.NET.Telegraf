namespace Metrics.NET.Telegraf
{
    using InfluxDB.LineProtocol.Payload;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// Sends metrics to the backend via UDP.
    /// </summary>
    internal class UdpSender : ISender, IDisposable
    {
        /// <summary>
        /// UDP client.
        /// </summary>
        private readonly UdpClient udpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpSender"/> class.
        /// </summary>
        /// <param name="uri">Metrics backend address.</param>
        public UdpSender(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            this.udpClient = new UdpClient(uri.Host, uri.Port);
        }

        /// <summary>
        /// Sends metrics to the backend.
        /// </summary>
        /// <param name="payload">Metrics payload.</param>
        public void Send(LineProtocolPayload payload)
        {
            var payloadText = new StringWriter();
            payload.Format(payloadText);
            byte[] content = Encoding.UTF8.GetBytes(payloadText.ToString());
            this.udpClient.Send(content, content.Length);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="UdpSender"/> class.
        /// </summary>
        public void Dispose()
        {
            this.udpClient.Close();
        }
    }
}
