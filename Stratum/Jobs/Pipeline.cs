using System;
using System.Collections.Generic;
using System.Text;

namespace Stratum.Jobs
{
	public abstract class Pipeline<TRet, TSelf> where TSelf : Pipeline<TRet, TSelf>
	{
		private string? _name;

		public TSelf? Parent { get; }

		public List<Job<TRet>> Jobs { get; } = new();

		public string Name => _name ?? "<unknown>";

		protected Pipeline(TSelf parent) => Parent = parent;

		public Pipeline() => _name = "<root>"; 

		protected abstract TSelf CreateNested();

		public TSelf WithName(string? name)
		{
			_name = name;
			return (TSelf) this;
		}

		public TSelf AddJob(Job<TRet> job)
		{
			Jobs.Add(job);
			return (TSelf) this;
		}

		public TSelf AddNested(Action<TSelf> nested, PipelineBuilder<TRet, TSelf> builder)
		{
			var subpipe = CreateNested();
			nested(subpipe);
			var job = builder(subpipe);

			return AddJob(job);
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			// Path-like structure to determine the callstack of the pipeline.
			var parent = Parent;
			while (parent is not null)
			{
				builder
					.Insert(0, '/')
					.Insert(0, parent.Name);
				parent = parent.Parent;
			}

			builder
				.Append(Name)
				.Append(" (")
				.Append(Jobs.Count)
				.Append(')');

			return builder.ToString();
		}
	}

	public sealed class Pipeline<TRet> : Pipeline<TRet, Pipeline<TRet>>
	{
		private Pipeline(Pipeline<TRet> parent) : base(parent) { }

		protected override Pipeline<TRet> CreateNested() => new(this);
	}
}
