using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Stratum.Internal.Dependencies;
using Xunit;

namespace Stratum.Tests.Internal.Dependencies
{
	public class GraphTests
	{
		[Fact]
		[SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
		public void Ctor()
		{
			new Graph<Empty, Empty>(Array.Empty<Empty>());
		}

		[Fact]
		public void Acyclic_AcyclicGraph_True()
		{
			Graph<Empty, Empty> graph = new(new Empty[3]);
			var (first, second, third) = (graph[0], graph[1], graph[2]);

			first.Attach(second, new Empty());
			second.Attach(third, new Empty());
			bool ret = graph.Acyclic();

			Assert.True(ret);
		}

		[Fact]
		public void Acyclic_CyclicGraph_False()
		{
			Graph<Empty, Empty> graph = new(new Empty[2]);
			var (first, second) = (graph[0], graph[1]);

			first.Attach(second, new Empty());
			second.Attach(first, new Empty());
			bool ret = graph.Acyclic();

			Assert.False(ret);
		}

		[Fact]
		public void Copy_MetadataEqual()
		{
			const int count = 10;
			object[] metadata = new object[count].Populate();
			Graph<object, Empty> graph = new(metadata);

			Graph<object, Empty> ret = graph.Copy();

			Assert.Equal(graph.Count, ret.Count);
			for (var i = 0; i < metadata.Length; ++i)
				Assert.Equal(graph[i].Metadata, ret[i].Metadata);
		}

		public class NodeTests
		{
			[Fact]
			public void IncomingOutgoingCount_MatchesCollections()
			{
				Graph<Empty, Empty> graph = new(new Empty[3]);
				var (first, second, third) = (graph[0], graph[1], graph[2]);

				first.Attach(second, new Empty());
				second.Attach(third, new Empty());

				static void AssertEquality(Graph<Empty, Empty>.Node node)
				{
					Assert.Equal(node.Incoming.Count(), node.IncomingCount);
					Assert.Equal(node.Outgoing.Count(), node.OutgoingCount);
				}

				AssertEquality(first);
				AssertEquality(second);
				AssertEquality(third);
			}

			[Fact]
			public void Attach_Edges_CorrectFromTo()
			{
				Graph<Empty, Empty> graph = new(new Empty[2]);
				var (first, second) = (graph[0], graph[1]);

				void EdgeValidator(Graph<Empty, Empty>.Edge x)
				{
					Assert.Equal(first, x.From);
					Assert.Equal(second, x.To);
				}

				first.Attach(second, new Empty());

				Assert.Empty(first.Incoming);
				Assert.Collection(first.Outgoing, EdgeValidator);
				Assert.Collection(second.Incoming, EdgeValidator);
				Assert.Empty(second.Outgoing);
			}

			[Fact]
			public void Attach_Edges_CorrectMetadata()
			{
				Graph<Empty, object> graph = new(new Empty[2]);
				var (first, second) = (graph[0], graph[1]);
				object metadata = new();

				void EdgeValidator(Graph<Empty, object>.Edge x)
				{
					Assert.Equal(metadata, x.Metadata);
				}

				first.Attach(second, metadata);

				Assert.Empty(first.Incoming);
				Assert.Collection(first.Outgoing, EdgeValidator);
				Assert.Collection(second.Incoming, EdgeValidator);
				Assert.Empty(second.Outgoing);
			}

			[Fact]
			public void Abandon_NoEdges()
			{
				Graph<Empty, Empty> graph = new(new Empty[3]);
				var (first, second, third) = (graph[0], graph[1], graph[2]);

				first.Attach(second, new Empty());
				second.Attach(third, new Empty());
				second.Abandon();

				Assert.Equal(1, second.IncomingCount);
				Assert.Equal(0, second.OutgoingCount);
			}
		}
	}
}
