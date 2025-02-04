using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace SecureLens.Infrastructure.Strategies
{
    public abstract class BaseHttpStrategy
    {
        private static readonly HttpClient _client = new();

        /// <summary>
        /// Sends an HTTP request and deserializes the JSON response.
        /// </summary>
        /// <typeparam name="T">The expected response type.</typeparam>
        /// <param name="method">HTTP method (GET, POST, etc.).</param>
        /// <param name="url">The request URL.</param>
        /// <param name="headers">Custom headers for the request.</param>
        /// <param name="content">Optional HTTP content (for POST, PUT, etc.).</param>
        /// <returns>The deserialized response of type T.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
        protected static async Task<T> SendRequestAsync<T>(
            HttpMethod method,
            string url,
            Dictionary<string, string> headers,
            HttpContent? content = null)
        {
            using var request = new HttpRequestMessage(method, url);
            request.Content = content;

            // Add custom headers
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await _client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseContent)!;
            }

            throw new HttpRequestException(
                $"Request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Response: {responseContent}");
        }

        /// <summary>
        /// Builds a query string from a dictionary of parameters.
        /// </summary>
        protected static string BuildQueryString(Dictionary<string, string> parameters)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> kv in parameters)
            {
                query[kv.Key] = kv.Value;
            }
            return query.ToString() ?? string.Empty;
        }
    }
}