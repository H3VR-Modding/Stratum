using System.IO;

namespace Stratum.Extensions
{
	public static class ExtDirectoryInfo
	{
		public static FileSystemInfo? GetChild(this DirectoryInfo @this, string name)
		{
			var abs = Path.Combine(@this.FullName, name);

			var dir = new DirectoryInfo(abs);
			if (dir.Exists)
				return dir;

			var file = new FileInfo(abs);
			if (file.Exists)
				return file;

			return null;
		}

		public static FileInfo GetFile(this DirectoryInfo @this, string path)
		{
			var abs = Path.Combine(@this.FullName, path);

			return new FileInfo(abs);
		}

		public static DirectoryInfo GetDirectory(this DirectoryInfo @this, string path)
		{
			var abs = Path.Combine(@this.FullName, path);

			return new DirectoryInfo(abs);
		}
	}
}
