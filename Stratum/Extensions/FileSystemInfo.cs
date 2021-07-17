using System;
using System.IO;

namespace Stratum.Extensions
{
	public static class ExtFileSystemInfo
	{
		public static FileInfo ConsumeFile(this FileSystemInfo @this)
		{
			return @this as FileInfo ?? throw new ArgumentException("Handle must be a file.");
		}

		public static DirectoryInfo ConsumeDirectory(this FileSystemInfo @this)
		{
			return @this as DirectoryInfo ?? throw new ArgumentException("Handle must be a directory.");
		}
	}
}
