using Network;
using System;

public class FileDownloaderStateChangedEventArgs : EventArgs
{
	public FileDownloaderState newDownloaderState;

	public string destinationPath;

	public string filename;

	public FileDownloaderStateChangedEventArgs(string destinationPath, string filename, FileDownloaderState newState)
	{
		newDownloaderState = newState;
		this.destinationPath = destinationPath;
		this.filename = filename;
	}
}
