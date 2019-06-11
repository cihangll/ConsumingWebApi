## ConsumingWebApi
Consuming Web Api with DotNetCore.

## How To Use

For example you need to consume this api. [https://jsonplaceholder.typicode.com/](https://jsonplaceholder.typicode.com/)

#### ***Create interface***
([IJsonServerDemoClient.cs](./ClientDemo.Application/JsonServerClient/Abstract/IJsonServerDemoClient.cs))
#### ***Create client and implement interface***
([JsonServerDemoClient.cs](./ClientDemo.Application/JsonServerClient/Concrete/JsonServerDemoClient.cs))

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

*ClientBase* include `CreateHttpRequestMessageAsync()` method for authorization.If Token value isn't null or not empty, this method automatically add authorization with token and token type to the header.

```csharp
protected Task<HttpRequestMessage> CreateHttpRequestMessageAsync()
{
	var msg = new HttpRequestMessage();
	if (!string.IsNullOrEmpty(Token))
	{
		msg.Headers.Authorization = new AuthenticationHeaderValue(string.IsNullOrEmpty(TokenType) ? "Bearer" : TokenType, Token);
	}
	return Task.FromResult(msg);
}
```
You can set Token and TokenType value with `SetToken()` and `SetTokenType()` methods.
```csharp
public void SetToken(string token)
{
	Token = token;
}

public void SetTokenType(string tokenType)
{
	TokenType = tokenType;
}

```

If you need customization before the request, you can override `PrepareRequest(..)` method.

```csharp
protected virtual void PrepareRequest(HttpClient client, HttpRequestMessage request, string url) { }
protected virtual void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder) { }
```

This method is called before the request.You can find example abstract class [here](./ClientDemo.Application/Abstract/WebApiClientBase.cs).

