using System.Collections.Generic;

namespace ClientDemo.Core.Web.Client.Contracts.Exception
{
	public partial class ClientException : System.Exception
	{
		public int StatusCode { get; private set; }

		public string Response { get; private set; }

		public Dictionary<string, IEnumerable<string>> Headers { get; private set; }

		public ClientException(string message, int statusCode, string response, Dictionary<string, IEnumerable<string>> headers, System.Exception innerException)
			: base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + response.Substring(0, response.Length >= 512 ? 512 : response.Length), innerException)
		{
			StatusCode = statusCode;
			Response = response;
			Headers = headers;
		}

		public override string ToString()
		{
			return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
		}
	}

	public partial class ClientException<TResult> : ClientException
	{
		public TResult Result { get; private set; }

		public ClientException(string message, int statusCode, string response, Dictionary<string, IEnumerable<string>> headers, TResult result, System.Exception innerException)
			: base(message, statusCode, response, headers, innerException)
		{
			Result = result;
		}
	}
}
