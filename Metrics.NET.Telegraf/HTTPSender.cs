namespace Metrics.NET.Telegraf
{
    using InfluxDB.LineProtocol.Client;
    using InfluxDB.LineProtocol.Payload;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Sends metrics to the backend via HTTP.
    /// </summary>
    internal class HttpSender : ISender, IDisposable
    {
        /// <summary>
        /// HTTP client.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSender"/> class.
        /// </summary>
        /// <param name="uri">Metrics backend address.</param>
        public HttpSender(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            this.httpClient = new HttpClient { BaseAddress = uri };
        }

        /// <summary>
        /// Sends metrics to the backend.
        /// </summary>
        /// <param name="payload">Metrics payload.</param>
        public void Send(LineProtocolPayload payload)
        {
            LineProtocolWriteResult result = this.WriteAsync(payload).GetAwaiter().GetResult();
            if (!result.Success)
                Console.Error.WriteLine(result.ErrorMessage);
        }

        /// <summary>
        /// Writes metrics to the backend asynchronously.
        /// </summary>
        /// <param name="payload">Metrics payload.</param>
        /// <param name="cancellationToken">Optional operation cancellation token.</param>
        /// <returns><see cref="System.Threading.Tasks.Task"/>, that performs the write operation.</returns>
        private async Task<LineProtocolWriteResult> WriteAsync(LineProtocolPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var payloadText = new StringWriter();
            payload.Format(payloadText);
            var content = new StringContent(payloadText.ToString(), Encoding.UTF8);
            var response = await this.httpClient.PostAsync(string.Empty, content, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return new LineProtocolWriteResult(true, null);

            return new LineProtocolWriteResult(false, string.Format("{0} {1}", response.StatusCode, response.ReasonPhrase));
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="HttpSender"/> class.
        /// </summary>
        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}