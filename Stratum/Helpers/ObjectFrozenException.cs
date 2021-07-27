using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Stratum
{
	/// <summary>
	///		Thrown when a write operation is called on a frozen object
	/// </summary>
	public sealed class ObjectFrozenException : ObjectDisposedException
	{
		/// <summary>
		///		Constructs an instance of <see cref="ObjectFrozenException"/>
		/// </summary>
		/// <param name="objectName">The full type name of the object</param>
		public ObjectFrozenException(string objectName) : base(objectName, "Object is frozen.") { }

		/// <summary>
		///		Constructs an instance of <see cref="ObjectFrozenException"/>
		/// </summary>
		/// <param name="innerException">The exception that caused this exception</param>
		public ObjectFrozenException(Exception innerException) : base("Object is frozen.", innerException) { }

		// We need it because of some serialization reflection garbage. Very simple, so why not keep it?
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private ObjectFrozenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
