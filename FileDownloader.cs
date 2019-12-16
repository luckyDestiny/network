using Network;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

public class FileDownloader : IDisposable
{
	private static Logger logger = LogManager.GetCurrentClassLogger();

	private Timer timeoutTimer;

	private const int BUFFER_SIZE = 1448;

	private Dictionary<string, string> additionalHeaders = new Dictionary<string, string>();

	private string resourceUri;

	public readonly string destinationPath;

	private string tempFilePath;

	private string filename;

	private string postXMLString;

	private long totalBytesToReceive;

	private double downloadedPercent;

	private string requestMethod = "GET";

	private int downloadStartOffset;

	private int retryCount;

	private bool failed;

	private FileStream destinationFileStream;

	public bool resumeDownload = true;

	private HttpWebRequest webRequest;

	private bool isInterrupt;

	private int _proxyMode = 1;

	private string _proxyHttpPort = "";

	public event EventHandler<FileDownloaderProgressChangedEventArgs> downloadProgressChanged;

	public event EventHandler<FileDownloaderStateChangedEventArgs> downloadStateChanged;

	public FileDownloader(string resourceUri, string destinationPath)
	{
		this.destinationPath = destinationPath;
		initializeAttributes(resourceUri, destinationPath, "");
	}

	public void setProxyPara(int proxyMode, string proxyHttpPort)
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

	public void Dispose()
	{
		try
		{
			timeoutTimer.Change(0, 0);
			resumeDownload = false;
			timeoutTimer.Dispose();
			abortHttpWebRequestAndFileStream();
		}
		catch
		{
		}
	}

	public FileDownloader(string resourceUri, string destinationPath, string postXMLString)
	{
		this.destinationPath = destinationPath;
		initializeAttributes(resourceUri, destinationPath, postXMLString);
	}

	private void TimerProc(object state)
	{
		if (new HttpRequest(_proxyMode, _proxyHttpPort).checkNetworkStatus() != 0)
		{
			retryCount++;
			if (retryCount > 5)
			{
				triggerDownloaderStateEvent(FileDownloaderState.NOCONTENT);
			}
			else
			{
				try
				{
					timeoutTimer.Change(10000, 0);
				}
				catch
				{
				}
			}
		}
		else
		{
			retryCount = 0;
			if (resumeDownload)
			{
				try
				{
					timeoutTimer.Change(10000, 0);
				}
				catch
				{
				}
			}
		}
	}

	private void initializeAttributes(string resourceUri, string destinationPath, string postXMLString)
	{
		this.resourceUri = resourceUri;
		tempFilePath = destinationPath + ".tmp";
		this.postXMLString = postXMLString;
		if (postXMLString.Length > 0)
		{
			requestMethod = "POST";
		}
		try
		{
			filename = Path.GetFileName(destinationPath);
		}
		catch (Exception ex)
		{
			logger.Debug("exception occured while getting file from destinationPath: " + destinationPath);
			logger.Debug(ex.ToString);
			failed = true;
		}
	}

	public void addAdditionalHeader(string headerField, string headerValue)
	{
		if (!additionalHeaders.ContainsKey(headerField))
		{
			additionalHeaders.Add(headerField, headerValue);
		}
		else
		{
			additionalHeaders[headerField] = headerValue;
		}
	}

	public void startDownload()
	{
		if (failed)
		{
			abortHttpWebRequestAndFileStream();
			triggerDownloaderStateEvent(FileDownloaderState.FAILED);
			return;
		}
		Thread thread = new Thread(startDownloadRequest);
		thread.Name = filename;
		thread.Start();
		timeoutTimer = new Timer(TimerProc);
	}

	private void startDownloadRequest()
	{
		WebRequestState reqState = null;
		try
		{
			if (File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
			webRequest = (HttpWebRequest)WebRequest.Create(new Uri(resourceUri));
			webRequest.Timeout = 200000;
			webRequest.Method = requestMethod;
			foreach (KeyValuePair<string, string> kvp in additionalHeaders)
			{
				webRequest.Headers.Add(kvp.Key, kvp.Value);
			}
			if (postXMLString.Length > 0)
			{
				webRequest.ContentType = "application/xml";
				byte[] byteArray = Encoding.UTF8.GetBytes(postXMLString);
				webRequest.ContentLength = byteArray.Length;
				using (Stream dataStream = webRequest.GetRequestStream())
				{
					if (dataStream == null)
					{
						return;
					}
					dataStream.Write(byteArray, 0, byteArray.Length);
					dataStream.Close();
				}
			}
			reqState = new HttpWebRequestState(1448);
			reqState.bytesRead = downloadStartOffset;
			reqState.request = webRequest;
			if (webRequest != null)
			{
				reqState.fileURI = new Uri(resourceUri);
				reqState.transferStart = DateTime.Now;
				triggerDownloaderStateEvent(FileDownloaderState.INITIALIZED);
				if (isInterrupt)
				{
					if (reqState != null && reqState.response != null)
					{
						reqState.response.Close();
					}
					abortHttpWebRequestAndFileStream();
					triggerDownloaderStateEvent(FileDownloaderState.FAILED);
				}
				else if (reqState != null)
				{
					webRequest.BeginGetResponse(RespCallback, reqState);
				}
			}
		}
		catch (Exception)
		{
			if (reqState != null && reqState.response != null)
			{
				reqState.response.Close();
			}
			abortHttpWebRequestAndFileStream();
			triggerDownloaderStateEvent(FileDownloaderState.FAILED);
		}
	}

	private void RespCallback(IAsyncResult asyncResult)
	{
		WebRequestState reqState = (WebRequestState)asyncResult.AsyncState;
		WebRequest req = reqState.request;
		Stream responseStream = null;
		try
		{
			HttpWebResponse resp = (HttpWebResponse)(reqState.response = (HttpWebResponse)req.EndGetResponse(asyncResult));
			if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.PartialContent)
			{
				triggerDownloaderStateEvent(FileDownloaderState.DOWNLOADING);
				_ = resp.StatusDescription;
				reqState.totalBytes = reqState.response.ContentLength + downloadStartOffset;
				totalBytesToReceive = reqState.response.ContentLength + downloadStartOffset;
				responseStream = reqState.response.GetResponseStream();
				try
				{
					destinationFileStream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write);
					if (downloadStartOffset > 0)
					{
						destinationFileStream.Seek(downloadStartOffset, SeekOrigin.Begin);
					}
					reqState.streamResponse = responseStream;
				}
				catch (Exception)
				{
					responseStream?.Close();
					if (reqState != null && reqState.response != null)
					{
						reqState.response.Close();
					}
					abortHttpWebRequestAndFileStream();
					triggerDownloaderStateEvent(FileDownloaderState.FAILED);
					return;
				}
				if (isInterrupt)
				{
					if (reqState != null && reqState.response != null)
					{
						reqState.response.Close();
					}
					abortHttpWebRequestAndFileStream();
					triggerDownloaderStateEvent(FileDownloaderState.FAILED);
				}
				else if (reqState != null)
				{
					responseStream.BeginRead(reqState.bufferRead, 0, 1448, ReadCallback, reqState);
				}
			}
			else
			{
				responseStream?.Close();
				if (reqState != null && reqState.response != null)
				{
					reqState.response.Close();
				}
				abortHttpWebRequestAndFileStream();
				triggerDownloaderStateEvent(FileDownloaderState.NOCONTENT);
			}
		}
		catch (WebException)
		{
			responseStream?.Close();
			if (reqState != null && reqState.response != null)
			{
				reqState.response.Close();
			}
			abortHttpWebRequestAndFileStream();
			triggerDownloaderStateEvent(FileDownloaderState.FAILED);
		}
		catch (Exception)
		{
			responseStream?.Close();
			if (reqState != null && reqState.response != null)
			{
				reqState.response.Close();
			}
			abortHttpWebRequestAndFileStream();
			triggerDownloaderStateEvent(FileDownloaderState.FAILED);
		}
	}

	private void ReadCallback(IAsyncResult asyncResult)
	{
		WebRequestState reqState = (WebRequestState)asyncResult.AsyncState;
		Stream responseStream = reqState.streamResponse;
		try
		{
			if (isInterrupt)
			{
				if (reqState != null && reqState.response != null)
				{
					reqState.response.Close();
				}
				abortHttpWebRequestAndFileStream();
				triggerDownloaderStateEvent(FileDownloaderState.FAILED);
			}
			else
			{
				_ = new byte[1448];
				downloadedPercent = (double)reqState.bytesRead / (double)reqState.totalBytes * 100.0;
				int bytesRead = responseStream.EndRead(asyncResult);
				if (bytesRead > 0 && !downloadedPercent.Equals(100.0))
				{
					retryCount = 0;
					reqState.bytesRead += bytesRead;
					downloadedPercent = (double)reqState.bytesRead / (double)reqState.totalBytes * 100.0;
					TimeSpan totalTime = DateTime.Now - reqState.transferStart;
					_ = (double)((float)reqState.bytesRead * 1000f) / (totalTime.TotalMilliseconds * 1024.0);
					destinationFileStream.Write(reqState.bufferRead, 0, bytesRead);
					this.downloadProgressChanged?.Invoke(this, new FileDownloaderProgressChangedEventArgs(destinationPath, filename, downloadedPercent));
					if (downloadedPercent.Equals(100.0))
					{
						responseStream.Close();
						reqState.response.Close();
						abortHttpWebRequestAndFileStream();
						if (File.Exists(destinationPath))
						{
							File.Delete(destinationPath);
						}
						File.Move(tempFilePath, destinationPath);
						Thread.Sleep(1000);
						triggerDownloaderStateEvent(FileDownloaderState.FINISHED);
					}
					else
					{
						responseStream.BeginRead(reqState.bufferRead, 0, 1448, ReadCallback, reqState);
					}
				}
				else if (retryCount < 2)
				{
					retryCount++;
					responseStream.BeginRead(reqState.bufferRead, 0, 1448, ReadCallback, reqState);
				}
				else
				{
					responseStream?.Close();
					if (reqState != null && reqState.response != null)
					{
						reqState.response.Close();
					}
					abortHttpWebRequestAndFileStream();
					triggerDownloaderStateEvent(FileDownloaderState.NOCONTENT);
				}
			}
		}
		catch (WebException)
		{
			responseStream?.Close();
			if (reqState != null && reqState.response != null)
			{
				reqState.response.Close();
			}
			abortHttpWebRequestAndFileStream();
			triggerDownloaderStateEvent(FileDownloaderState.FAILED);
		}
		catch (Exception)
		{
			responseStream?.Close();
			if (reqState != null && reqState.response != null)
			{
				reqState.response.Close();
			}
			abortHttpWebRequestAndFileStream();
			triggerDownloaderStateEvent(FileDownloaderState.FAILED);
		}
	}

	public void pauseDownload()
	{
		throw new NotImplementedException();
	}

	public void stopDownload()
	{
		isInterrupt = true;
	}

	private void abortHttpWebRequestAndFileStream()
	{
		logger.Debug("============" + filename + "@ abortHttpWebRequestAndFileStream");
		resumeDownload = false;
		try
		{
			webRequest.KeepAlive = false;
			webRequest.Abort();
		}
		catch
		{
		}
		if (destinationFileStream != null)
		{
			destinationFileStream.Close();
		}
	}

	private void triggerDownloaderStateEvent(FileDownloaderState status)
	{
		this.downloadStateChanged?.Invoke(this, new FileDownloaderStateChangedEventArgs(destinationPath, filename, status));
	}
}
