using Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Xml;
using Utility;

public class HttpRequest
{
	private Dictionary<string, string> additionsHeaders = new Dictionary<string, string>();

	private int _proxyMode = 1;

	private string _proxyHttpPort = "";

	public event EventHandler<HttpResponseJSONEventArgs> JSONResponsed;

	public event EventHandler<HttpResponseXMLEventArgs> xmlResponsed;

	public HttpRequest()
	{
	}

	public HttpRequest(int proxyMode, string proxyHttpPort)
	{
		_proxyMode = proxyMode;
		_proxyHttpPort = proxyHttpPort;
	}

	private void RequestSetProxy(ref WebRequest webReq)
	{
		try
		{
			switch (_proxyMode)
			{
			case 1:
				break;
			case 0:
				webReq.Proxy = null;
				break;
			case 2:
			{
				WebProxy myProxy = new WebProxy();
				Uri newUri = myProxy.Address = new Uri(_proxyHttpPort);
				webReq.Proxy = myProxy;
				break;
			}
			}
		}
		catch
		{
		}
	}

	private void RequestSetProxy(ref HttpWebRequest webReq)
	{
		try
		{
			switch (_proxyMode)
			{
			case 1:
				break;
			case 0:
				webReq.Proxy = null;
				break;
			case 2:
			{
				WebProxy myProxy = new WebProxy();
				Uri newUri = myProxy.Address = new Uri(_proxyHttpPort);
				webReq.Proxy = myProxy;
				break;
			}
			}
		}
		catch
		{
		}
	}

	public void addAdditionsHeader(string headerField, string headerValue)
	{
		if (headerField == null)
		{
			additionsHeaders = new Dictionary<string, string>();
		}
		additionsHeaders.Add(headerField, headerValue);
	}

	public void setAddtionalHeaders(Dictionary<string, string> headers)
	{
		additionsHeaders = headers;
	}

	public XmlDocument postXMLAndLoadXML(string url, XmlDocument postXmlDoc)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		return postXMLAndLoadXML(url, postXmlString);
	}

	public XmlDocument postXMLAndLoadXML(string url, string postXmlString)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		return postXMLAndLoadXML(url, postXmlString, emptyHeaders);
	}

	public XmlDocument loadXML(string url)
	{
		XmlDocument xmlDoc = new XmlDocument();
		try
		{
			string responseXMLString = getHttpResponseString(url);
			xmlDoc.LoadXml(responseXMLString);
			return xmlDoc;
		}
		catch
		{
			return xmlDoc;
		}
	}

	private bool isValidUrl(string url)
	{
		bool validUri = true;
		try
		{
			new Uri(url);
			return validUri;
		}
		catch (UriFormatException)
		{
			return false;
		}
	}

	public WebResponse getHttpResponse(string url)
	{
		if (!isValidUrl(url))
		{
			return null;
		}
		try
		{
			WebRequest webRequest = WebRequest.Create(url);
			RequestSetProxy(ref webRequest);
			if (additionsHeaders != null)
			{
				foreach (KeyValuePair<string, string> header in additionsHeaders)
				{
					webRequest.Headers.Add(header.Key, header.Value);
				}
			}
			return webRequest.GetResponse();
		}
		catch
		{
		}
		return null;
	}

	public string getHttpResponseString(string url)
	{
		string responseStr = null;
		try
		{
			WebResponse webResp = getHttpResponse(url);
			try
			{
				Stream responseStream = webResp.GetResponseStream();
				Encoding encode = Encoding.GetEncoding("utf-8");
				StreamReader readStream = new StreamReader(responseStream, encode);
				char[] buffer = new char[256];
				int count = readStream.Read(buffer, 0, 256);
				responseStr = "";
				while (count > 0)
				{
					string str = new string(buffer, 0, count);
					responseStr += str;
					count = readStream.Read(buffer, 0, 256);
				}
				return responseStr;
			}
			catch
			{
				return responseStr;
			}
		}
		catch (WebException ex)
		{
			_ = ex.Status;
			_ = 1;
			return responseStr;
		}
	}

	public WebResponse getHttpResponse(string url, Dictionary<string, string> headers)
	{
		if (!isValidUrl(url))
		{
			return null;
		}
		try
		{
			WebRequest webRequest = WebRequest.Create(url);
			RequestSetProxy(ref webRequest);
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (header.Key != null)
				{
					webRequest.Headers.Add(header.Key, header.Value);
				}
			}
			return webRequest.GetResponse();
		}
		catch
		{
		}
		return null;
	}

	public string getHttpResponseString(string url, Dictionary<string, string> headers)
	{
		string responseStr = null;
		try
		{
			WebResponse webResp = getHttpResponse(url);
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (header.Key != null)
				{
					webResp.Headers.Add(header.Key, header.Value);
				}
			}
			try
			{
				Stream responseStream = webResp.GetResponseStream();
				Encoding encode = Encoding.GetEncoding("utf-8");
				StreamReader readStream = new StreamReader(responseStream, encode);
				char[] buffer = new char[256];
				int count = readStream.Read(buffer, 0, 256);
				responseStr = "";
				while (count > 0)
				{
					string str = new string(buffer, 0, count);
					responseStr += str;
					count = readStream.Read(buffer, 0, 256);
				}
				return responseStr;
			}
			catch
			{
				return responseStr;
			}
		}
		catch (WebException ex)
		{
			_ = ex.Status;
			_ = 1;
			return responseStr;
		}
	}

	public string postXMLAndLoadString(string url, XmlDocument postXmlDoc)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		return postXMLAndLoadString(url, postXmlString);
	}

	public string postXMLAndLoadString(string url, string postXmlString)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		return postXMLAndLoadString(url, postXmlString, emptyHeaders);
	}

	public string postXMLAndLoadString(string url, string postXmlString, Dictionary<string, string> headers)
	{
		string xmlString = "";
		int bufferSize = 1024;
		try
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
			RequestSetProxy(ref webRequest);
			webRequest.Method = "POST";
			webRequest.ContentType = "text/xml";
			if (additionsHeaders != null)
			{
				foreach (KeyValuePair<string, string> header2 in additionsHeaders)
				{
					webRequest.Headers.Add(header2.Key, header2.Value);
				}
			}
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (header.Key != null)
				{
					webRequest.Headers.Add(header.Key, header.Value);
				}
			}
			byte[] byteArray = Encoding.UTF8.GetBytes(postXmlString);
			webRequest.ContentLength = byteArray.Length;
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(byteArray, 0, byteArray.Length);
			requestStream.Close();
			try
			{
				Stream responseStream = webRequest.GetResponse().GetResponseStream();
				Encoding encode = Encoding.GetEncoding("UTF-8");
				StreamReader readStream = new StreamReader(responseStream, encode);
				char[] buffer = new char[bufferSize];
				int count = readStream.Read(buffer, 0, bufferSize);
				string responseStr = "";
				while (count > 0)
				{
					string str = new string(buffer, 0, count);
					responseStr += str;
					count = readStream.Read(buffer, 0, bufferSize);
				}
				xmlString = responseStr;
				return xmlString;
			}
			catch (Exception)
			{
				return xmlString;
			}
		}
		catch (Exception)
		{
			return xmlString;
		}
	}

	public string postXMLAndLoadString(string url, XmlDocument postXmlDoc, Dictionary<string, string> headers)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		return postXMLAndLoadString(url, postXmlString, headers);
	}

	public XmlDocument postXMLAndLoadXML(string url, XmlDocument postXmlDoc, Dictionary<string, string> headers)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		return postXMLAndLoadXML(url, postXmlString, headers);
	}

	public XmlDocument postXMLAndLoadXML(string url, string postXmlString, Dictionary<string, string> headers)
	{
		XmlDocument xmlDoc = null;
		int bufferSize = 1024;
		try
		{
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
			RequestSetProxy(ref webRequest);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/xml";
			webRequest.UserAgent = "android";
			if (additionsHeaders != null)
			{
				foreach (KeyValuePair<string, string> header2 in additionsHeaders)
				{
					webRequest.Headers.Add(header2.Key, header2.Value);
				}
			}
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (header.Key != null)
				{
					webRequest.Headers.Add(header.Key, header.Value);
				}
			}
			byte[] byteArray = Encoding.UTF8.GetBytes(postXmlString);
			webRequest.ContentLength = byteArray.Length;
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(byteArray, 0, byteArray.Length);
			requestStream.Close();
			try
			{
				Stream responseStream = webRequest.GetResponse().GetResponseStream();
				Encoding encode = Encoding.GetEncoding("utf-8");
				StreamReader readStream = new StreamReader(responseStream, encode);
				char[] buffer = new char[bufferSize];
				int count = readStream.Read(buffer, 0, bufferSize);
				string responseStr = "";
				while (count > 0)
				{
					string str = new string(buffer, 0, count);
					responseStr += str;
					count = readStream.Read(buffer, 0, bufferSize);
				}
				xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(responseStr);
				return xmlDoc;
			}
			catch (Exception)
			{
				return xmlDoc;
			}
		}
		catch (Exception)
		{
			return xmlDoc;
		}
	}

	public void postXMLAndLoadXMLAsync(string url, XmlDocument postXmlDoc)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		postXMLAndLoadXMLAsync(url, postXmlString);
	}

	public void postXMLAndLoadXMLAsync(string url, XmlDocument postXmlDoc, object[] callBackParams)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		postXMLAndLoadXMLAsync(url, postXmlString, callBackParams);
	}

	public void postXMLAndLoadXMLAsync(string url, string postXmlString)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		postXMLAndLoadXMLAsync(url, postXmlString, emptyHeaders);
	}

	public void postXMLAndLoadXMLAsync(string url, string postXmlString, object[] callBackParams)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		postXMLAndLoadXMLAsync(url, postXmlString, emptyHeaders, callBackParams);
	}

	public void postXMLAndLoadXMLAsync(string url, XmlDocument postXmlDoc, Dictionary<string, string> headers)
	{
		string postXmlString = new XMLTool().xmlDocToString(postXmlDoc, withXMLDeclaration: false);
		postXMLAndLoadXMLAsync(url, postXmlString, headers);
	}

	public void postXMLAndLoadXMLAsync(string url, string postXmlStr, Dictionary<string, string> headers)
	{
		postXMLAndLoadXMLAsync(url, postXmlStr, headers, new object[0]);
	}

	public void postXMLAndLoadXMLAsync(string url, string postXmlStr, Dictionary<string, string> headers, object[] callBackParams)
	{
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		RequestSetProxy(ref request);
		request.ContentType = "application/xml; charset=UTF-8";
		request.Method = "POST";
		request.SendChunked = true;
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				request.Headers.Add(header.Key, header.Value);
			}
		}
		object[] asyncParams = new object[3]
		{
			request,
			callBackParams,
			postXmlStr
		};
		request.BeginGetRequestStream(getRequestStreamCallback, asyncParams);
	}

	private void getRequestStreamCallback(IAsyncResult asynchronousResult)
	{
		object[] asyncParams = (object[])asynchronousResult.AsyncState;
		HttpWebRequest request = (HttpWebRequest)asyncParams[0];
		RequestSetProxy(ref request);
		object[] callBackParams = (object[])asyncParams[1];
		string postXmlStr = (string)asyncParams[2];
		try
		{
			Stream stream = request.EndGetRequestStream(asynchronousResult);
			byte[] byteArray = Encoding.UTF8.GetBytes(postXmlStr);
			stream.Write(byteArray, 0, byteArray.Length);
			stream.Close();
			request.BeginGetResponse(readResponseXML, asyncParams);
		}
		catch
		{
			this.xmlResponsed?.Invoke(this, new HttpResponseXMLEventArgs(callBackParams, null, null, success: false));
		}
	}

	public void readResponseXML(IAsyncResult asyncResult)
	{
		object[] obj = (object[])asyncResult.AsyncState;
		HttpWebRequest request = (HttpWebRequest)obj[0];
		RequestSetProxy(ref request);
		object[] callBackParams = (object[])obj[1];
		try
		{
			using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult))
			{
				WebHeaderCollection headerCollection = response.Headers;
				Dictionary<string, string> headers = new Dictionary<string, string>();
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					string strContent = sr.ReadToEnd();
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(strContent);
					EventHandler<HttpResponseXMLEventArgs> responseResult = this.xmlResponsed;
					string[] allKeys = headerCollection.AllKeys;
					foreach (string keys in allKeys)
					{
						headers.Add(keys, headerCollection[keys]);
					}
					responseResult?.Invoke(this, new HttpResponseXMLEventArgs(callBackParams, xmlDoc, headers, success: true));
				}
			}
		}
		catch (Exception)
		{
			this.xmlResponsed?.Invoke(this, new HttpResponseXMLEventArgs(callBackParams, null, null, success: false));
		}
	}

	public void postJSONAndLoadJSONStringAsync(string url, string postJSONString)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		if (additionsHeaders != null)
		{
			emptyHeaders = additionsHeaders;
		}
		postJSONAndLoadJSONStringAsync(url, postJSONString, emptyHeaders);
	}

	public void postJSONAndLoadJSONStringAsync(string url, string postJSONString, object[] callBackParams)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		if (additionsHeaders != null)
		{
			emptyHeaders = additionsHeaders;
		}
		postJSONAndLoadJSONStringAsync(url, postJSONString, emptyHeaders, callBackParams);
	}

	public void postJSONAndLoadJSONStringAsync(string url, string postJSONStr, Dictionary<string, string> headers)
	{
		postJSONAndLoadJSONStringAsync(url, postJSONStr, headers, new object[0]);
	}

	public void postJSONAndLoadJSONStringAsync(string url, string postJSONStr, Dictionary<string, string> headers, object[] callBackParams)
	{
		Uri uri = new Uri(url);
		ForceCanonicalPathAndQuery(uri);
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
		RequestSetProxy(ref request);
		request.ContentType = "application/json; charset=UTF-8";
		request.Method = "POST";
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				request.Headers.Add(header.Key, header.Value);
			}
		}
		object[] asyncParams = new object[3]
		{
			request,
			callBackParams,
			postJSONStr
		};
		request.BeginGetRequestStream(getJSONRequestStreamCallback, asyncParams);
	}

	private void ForceCanonicalPathAndQuery(Uri uri)
	{
		_ = uri.PathAndQuery;
		FieldInfo field = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
		ulong flags2 = (ulong)field.GetValue(uri);
		flags2 = (ulong)((long)flags2 & -49L);
		field.SetValue(uri, flags2);
	}

	private void getJSONRequestStreamCallback(IAsyncResult asynchronousResult)
	{
		object[] asyncParams = (object[])asynchronousResult.AsyncState;
		HttpWebRequest request = (HttpWebRequest)asyncParams[0];
		RequestSetProxy(ref request);
		object[] callBackParams = (object[])asyncParams[1];
		string postJSONStr = (string)asyncParams[2];
		try
		{
			Stream stream = request.EndGetRequestStream(asynchronousResult);
			byte[] byteArray = Encoding.UTF8.GetBytes(postJSONStr);
			stream.Write(byteArray, 0, byteArray.Length);
			stream.Close();
			request.BeginGetResponse(readResponseJSONString, asyncParams);
		}
		catch
		{
			request.KeepAlive = false;
			request.Abort();
			this.JSONResponsed?.Invoke(this, new HttpResponseJSONEventArgs(callBackParams, null, success: false));
		}
	}

	public void readResponseJSONString(IAsyncResult asyncResult)
	{
		object[] obj = (object[])asyncResult.AsyncState;
		HttpWebRequest request = (HttpWebRequest)obj[0];
		RequestSetProxy(ref request);
		object[] callBackParams = (object[])obj[1];
		try
		{
			using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult))
			{
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					string strContent = sr.ReadToEnd();
					this.JSONResponsed?.Invoke(this, new HttpResponseJSONEventArgs(callBackParams, strContent, success: true));
				}
			}
		}
		catch (Exception)
		{
			this.JSONResponsed?.Invoke(this, new HttpResponseJSONEventArgs(callBackParams, null, success: false));
		}
		finally
		{
			request.KeepAlive = false;
			request.Abort();
		}
	}

	public void getJSONStringAsync(string url, string postJSONString)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		if (additionsHeaders != null)
		{
			emptyHeaders = additionsHeaders;
		}
		getJSONStringAsync(url, postJSONString, emptyHeaders);
	}

	public void getJSONStringAsync(string url, string postJSONString, object[] callBackParams)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		if (additionsHeaders != null)
		{
			emptyHeaders = additionsHeaders;
		}
		getJSONStringAsync(url, postJSONString, emptyHeaders, callBackParams);
	}

	public void getJSONStringAsync(string url, string postJSONStr, Dictionary<string, string> headers)
	{
		getJSONStringAsync(url, postJSONStr, headers, new object[0]);
	}

	public void getJSONStringAsync(string url, string postJSONStr, Dictionary<string, string> headers, object[] callBackParams)
	{
		Uri uri = new Uri(url);
		ForceCanonicalPathAndQuery(uri);
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
		RequestSetProxy(ref request);
		request.ContentType = "application/json; charset=UTF-8";
		request.Method = "GET";
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				request.Headers.Add(header.Key, header.Value);
			}
		}
		try
		{
			using (WebResponse response = request.GetResponse())
			{
				try
				{
					using (Stream content = response.GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(content))
						{
							string strContent = reader.ReadToEnd();
							this.JSONResponsed?.Invoke(this, new HttpResponseJSONEventArgs(callBackParams, strContent, success: true));
						}
					}
				}
				catch (Exception)
				{
					this.JSONResponsed?.Invoke(this, new HttpResponseJSONEventArgs(callBackParams, null, success: false));
				}
			}
		}
		catch
		{
			this.JSONResponsed?.Invoke(this, new HttpResponseJSONEventArgs(callBackParams, null, success: false));
		}
		finally
		{
			request.KeepAlive = false;
			request.Abort();
		}
	}

	public void putJSONAndLoadJSONStringAsync(string url, string postJSONString)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		if (additionsHeaders != null)
		{
			emptyHeaders = additionsHeaders;
		}
		putJSONAndLoadJSONStringAsync(url, postJSONString, emptyHeaders);
	}

	public void putJSONAndLoadJSONStringAsync(string url, string postJSONString, object[] callBackParams)
	{
		Dictionary<string, string> emptyHeaders = new Dictionary<string, string>();
		if (additionsHeaders != null)
		{
			emptyHeaders = additionsHeaders;
		}
		putJSONAndLoadJSONStringAsync(url, postJSONString, emptyHeaders, callBackParams);
	}

	public void putJSONAndLoadJSONStringAsync(string url, string postJSONStr, Dictionary<string, string> headers)
	{
		putJSONAndLoadJSONStringAsync(url, postJSONStr, headers, new object[0]);
	}

	public void putJSONAndLoadJSONStringAsync(string url, string postJSONStr, Dictionary<string, string> headers, object[] callBackParams)
	{
		Uri uri = new Uri(url);
		ForceCanonicalPathAndQuery(uri);
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
		RequestSetProxy(ref request);
		request.ContentType = "application/json; charset=UTF-8";
		request.Method = "PUT";
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				request.Headers.Add(header.Key, header.Value);
			}
		}
		object[] asyncParams = new object[3]
		{
			request,
			callBackParams,
			postJSONStr
		};
		request.BeginGetRequestStream(getJSONRequestStreamCallback, asyncParams);
	}

	public NetworkStatusCode checkNetworkStatus()
	{
		return checkNetworkStatus("");
	}

	public NetworkStatusCode checkNetworkStatus(string serviceEchoUrl)
	{
		if (NetworkInterface.GetIsNetworkAvailable())
		{
			try
			{
				HttpWebRequest google204Request = (HttpWebRequest)WebRequest.Create("http://www.apple.com/library/test/success.html");
				RequestSetProxy(ref google204Request);
				google204Request.Timeout = 10000;
				if (((HttpWebResponse)google204Request.GetResponse()).StatusCode == HttpStatusCode.OK)
				{
					if (serviceEchoUrl.Length > 0)
					{
						try
						{
							if (getHttpResponseString(serviceEchoUrl).Equals("1"))
							{
								return NetworkStatusCode.OK;
							}
							return NetworkStatusCode.SERVICE_NOT_AVAILABLE;
						}
						catch
						{
							return NetworkStatusCode.SERVICE_NOT_AVAILABLE;
						}
					}
					return NetworkStatusCode.OK;
				}
				return NetworkStatusCode.WIFI_NEEDS_LOGIN;
			}
			catch (WebException)
			{
				return NetworkStatusCode.NO_NETWORK;
			}
			catch (Exception)
			{
				return NetworkStatusCode.NO_NETWORK;
			}
		}
		return NetworkStatusCode.NO_NETWORK;
	}

	public bool getServerEcho()
	{
		string response = "";
		NetworkInterface.GetIsNetworkAvailable();
		string serverEchoUrl = "http://test.com.tw/service/systemService.do?action=getData";
		new XmlDocument().LoadXml("<body></body>");
		try
		{
			response = getHttpResponseString(serverEchoUrl);
		}
		catch (Exception)
		{
		}
		if (response == "1")
		{
			return true;
		}
		return false;
	}
}
