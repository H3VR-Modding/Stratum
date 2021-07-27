namespace Stratum
{
	/// <summary>
	///		Represents an object that may turn read-only
	/// </summary>
	public interface IFreezable
	{
		/// <summary>
		///		Freezes the object, turning it read-only. Any writing operations should throw an <see cref="ObjectFrozenException"/>.
		/// </summary>
		void Freeze();
	}
}
