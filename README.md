## ConsumingWebApi
Consuming Web Api with DotNetCore.

## How To Use

For example you need to consume this api. [https://jsonplaceholder.typicode.com/](https://jsonplaceholder.typicode.com/)

#### ***Create interface***
([IJsonServerDemoClient.cs](./ClientDemo.Application/JsonServerClient/Abstract/IJsonServerDemoClient.cs))
#### ***Create client and implement interface***
([IJsonServerDemoClient.cs](./ClientDemo.Application/JsonServerClient/Concrete/JsonServerDemoClient.cs))

In this example *JsonServerDemoClient* uses *ClientBase* and implements *IJsonServerDemoClient*. 
```csharp
public class JsonServerDemoClient : ClientBase, IJsonServerDemoClient
	{
		public JsonServerDemoClient() : base() { }
		public JsonServerDemoClient(string baseUrl) : base(baseUrl) { }
    ...
  }
```

ClientBase is a abstract class and contains several methods.
([ClientBase.cs](./ClientDemo.Core/Web/Client/ClientBase.cs))

#### Create Contracts

For example, you want to add some post with this api.(https://jsonplaceholder.typicode.com/posts - POST)
Api needs a request body and return same model. 

So, we have to create request model like this.

([PostRequest.cs](./ClientDemo.Application/JsonServerClient/Contracts/Request/PostRequest.cs)) - 
([PostResponse.cs](./ClientDemo.Application/JsonServerClient/Contracts/Response/PostResponse.cs))

```csharp
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
```

#### Create Post Request

You can use like this.

```csharp
public async Task<PostResponse> SavePost(PostRequest request)
{
  if (request == null)
  {
    throw new Exception($"{nameof(request)} cannot be null.");
  }

  return await SendAsync<PostResponse>(_posts, HttpMethodTypes.POST, request);
}
```

Click [here](./ClientDemo.Application/JsonServerClient/Concrete/JsonServerDemoClient.cs) to see more details about this class.

#### Authorization


