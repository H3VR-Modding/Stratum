using System;
using System.IO;

namespace Stratum.Extensions
{
	/// <summary>
	///     Extension methods pertaining to <see cref="FileSystemInfo" />
	/// </summary>
	public static class ExtFileSystemInfo
	{
		/// <summary>
		///     Returns a <see cref="FileInfo" /> if this handle is a file, otherwise throws an <see cref="ArgumentException" />
		/// </summary>
		/// <param name="this"></param>
		/// <exception cref="ArgumentException">Handle was not a file</exception>
		public static FileInfo ConsumeFile(this FileSystemInfo @this)
		{
			return @this as FileInfo ?? throw new ArgumentException("Handle must be a file.");
		}

		/// <summary>
		///     Returns a <see cref="DirectoryInfo" /> if this handle is a directory, otherwise throws an
		///     <see cref="ArgumentException" />
		/// </summary>
		/// <param name="this"></param>
		/// <exception cref="ArgumentException">Handle was not a file</exception>
		public static DirectoryInfo ConsumeDirectory(this FileSystemInfo @this)
		{
			return @this as DirectoryInfo ?? throw new ArgumentException("Handle must be a directory.");
		}
	}
}
