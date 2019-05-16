using ClientDemo.Core.Web.Client.Contracts;
using Newtonsoft.Json;

namespace ClientDemo.Application.JsonServerClient.Contracts.Response
{
	public class PostRequest : ContractBase
	{
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		[JsonProperty(PropertyName = "userId")]
		public int UserId { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "body")]
		public string Body { get; set; }

		public PostRequest(int ıd, int userId, string title, string body)
		{
			Id = ıd;
			UserId = userId;
			Title = title;
			Body = body;
		}
	}
}
