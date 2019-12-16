using System;

public class HttpResponseJSONEventArgs : EventArgs
{
	public bool success;

	public string jsonString;

	public object[] callBackParams;

	public HttpResponseJSONEventArgs(object[] callBackParams, string jsonString, bool success)
	{
		this.callBackParams = callBackParams;
		this.jsonString = jsonString;
		this.success = success;
	}
}
