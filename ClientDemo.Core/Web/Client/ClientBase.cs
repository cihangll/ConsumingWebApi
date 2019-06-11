using ClientDemo.Core.Json.NewtonsoftJson.Extensions;
using ClientDemo.Core.Web.Client.Contracts.Exception;
using ClientDemo.Core.Web.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo.Core.Web.Client
{
	public abstract class ClientBase
	{

		private Lazy<JsonSerializerSettings> _jsonSerializerSettings;
		protected JsonSerializerSettings JsonSerializerSettings { get { return _jsonSerializerSettings.Value; } }

		protected virtual void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
		{
			settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
		}

		protected virtual void PrepareRequest(HttpClient client, HttpRequestMessage request, string url, Guid uniqueId) { }
		protected virtual void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, Guid uniqueId) { }
		protected virtual void ProcessResponse(HttpClient client, HttpResponseMessage response, Guid uniqueId) { }


		public string Token { get; private set; }
		public string TokenType { get; private set; }

		public void SetToken(string token)
		{
			Token = token;
		}

		public void SetTokenType(string tokenType)
		{
			TokenType = tokenType;
		}

		public string BaseUrl { get; private set; }
		public void SetBaseUrl(string baseUrl)
		{
			BaseUrl = baseUrl;
		}

		protected ClientBase()
		{
			_jsonSerializerSettings = new Lazy<JsonSerializerSettings>(() =>
			{
				var settings = new JsonSerializerSettings();
				UpdateJsonSerializerSettings(settings);
				return settings;
			});
		}

		protected ClientBase(string baseUrl)
		{
			BaseUrl = baseUrl;
			_jsonSerializerSettings = new Lazy<JsonSerializerSettings>(() =>
			{
				var settings = new JsonSerializerSettings();
				UpdateJsonSerializerSettings(settings);
				return settings;
			});
		}

		protected Task<HttpRequestMessage> CreateHttpRequestMessageAsync()
		{
			var msg = new HttpRequestMessage();
			if (!string.IsNullOrEmpty(Token))
			{
				msg.Headers.Authorization = new AuthenticationHeaderValue(string.IsNullOrEmpty(TokenType) ? "Bearer" : TokenType, Token);
			}
			return Task.FromResult(msg);
		}

		protected async Task<T> SendAsync<T>(string url, string httpMethodType = HttpMethodTypes.GET, object requestData = null, string contentMediaType = ContentMediaTypes.Json)
		{
			var urlBuilder = GetAbsoluteUrl(url);

			var client = new HttpClient();
			try
			{
				using (var request = await CreateHttpRequestMessageAsync().ConfigureAwait(false))
				{
					#region Request

					if (requestData != null)
					{
						CreateHttpContent(request, requestData, contentMediaType);
					}

					request.Method = new HttpMethod(httpMethodType);
					request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(ContentMediaTypes.Json));

					var uniqueId = Guid.NewGuid();

					PrepareRequest(client, request, urlBuilder, uniqueId);
					var requestUrl = urlBuilder.ToString();
					request.RequestUri = new Uri(requestUrl, UriKind.RelativeOrAbsolute);
					PrepareRequest(client, request, requestUrl, uniqueId);

					#endregion

					#region Response

					using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
					{
						var headers = GetHeaders(response);

						ProcessResponse(client, response, uniqueId);

						var result = default(T);
						//var statusCode = GetStatusCode(response.StatusCode);
						var responseData = await GetResponseData(response.Content);

						#endregion

						#region Process

						if (typeof(T) == typeof(HttpResponseMessage))
						{
							return (T)Convert.ChangeType(response, typeof(T));
						}

						try
						{
							if (response.IsSuccessStatusCode)
							{
								result = DeserializeObject<T>(responseData);
								return result;
							}
							else if (response.StatusCode == HttpStatusCode.Unauthorized)
							{
								throw new ClientException("Could not authorized.", (int)response.StatusCode, responseData, headers, null);
							}
						}
						catch (Exception exception)
						{
							throw new ClientException("Could not deserialize the response body.", (int)response.StatusCode, responseData, headers, exception);
						}

						if (response.StatusCode != HttpStatusCode.OK)
						{
							throw new ClientException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
						}

						#endregion
					}
				}
			}
			finally
			{
				if (client != null)
				{ client.Dispose(); };
			}


			return default;
		}

		protected void CreateHttpContent(HttpRequestMessage request, object requestData, string contentMediaType)
		{
			var content = CreateHttpContent(requestData, contentMediaType);
			content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentMediaType);
			request.Content = content;
		}

		protected async Task<string> GetResponseData(HttpContent httpContent)
		{
			return httpContent == null ? null : await httpContent.ReadAsStringAsync().ConfigureAwait(false);
		}

		protected string GetStatusCode(HttpStatusCode httpStatusCode)
		{
			return ((int)httpStatusCode).ToString();
		}

		protected Dictionary<string, IEnumerable<string>> GetHeaders(HttpResponseMessage response)
		{
			var headers = System.Linq.Enumerable.ToDictionary(response.Headers, h_ => h_.Key, h_ => h_.Value);
			if (response.Content != null && response.Content.Headers != null)
			{
				foreach (var item_ in response.Content.Headers)
				{
					headers[item_.Key] = item_.Value;
				}
			}
			return headers;
		}

		protected T DeserializeObject<T>(string jsonData)
		{
			if (jsonData == null)
			{ return default; }
			return JsonConvert.DeserializeObject<T>(jsonData);
		}

		protected StringBuilder GetAbsoluteUrl(string url)
		{
			var builder = new StringBuilder();
			if (string.IsNullOrEmpty(BaseUrl))
			{
				builder.Append(url.TrimStart('/'));
			}
			else
			{
				builder.Append(BaseUrl.TrimEnd('/'));
				builder.Append('/');
				builder.Append(url.TrimStart('/'));
			}
			return builder;
		}

		private void AddHeaders(HttpClient httpClient, string name, string value)
		{
			RemoveHeader(httpClient, name);
			httpClient.DefaultRequestHeaders.Add(name, value);
		}

		private void RemoveHeader(HttpClient httpClient, string name)
		{
			httpClient.DefaultRequestHeaders.Remove(name);
		}

		private void RemoveAllHeaders(HttpClient httpClient)
		{
			httpClient.DefaultRequestHeaders.Clear();
		}

		//Update all
		private void ClearHeadersAccept(HttpClient httpClient)
		{
			httpClient.DefaultRequestHeaders.Accept.Clear();
		}

		private void AddHeadersAccept(HttpClient httpClient, string mediaType = ContentMediaTypes.Json)
		{
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
		}

		protected HttpContent CreateHttpContent<T>(T content, string mediaType = ContentMediaTypes.Json)
		{
			if (content == null)
			{
				return null;
			}

			//work here for other types...
			if (mediaType == ContentMediaTypes.UrlEncoded)
			{
				return CreateHttpContentFormUrlEncoded(content);
			}

			var json = JsonConvert.SerializeObject(content, _jsonSerializerSettings.Value);
			return new StringContent(json, Encoding.UTF8, mediaType);
		}

		protected HttpContent CreateHttpContentFormUrlEncoded<T>(T content)
		{
			var keyValueContent = content.ToKeyValue();
			return new FormUrlEncodedContent(keyValueContent);
		}
	}
}
