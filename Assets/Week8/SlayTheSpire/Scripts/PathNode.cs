using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelType {
    Boss,
    Unknown,
    Merchant,
    Treasure,
    Rest,
    Enemy,
    Elite
}
public class PathNode {
    private List<PathNode> connectedNodes;
    public List<PathNode> ConnectedNodes => connectedNodes;

    public List<PathNode> ConnectedByNodes;
    

    private int depth = 0;
    public int Depth => depth;

    private int order = 0;
    public int Order => order;

    private LevelType nodeType;
    public LevelType NodeType => nodeType;

    public Vector3 PositionOnMap;

    public List<GameObject> ConnectedByLevelObject;
    public PathNode() {
        connectedNodes = new List<PathNode>();
        nodeType = LevelType.Enemy;
        ConnectedByLevelObject = new List<GameObject>();
        ConnectedByNodes = new List<PathNode>();
    }

    public PathNode(int depth, int order, LevelType nodeType) : this() {
        this.depth = depth;
        this.order = order;
        this.nodeType = nodeType;
    }

    public PathNode(int depth, int order, LevelType levelType, params PathNode[] nextNodes) : this(depth, order, levelType) {
        connectedNodes.AddRange(nextNodes);
    }

    public void AddNode(PathNode node) {
        connectedNodes.Add(node);
    }

    public void AddNodes(params PathNode[] nodes) {
        connectedNodes.AddRange(nodes);
    }
}
