namespace Stratum
{
	/// <summary>
	///		Represents a plugin batch that has been loaded
	/// </summary>
	public interface ILoadedBatch : IEventBatch<ILoadedPlugin>, ITimed
	{
	}
}
