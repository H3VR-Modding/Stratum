using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Stratum
{
	public sealed class ObjectFrozenException : ObjectDisposedException
	{
		public ObjectFrozenException(string objectName) : base(objectName, "Object is frozen.") { }
		public ObjectFrozenException(Exception innerException) : base("Object is frozen.", innerException) { }

		// We need it because of some serialization reflection garbage. Very simple, so why not keep it?
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private ObjectFrozenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
