using ClientDemo.Application.JsonServerClient.Contracts.Response;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ClientDemo.Application.JsonServerClient.Abstract
{
	public interface IJsonServerDemoClient
	{
		Task<PostResponse> GetPostAsync(int postId);
		Task<ReadOnlyCollection<PostResponse>> GetPostsByUserId(int userId);
		Task<ReadOnlyCollection<PostResponse>> GetPostsAsync();
		Task<CommentResponse> GetCommentAsync(int commentId);
		Task<ReadOnlyCollection<CommentResponse>> GetCommentsAsync();
		Task<PostResponse> SavePost(PostRequest request);
	}

}
