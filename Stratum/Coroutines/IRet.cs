namespace Stratum.Coroutines
{
	public interface IRet<in T>
	{
		T Value { set; }
	}
}
