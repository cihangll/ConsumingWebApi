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
			return await SendAsync<PostResponse>(url, HttpMethodTypes.GET);
		}

		public async Task<ReadOnlyCollection<PostResponse>> GetPostsByUserId(int userId)
		{
			var urlBuilder = new StringBuilder();
			urlBuilder.Append(_posts);
			urlBuilder.Append("?userId=");
			urlBuilder.Append(userId);

			var url = urlBuilder.ToString();
			return await SendAsync<ReadOnlyCollection<PostResponse>>(url, HttpMethodTypes.GET);
		}

		public async Task<ReadOnlyCollection<PostResponse>> GetPostsAsync()
		{
			return await SendAsync<ReadOnlyCollection<PostResponse>>(_posts, HttpMethodTypes.GET);
		}

		public async Task<CommentResponse> GetCommentAsync(int commentId)
		{
			var urlBuilder = new StringBuilder();
			urlBuilder.Append(_comments);
			urlBuilder.Append("/");
			urlBuilder.Append(commentId);

			var url = urlBuilder.ToString();
			return await SendAsync<CommentResponse>(url, HttpMethodTypes.GET);
		}

		public async Task<ReadOnlyCollection<CommentResponse>> GetCommentsAsync()
		{
			return await SendAsync<ReadOnlyCollection<CommentResponse>>(_comments, HttpMethodTypes.GET);
		}

		public async Task<PostResponse> SavePost(PostRequest request)
		{
			if (request == null)
			{
				throw new Exception($"{nameof(request)} cannot be null.");
			}

			return await SendAsync<PostResponse>(_posts, HttpMethodTypes.POST, request);
		}

	}
}
