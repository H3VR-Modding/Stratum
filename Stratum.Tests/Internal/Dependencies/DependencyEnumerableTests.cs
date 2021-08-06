using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Stratum.Extensions;
using Stratum.Internal.Dependencies;
using Xunit;

namespace Stratum.Tests.Internal.Dependencies
{
	public class DependencyEnumerableTests
	{
		private static Action<IEnumerable<int>> AssertBatchEquals(params int[] expected)
		{
			return actual => Assert.True(actual.ToHashSet().SetEquals(expected));
		}

		[Fact]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		public void Ctor()
		{
			Graph<Empty, bool> graph = new(new Empty[0]);
			new DependencyEnumerable<Empty>(graph);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		public void GetEnumerator_LinearDependencies_ProperOrder(int count)
		{
			Graph<Empty, bool> graph = new(new Empty[count]);
			for (int i = graph.Count - 1; i >= 1; --i)
				graph[i].Attach(graph[i - 1], true);
			DependencyEnumerable<Empty> enumerable = new(graph);

			IEnumerable<int> ret = enumerable.SelectMany(x => x).Select(x => x.Value);

			Assert.Equal(Enumerable.Range(0, count), ret);
		}

		[Fact]
		public void GetEnumerator_MultipleDependencies_ProperOrder()
		{
			Graph<Empty, bool> graph = new(new Empty[3]);
			var (dependencyA, dependencyB, dependent) = (graph[0], graph[1], graph[2]);
			dependent.Attach(dependencyA, true);
			dependent.Attach(dependencyB, true);

			DependencyEnumerable<Empty> enumerable = new(graph);

			IEnumerable<IEnumerable<int>> ret = enumerable.Select(x => x.Select(y => y.Value));

			Assert.Collection(ret, AssertBatchEquals(0, 1), AssertBatchEquals(2));
		}

		[Fact]
		public void GetEnumerator_KilledDependency_CancelsHardDependencies()
		{
			Graph<Empty, bool> graph = new(new Empty[4]);
			var (dependency, dependent, implicitDependent, softDependent) = (graph[0], graph[1], graph[2], graph[3]);
			implicitDependent.Attach(dependent, true);
			dependent.Attach(dependency, true);
			softDependent.Attach(dependency, false);

			DependencyEnumerable<Empty> enumerable = new(graph);

			IEnumerable<Graph<Empty, bool>.Node[]> Killed()
			{
				using IEnumerator<Graph<Empty, bool>.Node[]> enumerator = enumerable.GetEnumerator();

				Assert.True(enumerator.MoveNext());

				enumerable.Kill(dependency);

				do
				{
					yield return enumerator.Current;
				} while (enumerator.MoveNext());
			}

			IEnumerable<IEnumerable<int>> ret = Killed().Select(x => x.Select(y => y.Value));

			Assert.Collection(ret, AssertBatchEquals(dependency.Value), AssertBatchEquals(softDependent.Value));
		}
	}
}
