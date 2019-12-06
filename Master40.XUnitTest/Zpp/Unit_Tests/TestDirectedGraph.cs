using System.Collections.Generic;
using System.Linq;
using Xunit;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;
using Zpp.Util.StackSet;

namespace Master40.XUnitTest.Zpp.Unit_Tests
{
    public class TestDirectedGraph
    {
        [Fact]
        public void TestAsString()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(7);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            Assert.True(directedGraph.ToString() != null,
                "AsString() must work in unit tests without a database.");
        }

        [Fact]
        public void TestGetLeafs()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(7);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            INodes leafs = directedGraph.GetLeafNodes();

            Assert.True(leafs != null, "There should be leafs in the graph.");

            for (int i = 3; i < 7; i++)
            {
                Assert.True(leafs.Contains(nodes[i]), $"Leafs do not contain node {nodes[i]}.");
            }
        }

        [Fact]
        public void TestGetRoots()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(7);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            INodes roots = directedGraph.GetRootNodes();

            Assert.True(roots != null, "There should be roots in the graph.");

            Assert.True(roots.Contains(nodes[0]), $"Leafs do not contain node {nodes[0]}.");
            Assert.True(roots.Count() == 1, "Roots must contain exact one node.");
        }

        [Fact]
        public void TestGetSuccessorNodes()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(7);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            INodes leafs = directedGraph.GetLeafNodes();
            foreach (var node in nodes)
            {
                INodes successors = directedGraph.GetSuccessorNodes(node);
                bool isLeaf = leafs.Contains(node);
                if (isLeaf)
                {
                    Assert.True(successors == null, "A leaf cannot have successors.");
                }
                else
                {
                    Assert.True(successors != null, "A non-leaf MUST have successors.");
                }
            }
        }

        [Fact]
        public void TestGetPredecessorNodes()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(7);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            INodes roots = directedGraph.GetRootNodes();
            foreach (var node in nodes)
            {
                INodes predecessors = directedGraph.GetPredecessorNodes(node);
                bool isRoot = roots.Contains(node);
                if (isRoot)
                {
                    Assert.True(predecessors == null, "A root cannot have predecessors.");
                }
                else
                {
                    Assert.True(predecessors != null, "A non-root MUST have predecessors.");
                }
            }
        }

        [Fact]
        public void TestAddEdge()
        {
            IDirectedGraph<INode> directedGraph = new DirectedGraph();
            INode[] nodes = EntityFactory.CreateDummyNodes(3);
            INode a = nodes[0];
            INode b = nodes[1];
            INode c = nodes[2];
            // create a -> b -> c
            IEdge ab = new Edge(a, b);
            IEdge bc = new Edge(b, c);
            directedGraph.AddEdge(ab);
            directedGraph.AddEdge(bc);


            Assert.True(directedGraph.Contains(a) && directedGraph.Contains(b) && directedGraph.Contains(c),
                    $"Not every node was added..");
            
        }

        [Fact]
        public void TestGetEdges()
        {
            IDirectedGraph<INode> directedGraph = new DirectedGraph();
            INode[] nodes = EntityFactory.CreateDummyNodes(3);
            INode a = nodes[0];
            INode b = nodes[1];
            INode c = nodes[2];
            // create a -> b -> c
            IEdge ab = new Edge(a, b);
            IEdge bc = new Edge(b, c);
            directedGraph.AddEdge(ab);
            directedGraph.AddEdge(bc);
            List<IEdge> expectedEdges = new List<IEdge>();
            expectedEdges.Add(ab);
            expectedEdges.Add(bc);

            IStackSet<IEdge> actualEdges = directedGraph.GetEdges();
            foreach (var actualEdge in actualEdges)
            {
                Assert.True(expectedEdges.Contains(actualEdge),
                    $"I have not added this edge {actualEdge}. Where comes that from?");
            }

            foreach (var expectedEdge in expectedEdges)
            {
                Assert.True(actualEdges.Contains(expectedEdge),
                    $"This edge {expectedEdge} was not returned.");
            }
        }

        [Fact]
        public void TestRemoveNode()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(6);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            INode nodeToRemove = nodes[2];
            directedGraph.RemoveNode(nodeToRemove, false);
            
            Assert.True(directedGraph.Contains(nodeToRemove) == false);
            
        }

        private IDirectedGraph<INode> CreateBinaryDirectedGraph(INode[] nodes)
        {
            IDirectedGraph<INode> directedGraph = new DirectedGraph();
            INode root;
            INodes leafs = new Nodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                // left: 2*i + 1 , right: 2*i + 2
                int maxIndex = nodes.Length;
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                if (left < maxIndex)
                {
                    directedGraph.AddEdge(new Edge(nodes[i], nodes[left]));
                }

                if (right < maxIndex)
                {
                    directedGraph.AddEdge(new Edge(nodes[i], nodes[right]));
                }
            }

            return directedGraph;
        }

        [Fact]
        public void TestGetPredecessorNodesRecursive()
        {
            INode[] nodes = EntityFactory.CreateDummyNodes(6);
            IDirectedGraph<INode> directedGraph = CreateBinaryDirectedGraph(nodes);
            foreach (var node in nodes)
            {
                INodes predecessors = directedGraph.GetPredecessorNodes(node);
                if (predecessors == null)
                {
                    continue;
                }
                INodes predecessorsRecursive = directedGraph.GetPredecessorNodesRecursive(node);
                Assert.True(predecessorsRecursive.Contains(node) == false);
                foreach (var predecessor in predecessors)
                {
                    Assert.True(predecessorsRecursive.Contains(predecessor));
                }
            }
            
        }
    }
}