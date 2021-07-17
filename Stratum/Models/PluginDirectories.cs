using System.IO;

namespace Stratum
{
	public sealed class PluginDirectories
	{
		private readonly DirectoryInfo _root;
		private DirectoryInfo? _cache;
		private DirectoryInfo? _data;
		private DirectoryInfo? _resources;

		public PluginDirectories(DirectoryInfo root)
		{
			_root = root;
		}

		public DirectoryInfo Resources => _resources ??= _root.CreateSubdirectory("resources");
		public DirectoryInfo Cache => _cache ??= _root.CreateSubdirectory("cache");
		public DirectoryInfo Data => _data ??= _root.CreateSubdirectory("data");
	}
}
