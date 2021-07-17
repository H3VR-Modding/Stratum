using System;
using System.Collections;
using System.Collections.Generic;

namespace Stratum.Internal.Dependencies
{
	internal class DependencyEnumerable<TNode> : IEnumerable<IEnumerable<Graph<TNode, bool>.Node>>
	{
		private readonly bool[] _dead;
		private readonly Graph<TNode, bool> _graph;

		public DependencyEnumerable(Graph<TNode, bool> graph)
		{
			if (!graph.Acyclic())
				throw new ArgumentException("Graph must be acyclic", nameof(graph));

			_graph = graph.Copy();
			_dead = new bool[_graph.Count];
		}

		public DependencyEnumerable(Graph<TNode, bool> graph, bool[] dead) : this(graph)
		{
			if (graph.Count != dead.Length)
				throw new ArgumentException("The dead array must be as long as the count of the graph.");

			_dead = dead;
		}

		public int Count => _graph.Count;

		// Kahn's algorithm, adjusted to yield batches and allow for killing.
		public IEnumerator<IEnumerable<Graph<TNode, bool>.Node>> GetEnumerator()
		{
			Graph<TNode, bool> graph = _graph.Copy();
			List<Graph<TNode, bool>.Node> set = new(graph.Count);
			List<Graph<TNode, bool>.Edge> incomingEdges = new(graph.Count);

			foreach (var node in graph)
				if (!_dead[node.Value] && node.OutgoingCount == 0)
					set.Add(node);

			while (set.Count > 0)
			{
				yield return set;

				var count = set.Count;
				for (var c = 0; c < count; ++c)
				{
					Graph<TNode, bool>.Node? node = set[0];

					// We need to enumerate before detaching the nodes, otherwise we will be modifying the collection
					// during enumeration
					incomingEdges.AddRange(node.Incoming);

					foreach (Graph<TNode, bool>.Edge edge in incomingEdges)
					{
						Graph<TNode, bool>.Node from = edge.From;

						from.Detach(node);

						if (!_dead[from.Value] && from.OutgoingCount == 0)
							set.Add(from);
					}

					incomingEdges.Clear();
					set.RemoveAt(0);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public DependencyEnumerable<TNode> Copy()
		{
			return new(_graph);
		}

		private void Kill(Graph<TNode, bool>.Node node, HashSet<Graph<TNode, bool>.Node> killed)
		{
			killed.Add(node);

			_dead[node.Value] = true;

			List<Graph<TNode, bool>.Edge> incomingEdges = new(node.Incoming);
			foreach (Graph<TNode, bool>.Edge edge in incomingEdges)
			{
				Graph<TNode, bool>.Node from = edge.From;

				if (edge.Metadata)
					Kill(from, killed);

				from.Detach(node);
			}

			node.Abandon();
		}

		public IEnumerable<Graph<TNode, bool>.Node> Kill(Graph<TNode, bool>.Node node)
		{
			HashSet<Graph<TNode, bool>.Node> killed = new();
			Kill(node, killed);

			return killed;
		}
	}
}
