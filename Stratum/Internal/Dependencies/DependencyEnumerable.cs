using System;
using System.Collections;
using System.Collections.Generic;

namespace Stratum.Internal.Dependencies
{
	internal class DependencyEnumerable<TNode> : IEnumerable<IEnumerable<Graph<TNode, bool>.Node>>
	{
	    private readonly Graph<TNode, bool> _graph;
	    private readonly bool[] _dead;

	    public int Count => _graph.Count;

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

	    public DependencyEnumerable<TNode> Copy() => new(_graph);

	    private void Kill(Graph<TNode, bool>.Node node, HashSet<Graph<TNode, bool>.Node> killed)
	    {
	        killed.Add(node);

	        _dead[node.Value] = true;

	        var incomingEdges = new List<Graph<TNode, bool>.Edge>(node.Incoming);
	        foreach (var edge in incomingEdges)
	        {
	            var from = edge.From;

	            if (edge.Metadata)
			        Kill(from, killed);

	            from.Detach(node);
	        }

	        node.Abandon();
	    }

	    public IEnumerable<Graph<TNode, bool>.Node> Kill(Graph<TNode, bool>.Node node)
	    {
	        var killed = new HashSet<Graph<TNode, bool>.Node>();
	        Kill(node, killed);

	        return killed;
	    }

	    // Kahn's algorithm, adjusted to yield batches and allow for killing.
	    public IEnumerator<IEnumerable<Graph<TNode, bool>.Node>> GetEnumerator()
	    {
	        var graph = _graph.Copy();
	        var set = new List<Graph<TNode, bool>.Node>(graph.Count);
	        var incomingEdges = new List<Graph<TNode, bool>.Edge>(graph.Count);

	        foreach (var node in graph)
	            if (!_dead[node.Value] && node.OutgoingCount == 0)
	                set.Add(node);

	        while (set.Count > 0)
	        {
	            yield return set;

	            var count = set.Count;
	            for (var c = 0; c < count; ++c)
	            {
	                var node = set[0];

	                // We need to enumerate before detaching the nodes, otherwise we will be modifying the collection
	                // during enumeration
	                incomingEdges.AddRange(node.Incoming);

	                foreach (var edge in incomingEdges)
	                {
	                    var from = edge.From;

	                    from.Detach(node);

	                    if (!_dead[from.Value] && from.OutgoingCount == 0)
	                        set.Add(from);
	                }

	                incomingEdges.Clear();
	                set.RemoveAt(0);
	            }
	        }
	    }

	    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
