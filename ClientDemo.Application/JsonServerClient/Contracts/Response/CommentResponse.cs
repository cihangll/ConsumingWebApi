using ClientDemo.Core.Web.Client.Contracts;
using Newtonsoft.Json;

namespace ClientDemo.Application.JsonServerClient.Contracts.Response
{
	public class CommentResponse : ContractBase
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "postId")]
		public int PostId { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }

		[JsonProperty(PropertyName = "body")]
		public string Body { get; set; }
	}
}
