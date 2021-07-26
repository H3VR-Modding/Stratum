using System;
using System.Collections.Generic;
using System.Text;

namespace Stratum.Jobs
{
	public abstract class Pipeline<TRet, TSelf> where TSelf : Pipeline<TRet, TSelf>
	{
		protected Pipeline(TSelf parent)
		{
			Parent = parent;
		}

		public Pipeline() { }

		public TSelf? Parent { get; }

		public List<Job<TRet>> Jobs { get; } = new();

		public string? Name { get; set; }

		protected abstract TSelf CreateNested();

		public TSelf WithName(string? name)
		{
			Name = name;
			return (TSelf) this;
		}

		public TSelf AddJob(Job<TRet> job)
		{
			Jobs.Add(job);
			return (TSelf) this;
		}

		public TSelf AddNested(Action<TSelf> nested, PipelineBuilder<TRet, TSelf> builder)
		{
			TSelf subpipe = CreateNested();
			nested(subpipe);
			Job<TRet> job = builder(subpipe);

			return AddJob(job);
		}

		public override string ToString()
		{
			const string unnamed = "<unnamed>";
			StringBuilder builder = new();

			// Path-like structure to determine the callstack of the pipeline.
			TSelf? parent = Parent;
			while (parent != null)
			{
				builder
					.Insert(0, '/')
					.Insert(0, parent.Name ?? unnamed);
				parent = parent.Parent;
			}

			builder.Append(Name ?? unnamed);

			return builder.ToString();
		}
	}

	public sealed class Pipeline<TRet> : Pipeline<TRet, Pipeline<TRet>>
	{
		private Pipeline(Pipeline<TRet> parent) : base(parent) { }

		protected override Pipeline<TRet> CreateNested()
		{
			return new(this);
		}
	}
}
