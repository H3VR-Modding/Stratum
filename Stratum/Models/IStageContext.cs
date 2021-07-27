using System.Collections.Generic;

namespace Stratum
{
	/// <summary>
	///     Represents a plugin's read-only data during a <see cref="IStage{TRet}" />. This is the read-only version of
	///     <see cref="IStageContext{TRet}" />.
	/// </summary>
	public interface IReadOnlyStageContext<TRet>
	{
		/// <summary>
		///     The stage that this context is a part of
		/// </summary>
		IStage<TRet> Stage { get; }

		/// <summary>
		///     The plugin that this context is for
		/// </summary>
		IReadOnlyStratumPlugin Plugin { get; }

		/// <summary>
		///     The loaders that this context provides
		/// </summary>
		IReadOnlyDictionary<string, Loader<TRet>> Loaders { get; }
	}

	/// <summary>
	///     Represents a plugin's data during a <see cref="IStage{TRet}" />
	/// </summary>
	public interface IStageContext<TRet> : IReadOnlyStageContext<TRet>
	{
		/// <inheritdoc cref="IReadOnlyStageContext{TRet}.Plugin" />
		new IStratumPlugin Plugin { get; }

		/// <inheritdoc cref="IReadOnlyStageContext{TRet}.Loaders" />
		new IDictionary<string, Loader<TRet>> Loaders { get; }
	}
}
