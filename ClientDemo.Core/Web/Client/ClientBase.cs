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
			settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
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

		protected async Task<object> CreateClientAndSendAsync(string url, string httpMethodType = HttpMethodTypes.GET, object requestData = null, string contentMediaType = ContentMediaTypes.Json, Func<HttpClient, Task<object>> function = null)
		{
			var client = new HttpClient();
			try
			{
				return await function(client);
			}
			finally
			{
				if (client != null)
				{ client.Dispose(); };
			}
		}
		protected async Task<object> SendAsync(string url, Func<HttpStatusCode, string, string, Task<object>> function, string httpMethodType = HttpMethodTypes.GET, object requestData = null, string contentMediaType = ContentMediaTypes.Json)
		{
			return await CreateClientAndSendAsync(url, httpMethodType, requestData, contentMediaType, async (client) =>
			{
				var response = await SendAsync(client, url, httpMethodType, requestData, contentMediaType);
				if (function != null)
				{
					return await function(response.Item1, response.Item2, response.Item3);
				}
				return response;
			});
		}

		protected async Task<object> SendAsync(string url, Func<HttpStatusCode, string, string, object> function, string httpMethodType = HttpMethodTypes.GET, object requestData = null, string contentMediaType = ContentMediaTypes.Json)
		{
			return await CreateClientAndSendAsync(url, httpMethodType, requestData, contentMediaType, async (client) =>
			{
				var response = await SendAsync(client, url, httpMethodType, requestData, contentMediaType);
				if (function != null)
				{
					return function(response.Item1, response.Item2, response.Item3);
				}
				return response;
			});
		}

		protected async Task<Tuple<HttpStatusCode, string, string>> SendAsync(string url, string httpMethodType = HttpMethodTypes.GET, object requestData = null, string contentMediaType = ContentMediaTypes.Json)
		{
			return (Tuple<HttpStatusCode, string, string>)await CreateClientAndSendAsync(url, httpMethodType, requestData, contentMediaType, async (client) =>
			{
				return await SendAsync(client, url, httpMethodType, requestData, contentMediaType);
			});
		}

		protected async Task<Tuple<HttpStatusCode, string, string>> SendAsync(HttpClient client, string url, string httpMethodType = HttpMethodTypes.GET, object requestData = null, string contentMediaType = ContentMediaTypes.Json)
		{
			using (var request = await CreateHttpRequestMessageAsync().ConfigureAwait(false))
			{
				var urlBuilder = GetAbsoluteUrl(url);
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

					var responseData = await GetResponseData(response.Content);

					#endregion

					#region Process

					try
					{
						var responseBodyMediaType = response.Content?.Headers?.ContentType?.MediaType;
						return Tuple.Create(response.StatusCode, responseBodyMediaType, responseData);
					}
					catch (Exception exception)
					{
						throw new ClientException("Could not deserialize the response body.", (int)response.StatusCode, responseData, headers, exception);
					}

					#endregion
				}
			}
		}

		protected void CreateHttpContent(HttpRequestMessage request, object requestData, string contentMediaType)
		{
			var content = CreateHttpContent(requestData, contentMediaType);
			content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentMediaType);
			request.Content = content;
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

		protected T GetDataFromJsonResult<T>(Tuple<HttpStatusCode, string, string> result)
		{
			if (string.IsNullOrEmpty(result.Item3))
			{
				return default;
			}
			return DeserializeObject<T>(result.Item3);
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

		private void ClearHeadersAccept(HttpClient httpClient)
		{
			httpClient.DefaultRequestHeaders.Accept.Clear();
		}

		private void AddHeadersAccept(HttpClient httpClient, string mediaType = ContentMediaTypes.Json)
		{
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
		}

		private async Task<string> GetResponseData(HttpContent httpContent)
		{
			//NOTO: you have to call before HttpClient disposed.
			return httpContent == null ? null : await httpContent.ReadAsStringAsync().ConfigureAwait(false);
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
