using System.IO;

namespace Stratum.IO
{
	public sealed class PluginDirectories
	{
		private readonly DirectoryInfo _root;
		private DirectoryInfo? _resources;
		private DirectoryInfo? _cache;
		private DirectoryInfo? _data;

		public DirectoryInfo Resources => _resources ??= _root.CreateSubdirectory("resources");
		public DirectoryInfo Cache => _cache ??= _root.CreateSubdirectory("cache");
		public DirectoryInfo Data => _data ??= _root.CreateSubdirectory("data");

		public PluginDirectories(DirectoryInfo root) => _root = root;
	}
}
