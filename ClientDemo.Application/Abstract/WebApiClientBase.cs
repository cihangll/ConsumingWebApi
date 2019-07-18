using ClientDemo.Core.Json.NewtonsoftJson.Extensions;
using ClientDemo.Core.Json.NewtonsoftJson.Helpers;
using ClientDemo.Core.Web.Client;
using ClientDemo.Core.Web.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace ClientDemo.Application.Abstract
{
	public abstract class WebApiClientBase : ClientBase
	{
		private readonly List<string> _blacklist = new List<string> {
			"authorization", "*.authorization",
			"password", "*.password",
			"access_token", "*.access_token",
			"token", "*.token",
			"refresh_token", "*.refresh_token",
			"sessionId", "*.sessionId",
			"tckno", "*.tckno*",
			"phone", "*.phone",
			"last_four", "*.last_four",
			"card_info", "*.card_info"
		};
		private readonly string _mask = "*****";

		protected WebApiClientBase() { }

		private void LogToDatabaseBeforeRun(HttpRequestMessage request, string url, Guid uniqueId)
		{
			var headers = SerializeHeaders(request.Headers);

			var requestBody = request.Content?.ReadAsStringAsync().Result;
			var requestBodyMediaType = request.Content?.Headers?.ContentType?.MediaType;
			if (requestBodyMediaType == ContentMediaTypes.Json)
			{
				requestBody = MaskJsonData(requestBody);
			}
			else if (requestBodyMediaType == ContentMediaTypes.UrlEncoded)
			{
				requestBody = MaskUrlEncodedData(requestBody);
			}

			var maskedRequestUrl = MaskUrlParams(request.RequestUri);
			var webApiLog = new WebApiLog()
			{
				Id = uniqueId,
				Headers = headers,
				RequestBody = requestBody,
				RequestType = request.Method?.Method,
				RequestUrl = maskedRequestUrl
			};

			// work here.
			// You can save database, write to file this class.
			Debug.WriteLine($"LogToDatabaseBeforeRun {JsonConvert.SerializeObject(webApiLog, Formatting.None)}");
		}

		private void LogToDatabaseAfterRun(HttpResponseMessage response, Guid uniqueId)
		{
			//var webApiLog = _webApiLogRepository.Find(x => x.Id == uniqueId);

			// work here.
			// You can save database, write to file this class.
			var webApiLog = new WebApiLog(); //(Mock) Normally you can get it from database.
			if (webApiLog == null)
			{
				return;
			}

			webApiLog.ResponseCode = (int)response.StatusCode;
			webApiLog.ResponseStatus = response.StatusCode.ToString();

			var responseBody = response.Content?.ReadAsStringAsync().Result;
			var responseBodyMediaType = response.Content?.Headers?.ContentType?.MediaType;
			if (responseBodyMediaType == ContentMediaTypes.Json)
			{
				responseBody = MaskJsonData(responseBody);
			}
			else if (responseBodyMediaType == ContentMediaTypes.UrlEncoded)
			{
				responseBody = MaskUrlEncodedData(responseBody);
			}

			webApiLog.ResponseBody = responseBody;

			// Save changes.
			//_unitOfWork.SaveChanges();
			Debug.WriteLine($"LogToDatabaseAfterRun {JsonConvert.SerializeObject(webApiLog, Formatting.None)}");
		}

		protected override void PrepareRequest(HttpClient client, HttpRequestMessage request, string url, Guid uniqueId)
		{
			LogToDatabaseBeforeRun(request, url, uniqueId);
		}

		protected override void ProcessResponse(HttpClient client, HttpResponseMessage response, Guid uniqueId)
		{
			LogToDatabaseAfterRun(response, uniqueId);
		}

		private List<KeyValuePair<string, string>> MaskQueryParamsAndReturn(string query)
		{
			var list = new List<KeyValuePair<string, string>>();
			var @params = HttpUtility.ParseQueryString(query);
			foreach (string key in @params.AllKeys)
			{
				if (_blacklist.Select(x => x.ToLower()).ToList().Contains(key.ToLower()))
				{
					list.Add(new KeyValuePair<string, string>(key, string.Empty));
					continue;
				}
				list.Add(new KeyValuePair<string, string>(key, @params[key]));
			}
			return list;
		}

		private string MaskUrlParams(Uri requestUri)
		{
			if (requestUri == null)
			{
				return string.Empty;
			}

			var list = MaskQueryParamsAndReturn(requestUri.Query);
			return requestUri.GetLeftPart(UriPartial.Path) + "?" + ConvertKeyValueListToFormUrlEncodedContent(list);
		}

		private string MaskJsonData(string jsonData)
		{
			if (string.IsNullOrEmpty(jsonData))
			{
				return string.Empty;
			}
			return JsonHelper.Minify(jsonData.MaskFields(_blacklist, _mask));
		}

		private string MaskUrlEncodedData(string urlEncodedData)
		{
			if (string.IsNullOrEmpty(urlEncodedData))
			{
				return string.Empty;
			}

			var list = MaskQueryParamsAndReturn(urlEncodedData);
			var jsonData = JsonConvert.SerializeObject(list);
			return MaskJsonData(jsonData);
		}

		private string SerializeHeaders(HttpRequestHeaders requestHeaders)
		{
			if (requestHeaders == null)
			{
				return string.Empty;
			}

			var list = requestHeaders.Select(x => new KeyValuePair<string, List<string>>(x.Key, x.Value?.ToList())).ToList();
			if (list.Any(x => x.Key == "Authorization"))
			{
				list.Remove(list.First(x => x.Key == "Authorization"));
				list.Add(new KeyValuePair<string, List<string>>("Authorization", new List<string> { _mask }));
			}

			var jsonString = JsonConvert.SerializeObject(list, new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.None
			});
			return jsonString;
		}
	}
}
