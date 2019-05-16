using ClientDemo.Core.Web.Client.Contracts;
using Newtonsoft.Json;

namespace ClientDemo.Application.JsonServerClient.Contracts.Response
{
	public class PostResponse : ContractBase
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "userId")]
		public int UserId { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "body")]
		public string Body { get; set; }
	}
}
