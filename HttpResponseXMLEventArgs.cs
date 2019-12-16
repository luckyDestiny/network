using System;
using System.Collections.Generic;
using System.Xml;

public class HttpResponseXMLEventArgs : EventArgs
{
	public bool success;

	public XmlDocument responseXML;

	public object[] callBackParams;

	public Dictionary<string, string> responseHeaders;

	public HttpResponseXMLEventArgs(object[] callBackParams, XmlDocument responseXML, Dictionary<string, string> responseHeader, bool success)
	{
		this.callBackParams = callBackParams;
		this.responseXML = responseXML;
		this.success = success;
		responseHeaders = responseHeader;
	}
}
