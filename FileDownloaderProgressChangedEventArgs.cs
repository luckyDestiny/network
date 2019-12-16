using System;

public class FileDownloaderProgressChangedEventArgs : EventArgs
{
	public double downloadedPercentage;

	public string destinationPath;

	public string filename;

	public FileDownloaderProgressChangedEventArgs(string destinationPath, string filename, double percentage)
	{
		this.filename = filename;
		this.destinationPath = destinationPath;
		downloadedPercentage = percentage;
	}
}
