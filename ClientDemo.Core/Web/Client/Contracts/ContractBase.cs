using Newtonsoft.Json;

namespace ClientDemo.Core.Web.Client.Contracts
{
	public abstract class ContractBase
	{
		public virtual string ToJson()
		{
			return JsonConvert.SerializeObject(
				this,
				Formatting.None,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
		}
	}
}
