using System;

namespace Stratum.Internal
{
	internal static class Extensions
	{
		public static T[] GetCustomAttributes<T>(this Type @this) where T : Attribute
		{
			object[] attrs = @this.GetCustomAttributes(typeof(T), false);

			return Array.ConvertAll(attrs, x => (T) x);
		}
	}
}
