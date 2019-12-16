using Network;
using System.Net;

public class HttpWebRequestState : WebRequestState
{
	private HttpWebRequest _request;

	private HttpWebResponse _response;

	public override WebRequest request
	{
		get
		{
			return _request;
		}
		set
		{
			_request = (HttpWebRequest)value;
		}
	}

	public override WebResponse response
	{
		get
		{
			return _response;
		}
		set
		{
			_response = (HttpWebResponse)value;
		}
	}

	public HttpWebRequestState(int buffSize)
		: base(buffSize)
	{
	}
}
