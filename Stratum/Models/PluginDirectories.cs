using System.IO;

namespace Stratum
{
	/// <summary>
	///		Commonly used directories of plugins. Beware: there is no collision detection; another plugin can use the same directories.
	/// </summary>
	public sealed class PluginDirectories
	{
		private readonly DirectoryInfo _root;
		private DirectoryInfo? _cache;
		private DirectoryInfo? _data;
		private DirectoryInfo? _resources;

		/// <summary>
		///		Constructs an instance of <see cref="PluginDirectories"/>
		/// </summary>
		/// <param name="root">The directory to house subdirectories</param>
		public PluginDirectories(DirectoryInfo root)
		{
			_root = root;
		}

		/// <summary>
		///		Read-only files and directories required for the plugin to function. For example, items
		/// </summary>
		public DirectoryInfo Resources => _resources ??= _root.CreateSubdirectory("resources");

		/// <summary>
		///		Files and directories created and read by the plugin, which can be deleted with little to no repercussions. For example,
		///		saved web API responses
		/// </summary>
		public DirectoryInfo Cache => _cache ??= _root.CreateSubdirectory("cache");

		/// <summary>
		///		Files and directories created and read by the plugin, which have noticable effects. For example, player progression
		/// </summary>
		public DirectoryInfo Data => _data ??= _root.CreateSubdirectory("data");
	}
}
