using System.IO;

namespace Stratum.Extensions
{
	/// <summary>
	///		Extension methods pertaining to <see cref="FileSystemInfo"/>
	/// </summary>
	public static class ExtDirectoryInfo
	{
		/// <summary>
		///		Attempts to find a child handle of this directory with the specified name, otherwise returns <see langword="null"/>
		/// </summary>
		/// <param name="this"></param>
		/// <param name="name">The name of the handle</param>
		public static FileSystemInfo? GetChild(this DirectoryInfo @this, string name)
		{
			string abs = Path.Combine(@this.FullName, name);

			DirectoryInfo dir = new(abs);
			if (dir.Exists)
				return dir;

			FileInfo file = new(abs);
			if (file.Exists)
				return file;

			return null;
		}

		/// <summary>
		///		Gets a file within this directory, whether it exists or not
		/// </summary>
		/// <param name="this"></param>
		/// <param name="path">The path to the file, relative to this directory</param>
		public static FileInfo GetFile(this DirectoryInfo @this, string path)
		{
			string abs = Path.Combine(@this.FullName, path);

			return new FileInfo(abs);
		}

		/// <summary>
		///		Gets a file within this directory if it exists, otherwise throws a <see cref="FileNotFoundException"/>
		/// </summary>
		/// <param name="this"></param>
		/// <param name="path">The path to the file, relative to this directory</param>
		/// <exception cref="FileNotFoundException">File did not exist</exception>
		public static FileInfo RequireFile(this DirectoryInfo @this, string path)
		{
			FileInfo file = @this.GetFile(path);
			if (!file.Exists)
				throw new FileNotFoundException("File required", file.FullName);

			return file;
		}

		/// <summary>
		///		Gets a directory within this directory, whether it exists or not
		/// </summary>
		/// <param name="this"></param>
		/// <param name="path">The path to the directory, relative to this directory</param>
		public static DirectoryInfo GetDirectory(this DirectoryInfo @this, string path)
		{
			string abs = Path.Combine(@this.FullName, path);

			return new DirectoryInfo(abs);
		}

		/// <summary>
		///		Gets a directory within this directory if it exists, otherwise throws a <see cref="DirectoryNotFoundException"/>
		/// </summary>
		/// <param name="this"></param>
		/// <param name="path">The path to the directory, relative to this directory</param>
		/// <exception cref="DirectoryNotFoundException">Directory did not exist</exception>
		public static DirectoryInfo RequireDirectory(this DirectoryInfo @this, string path)
		{
			DirectoryInfo directory = @this.GetDirectory(path);
			if (!directory.Exists)
				throw new DirectoryNotFoundException();

			return directory;
		}
	}
}
