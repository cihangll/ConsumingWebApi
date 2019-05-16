using ClientDemo.Application.JsonServerClient.Abstract;
using ClientDemo.Application.JsonServerClient.Contracts.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ClientDemo.WebUI.Controllers
{
	public class HomeController : Controller
	{

		private readonly JsonSerializerSettings _jsonSerializerSettings;

		private readonly IJsonServerDemoClient _jsonServerDemoClient;

		public HomeController(IJsonServerDemoClient jsonServerDemoClient)
		{
			_jsonServerDemoClient = jsonServerDemoClient;
			_jsonSerializerSettings = new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented
			};
		}

		[HttpGet("posts/{postId:int}")]
		public async Task<IActionResult> GetPost(int postId)
		{
			return Json(await _jsonServerDemoClient.GetPostAsync(postId), _jsonSerializerSettings);
		}

		[HttpGet("posts")]
		public async Task<IActionResult> GetPosts([FromQuery] int? userId)
		{
			if (userId.HasValue)
			{
				return Json(await _jsonServerDemoClient.GetPostsByUserId(userId.Value), _jsonSerializerSettings);
			}
			else
			{
				return Json(await _jsonServerDemoClient.GetPostsAsync(), _jsonSerializerSettings);
			}
		}

		[HttpGet("comments/{commentId:int}")]
		public async Task<IActionResult> GetComment(int commentId)
		{
			return Json(await _jsonServerDemoClient.GetCommentAsync(commentId), _jsonSerializerSettings);
		}

		[HttpGet("comments")]
		public async Task<IActionResult> GetComments()
		{
			return Json(await _jsonServerDemoClient.GetCommentsAsync(), _jsonSerializerSettings);
		}

		[HttpPost("posts")]
		public async Task<IActionResult> SavePost()
		{
			//for test
			var requestData = new PostRequest(1, 1, "test_title", "test_body");
			return Json(await _jsonServerDemoClient.SavePost(requestData), _jsonSerializerSettings);
		}

	}
}