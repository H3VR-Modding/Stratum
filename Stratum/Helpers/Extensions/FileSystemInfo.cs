using System;
using System.IO;

namespace Stratum.Extensions
{
	public static class ExtFileSystemInfo
	{
		public static FileInfo RequireFile(this FileSystemInfo @this) =>
			@this as FileInfo ?? throw new ArgumentException("Handle must be a file.");

		public static DirectoryInfo RequireDirectory(this FileSystemInfo @this) =>
			@this as DirectoryInfo ?? throw new ArgumentException("Handle must be a directory.");
	}
}
