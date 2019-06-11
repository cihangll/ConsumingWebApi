using System;

namespace ClientDemo.Application
{
	public class WebApiLog
	{
		public Guid Id { get; set; }
		public string RequestType { get; set; }
		public string RequestUrl { get; set; }
		public string Headers { get; set; }
		public string RequestBody { get; set; }
		public int ResponseCode { get; set; }
		public string ResponseStatus { get; set; }
		public string ResponseBody { get; set; }
		public DateTime? CreatedOn { get; set; }
	}
}
