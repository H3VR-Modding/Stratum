using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Stratum.Internal.Dependencies
{
	internal class Graph<TNode, TEdge> : IEnumerable<Graph<TNode, TEdge>.Node>
	{
		private readonly Node[] _nodes;

		public int Count => _nodes.Length;

		private Graph(Node[] nodes) => _nodes = nodes;

		public Graph(IList<TNode> metadata)
		{
			_nodes = new Node[metadata.Count];
			for (var i = 0; i < _nodes.Length; ++i)
				_nodes[i] = new(this, i, metadata[i]);
		}

		public Graph<TNode, TEdge> Copy()
		{
			var nodes = new Node[_nodes.Length];
			var graph = new Graph<TNode, TEdge>(nodes);

			for (var i = 0; i < nodes.Length; ++i)
				nodes[i] = _nodes[i].Copy(graph);

			return graph;
		}

		private bool Acyclic(HashSet<int> history, Node node)
		{
			if (!history.Add(node.Value))
				return false;

			foreach (var next in node.Outgoing)
				if (!Acyclic(history, next.To))
					return false;

			history.Remove(node.Value);

			return true;
		}

		public bool Acyclic()
		{
			var history = new HashSet<int>();

			foreach (var node in this)
			{
				if (!Acyclic(history, node))
					return false;

				history.Clear();
			}

			return true;
		}

		public Node this[int item] => _nodes[item];

		public IEnumerator<Node> GetEnumerator() => ((IEnumerable<Node>) _nodes).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public class Node
		{
			private static Dictionary<int, TEdge> Copy(Dictionary<int, TEdge> source) =>
				source.ToDictionary(v => v.Key, v => v.Value);

			private readonly Graph<TNode, TEdge> _graph;
			private readonly Dictionary<int, TEdge> _incoming;
			private readonly Dictionary<int, TEdge> _outgoing;

			public int Value { get; }

			public TNode Metadata { get; }

			public IEnumerable<Edge> Incoming
			{
				get
				{
					foreach (var item in _incoming)
						yield return new(_graph[item.Key], this, item.Value);
				}
			}

			public int IncomingCount => _incoming.Count;

			public IEnumerable<Edge> Outgoing
			{
				get
				{
					foreach (var item in _outgoing)
						yield return new(this, _graph[item.Key], item.Value);
				}
			}

			public int OutgoingCount => _outgoing.Count;

			private Node(Graph<TNode, TEdge> graph, Dictionary<int, TEdge> incoming, Dictionary<int, TEdge> outgoing, int value, TNode metadata)
			{
				_graph = graph;
				_incoming = incoming;
				_outgoing = outgoing;

				Value = value;
				Metadata = metadata;
			}

			public Node(Graph<TNode, TEdge> graph, int value, TNode metadata)
			{
				_graph = graph;
				_incoming = new();
				_outgoing = new();

				Value = value;
				Metadata = metadata;
			}

			public Node Copy(Graph<TNode, TEdge> graph) =>
				new(graph, Copy(_incoming), Copy(_outgoing), Value, Metadata);

			public void Attach(Node node, TEdge metadata)
			{
				node._incoming.Add(Value, metadata);
				_outgoing.Add(node.Value, metadata);
			}

			public void Detach(Node node)
			{
				node._incoming.Remove(Value);
				_outgoing.Remove(node.Value);
			}

			public override string ToString() => $"{Value} ({Metadata})";

			public void Abandon()
			{
				foreach (var outgoing in _outgoing.Keys)
					_graph[outgoing]._incoming.Remove(Value);

				_outgoing.Clear();
			}
		}

		public readonly struct Edge
		{
			public Node From { get; }
			public Node To { get; }
			public TEdge Metadata { get; }

			public Edge(Node from, Node to, TEdge metadata)
			{
				From = from;
				To = to;
				Metadata = metadata;
			}

			public override string ToString() => $"{From} --({Metadata})-> {To}";
		}
	}
}
