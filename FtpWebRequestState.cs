using Network;
using System.Net;

public class FtpWebRequestState : WebRequestState
{
	private FtpWebRequest _request;

	private FtpWebResponse _response;

	public override WebRequest request
	{
		get
		{
			return _request;
		}
		set
		{
			_request = (FtpWebRequest)value;
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
			_response = (FtpWebResponse)value;
		}
	}

	public FtpWebRequestState(int buffSize)
		: base(buffSize)
	{
	}
}
