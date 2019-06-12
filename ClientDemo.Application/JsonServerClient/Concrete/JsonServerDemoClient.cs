using ClientDemo.Application.Abstract;
using ClientDemo.Application.JsonServerClient.Abstract;
using ClientDemo.Application.JsonServerClient.Contracts.Response;
using ClientDemo.Core.Web.Client.Models;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientDemo.Application.JsonServerClient.Concrete
{
	public class JsonServerDemoClient : WebApiClientBase, IJsonServerDemoClient
	{
		private const string _posts = "/posts";
		private const string _comments = "/comments";

		public JsonServerDemoClient() : base() { }

		public JsonServerDemoClient(string baseUrl) : base()
		{
			SetBaseUrl(baseUrl);
		}

		public async Task<PostResponse> GetPostAsync(int postId)
		{
			var urlBuilder = new StringBuilder();
			urlBuilder.Append(_posts);
			urlBuilder.Append("/");
			urlBuilder.Append(postId);

			var url = urlBuilder.ToString();
			var result = await SendAsync(url, HttpMethodTypes.GET);
			return GetDataFromJsonResult<PostResponse>(result);
		}

		public async Task<ReadOnlyCollection<PostResponse>> GetPostsByUserId(int userId)
		{
			var urlBuilder = new StringBuilder();
			urlBuilder.Append(_posts);
			urlBuilder.Append("?userId=");
			urlBuilder.Append(userId);

			var url = urlBuilder.ToString();
			var result = await SendAsync(url, HttpMethodTypes.GET);
			return GetDataFromJsonResult<ReadOnlyCollection<PostResponse>>(result);
		}

		public async Task<ReadOnlyCollection<PostResponse>> GetPostsAsync()
		{
			var result = await SendAsync(_posts, HttpMethodTypes.GET);
			return GetDataFromJsonResult<ReadOnlyCollection<PostResponse>>(result);
		}

		public async Task<CommentResponse> GetCommentAsync(int commentId)
		{
			var urlBuilder = new StringBuilder();
			urlBuilder.Append(_comments);
			urlBuilder.Append("/");
			urlBuilder.Append(commentId);

			var url = urlBuilder.ToString();
			var result = await SendAsync(url, HttpMethodTypes.GET);
			return GetDataFromJsonResult<CommentResponse>(result);
		}

		public async Task<ReadOnlyCollection<CommentResponse>> GetCommentsAsync()
		{
			var result = await SendAsync(_comments, HttpMethodTypes.GET);
			return GetDataFromJsonResult<ReadOnlyCollection<CommentResponse>>(result);
		}

		public async Task<PostResponse> SavePost(PostRequest request)
		{
			if (request == null)
			{
				throw new Exception($"{nameof(request)} cannot be null.");
			}

			var result = await SendAsync(_posts, HttpMethodTypes.POST, request);
			return GetDataFromJsonResult<PostResponse>(result);
		}

	}
}
