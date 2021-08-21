namespace Stratum
{
	/// <summary>
	///     Represents an object that is part of a stage
	/// </summary>
	public interface IHasStage
	{
		/// <summary>
		///     The stage that this object is a part of
		/// </summary>
		Stages Stage { get; }
	}
}
