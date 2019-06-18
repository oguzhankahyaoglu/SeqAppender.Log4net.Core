using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using log4net.Appender;
using log4net.Core;
using Microsoft.AspNetCore.Hosting;

namespace SeqAppender.Log4net.Core
{
   /// <summary>
    /// A log4net <see cref="T:log4net.Appender.IAppender" /> that writes events synchronously over
    /// HTTP to the Seq event server.
    /// </summary>
    public class SeqAppender : BufferingAppenderSkeleton
    {
        private readonly IHostingEnvironment _environment;
        private readonly HttpClient _httpClient = new HttpClient();

        public SeqAppender(IHostingEnvironment environment)
        {
            _environment = environment;
        }
        

        /// <summary>
        /// 
        /// </summary>
        protected List<AdoNetAppenderParameter> m_parameters = new List<AdoNetAppenderParameter>();

        private const string BulkUploadResource = "api/events/raw";
        private const string ApiKeyHeaderName = "X-Seq-ApiKey";

        /// <summary>
        /// The address of the Seq server to write to. Specified in configuration
        /// like &lt;serverUrl value="http://my-seq:5341" /&gt;.
        /// </summary>
        public string ServerUrl
        {
            get
            {
                if (_httpClient.BaseAddress != null)
                    return _httpClient.BaseAddress.OriginalString;
                return null;
            }
            set
            {
                if (!value.EndsWith("/"))
                    value += "/";
                _httpClient.BaseAddress = new Uri(value);
            }
        }

        /// <summary>
        /// A Seq <i>API key</i> that authenticates the client to the Seq server. Specified in configuration
        /// like &lt;apiKey value="A1A2A3A4A5A6A7A8A9A0" /&gt;.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets HttpClient timeout.
        /// Specified in configuration like &lt;timeout value="00:00:01" /&gt; which coresponds to 1 second.
        /// </summary>
        public string Timeout
        {
            get { return _httpClient.Timeout.ToString(); }
            set { _httpClient.Timeout = TimeSpan.Parse(value); }
        }

        /// <summary>Adds a parameter to the command.</summary>
        /// <param name="parameter">The parameter to add to the command.</param>
        /// <remarks>
        /// <para>
        /// Adds a parameter to the ordered list of command parameters.
        /// </para>
        /// </remarks>
        public void AddParameter(AdoNetAppenderParameter parameter)
        {
            m_parameters.Add(parameter);
        }

        /// <summary>Send events to Seq.</summary>
        /// <param name="events">The buffered events to send.</param>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            if (ServerUrl == null)
                return;
            
            foreach (var e in events)
            {
                e.Properties["Environment"] = _environment.EnvironmentName;
            }

            var payload = new StringWriter();
            payload.Write("{\"events\":[");
            LoggingEventFormatter.ToJson(events, payload, m_parameters);
            payload.Write("]}");
            var stringContent = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            if (!string.IsNullOrWhiteSpace(ApiKey))
                stringContent.Headers.Add("X-Seq-ApiKey", ApiKey);
            using (var result =
                _httpClient.PostAsync("api/events/raw", stringContent).Result)
            {
                if (result.IsSuccessStatusCode)
                    return;
                ErrorHandler.Error($"Received failed result {result.StatusCode}: {result.Content.ReadAsStringAsync().Result}");
            }
        }
    }
}