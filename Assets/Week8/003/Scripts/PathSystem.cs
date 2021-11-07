using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PathSystem : MonoBehaviour {

    private System.Random random;

    //level seed, not game seed
    [HideInInspector]
    public int LevelSeed;

    [Space]
    [SerializeField]
    private bool animatedPath;


    private List<List<PathNode>> nodeTree = new List<List<PathNode>>();

    [SerializeField]
    private int pathDepth = 20;
    public int PathDepth => pathDepth;

    [SerializeField]
    private int pathWidth = 7;


    public static PathSystem Singleton;

    [Range(0f, 10.0f)]
    [SerializeField]
    private float cellXInterval = 1.0f;

    [Range(0f, 20f)]
    [SerializeField]
    private float cellYInterval = 1.0f;


    [SerializeField]
    private float unknownLevelPossibility;

    [SerializeField]
    private float merchantLevelPossibility;

    [SerializeField]
    private float restLevelPossibility;

    [SerializeField]
    private float enemyLevelPossibility;

    [SerializeField]
    private float eliteLevelPossibility;

    [SerializeField] 
    private GameObject[] levelObjectPrefabs;

    [SerializeField] 
    private GameObject line;

    public Transform startLocation;

    
    private int treasureLevel;

    private void Awake() {
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start() {
        SetSeed();
        InitializeTree();
        CreatePath();
    }

    private void InitializeTree()
    {
        nodeTree.Clear();
        for (int i = 0; i <= pathDepth; i++)
        {
            List<PathNode> currentPathHeight = new List<PathNode>(pathWidth);

            for (int j = 0; j < pathWidth; j++) {
                currentPathHeight.Add(null);
            }

            nodeTree.Add(currentPathHeight);
        }

        int middleHeight = pathDepth / 2;
        treasureLevel = random.Next(middleHeight - 1, middleHeight + 2);
    }


    void SetSeed() {
        random = new System.Random(GameManager.Singleton.LevelSeed);
    }


    //level possibilities:
    //Boss: Always at depth 0, only 1
    //Treasure: Must but only one depth always have a line of treasure
    //(players always encounter one and only one treasure level in each chapter)
    //UnKnown, Merchant, Rest, Enemy, Elite: depend on possibilities
    //(The deepest level is always enemy; depth 1 is always rest)

    void CreatePath()
    {
        InitializeTree();
        BuildBossLevel();

        List<PathNode> nodesAtCurrentDepth = new List<PathNode>() {
            nodeTree[0][pathWidth / 2]
        };


        //all nodes (exclude the last line)
        for (int depth = 0; depth < pathDepth; depth++)
        {
            Debug.Log(nodeTree.Count);
            List<PathNode> tempNextLevelNodesRecord = new List<PathNode>();
            int mimimumIndex = 0;
            List<int> connectedIndex = new List<int>();
            Debug.Log($"Depth: {depth}");

            //all nodes in current depth
            foreach (PathNode currentLevelNode in nodesAtCurrentDepth)
            {
                
                int currentLevelOrder = currentLevelNode.Order; //0-6


                int connectedNodeNumber = GetConnectedNodeNumber(depth);
                Debug.Log($"Current Node Order: {currentLevelNode.Order}, " +
                          $"Connecting {connectedNodeNumber} nodes");

                //connected order range: order-2 ~ order+2
                //also make sure lines don't intersect
                int connectedLevelMinimumBound = Math.Max(mimimumIndex, currentLevelOrder - 2);
                int connectedLevelMaxmimumBound = Math.Min(pathWidth - 1, currentLevelOrder + 2);

                connectedNodeNumber = Math.Min(connectedLevelMaxmimumBound - connectedLevelMinimumBound +1,
                    connectedNodeNumber);

                Debug.Log($"Min: {connectedLevelMinimumBound}, Max: {connectedLevelMaxmimumBound}");
                //connected nodes
                List<PathNode> connectedNodes = new List<PathNode>(connectedNodeNumber);
                

                for (int i = 0; i < connectedNodeNumber; i++)
                {

                    bool duplicate = false;
                    int connectedNodeIndex;

                    do {
                        duplicate = false;
                        //have more possibility to connect to what others already connected
                        connectedNodeIndex = GenerateConnectedNodeIndex(connectedLevelMinimumBound,
                            connectedLevelMaxmimumBound,
                            tempNextLevelNodesRecord);

                        foreach (PathNode node in connectedNodes)
                        {
                            if (node.Order == connectedNodeIndex)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                    } while (duplicate);
                    Debug.Log($"Connecting: {connectedNodeIndex}");
                    connectedIndex.Add(connectedNodeIndex);
                    PathNode connectedNode = CheckNodeExist(connectedNodeIndex, tempNextLevelNodesRecord);
                   
                    if (connectedNode == null)
                    {

                        connectedNode = CreateNewNode(depth + 1, connectedNodeIndex);
                        tempNextLevelNodesRecord.Add(connectedNode);
                        
                        nodeTree[depth + 1][connectedNodeIndex] = connectedNode;
                    }
                    connectedNode.ConnectedByNodes.Add(currentLevelNode);
                    currentLevelNode.AddNode(connectedNode);
                    connectedNodes.Add(connectedNode);
                }

                mimimumIndex = connectedIndex.FindBiggest();
                SortNodesList(tempNextLevelNodesRecord);
            }

            nodesAtCurrentDepth.Clear();
            nodesAtCurrentDepth.AddRange(tempNextLevelNodesRecord);
            //sort nodesAtCurrentDepth
            SortNodesList(nodesAtCurrentDepth);
        }

        StartCoroutine(CreatePathRoutine());
    }

    private int GenerateConnectedNodeIndex(int minimumBound, int maxBound, List<PathNode> existingNodes) {
        if (existingNodes.Count > 0 && existingNodes[existingNodes.Count-1].Order >= minimumBound) {
            int ran = random.Next(0, 100);
            if (ran <= 95) {
                return existingNodes[existingNodes.Count - 1].Order;
            }
        }
        return random.Next(minimumBound, maxBound + 1);

    }


    private void SortNodesList(List<PathNode> nodes) {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {

                if (nodes[i].Order > nodes[j].Order)
                {
                    PathNode temp = nodes[i];
                    nodes[i] = nodes[j];
                    nodes[j] = temp;
                }
            }
        }
    }



    private int GetConnectedNodeNumber(int depth) {
        int distanceFromStart = (pathDepth / 2) - Mathf.Abs(depth - (pathDepth / 2));

        float frac = (float) distanceFromStart / (pathDepth / 2);
        //frac: 0 - 1
        int chance = random.Next(0, 100);

        if  (depth> 2 && depth != pathDepth - 2) {
           
            if (frac >= 0 && frac < 0.3) {

                if (chance >= 0 && chance < 70) {
                    return 1;
                }else if (chance >= 70 && chance < 90) {
                    return 2;
                }
                
                return 3;
                

            }else if (frac >= 0.3 && frac < 0.6) {
                if (chance >= 0 && chance < 60)
                {
                    return 1;
                }
                else if (chance >= 60 && chance < 90)
                {
                    return 2;
                }

                return 3;
            }
            else if (frac >= 0.6 && frac < 0.9) {

                if (chance >= 0 && chance < 50)
                {
                    return 1;
                }
                else if (chance >= 50 && chance < 90)
                {
                    return 2;
                }

                return 3;
            }
           
            else {
                if (chance >= 0 && chance < 40)
                {
                    return 1;
                }else if (chance >= 50 && chance <= 80) {
                    return 2;
                }
                return 3;
            }

        }else {
            if (depth <= 2) {
                return random.Next(3, 6);
            }
            else {
                return 2;
            }
           
        }
        
    }
    private PathNode CheckNodeExist(int order, List<PathNode> existingNodes)
    {
        foreach (PathNode existingNode in existingNodes)
        {
            if (existingNode.Order == order)
            {
                return existingNode;
            }
        }

        return null;
    }

    private PathNode CreateNewNode(int depth, int order)
    {
        LevelType type;

        if (depth == 1) {
            type = LevelType.Rest;
        }else if (depth == treasureLevel) {
            type = LevelType.Treasure;
        }else if (depth == pathDepth) {
            type = LevelType.Enemy;
        }
        else {
            int levelChance = random.Next(0, 100);

            
            if (levelChance >= 0 && levelChance < unknownLevelPossibility)
            {
                type = LevelType.Unknown;
            }
            else if (levelChance >= unknownLevelPossibility &&
                     levelChance < unknownLevelPossibility + merchantLevelPossibility)
            {
                type = LevelType.Merchant;
            }
            else if (levelChance >= unknownLevelPossibility + merchantLevelPossibility &&
                     levelChance < unknownLevelPossibility + merchantLevelPossibility + restLevelPossibility)
            {
                type = LevelType.Rest;
            }
            else if (levelChance >= unknownLevelPossibility + merchantLevelPossibility + restLevelPossibility
                     && levelChance <= unknownLevelPossibility + merchantLevelPossibility + restLevelPossibility +
                     enemyLevelPossibility)
            {
                type = LevelType.Enemy;
            }
            else
            {
                type = LevelType.Elite;
            }
        }

        

        return new PathNode(depth, order, type);
    }

    void BuildBossLevel()
    {
       
        nodeTree[0][pathWidth / 2] = new PathNode(0, pathWidth / 2,
            LevelType.Boss);
    }



    IEnumerator CreatePathRoutine()
    {
        float YPos = startLocation.position.y;

        for (int i = pathDepth; i >= 0; i--)
        {
            float XPos = startLocation.position.x;

            for (int j = 0; j < pathWidth; j++)
            {
                if (nodeTree[i][j] != null) {
                    PathNode node = nodeTree[i][j];

                    Vector3 pos;
                    if (node.NodeType == LevelType.Boss)
                    {//add a bit offset
                        pos = new Vector3(XPos + (float)(random.NextDouble() * 2 - 1) * 0.1f,
                            (YPos + 1 + ((float)random.NextDouble() * 2 - 1) * 0.2f));
                    }
                    else {
                        //add a bit offset
                        pos = new Vector3(XPos + (float)(random.NextDouble() * 2 - 1) * 0.1f,
                            (YPos + ((float)random.NextDouble() * 2 - 1) * 0.2f));
                    }


                    node.PositionOnMap = pos;

                    GameObject obj = Instantiate(levelObjectPrefabs[(int)node.NodeType],
                        pos, Quaternion.identity);

                    node.LevelObject = obj;
                    obj.GetComponent<LevelObject>().LevelType = node.NodeType;
                    node.ConnectedNodes.ForEach(connectedNodes => {
                        connectedNodes.ConnectedByLevelObject.Add(obj);
                    });
                    obj.GetComponent<LevelObject>().Node = node;
                    yield return new WaitForSeconds(0.01f);
                }

                XPos += cellXInterval;
            }

            YPos += cellYInterval;
        }



        //draw connection
        for (int i = 0; i < nodeTree.Count; i++) {
            for (int j = 0; j < nodeTree[i].Count; j++) {
                if (nodeTree[i][j] != null) {
                    if (nodeTree[i][j].ConnectedNodes.Count > 0) {
                        PathNode node = nodeTree[i][j];

                        Vector3 fromPos;
                        if (node.NodeType == LevelType.Boss) {
                            fromPos = new Vector3(node.PositionOnMap.x,
                                node.PositionOnMap.y - 1f, -0.1f);
                        }
                        else {
                            fromPos = new Vector3(node.PositionOnMap.x,
                                node.PositionOnMap.y - 0.4f, -0.1f);
                        }
                       

                        PathNode[] ToNodes = node.ConnectedNodes.ToArray();

                        foreach (PathNode toNode in ToNodes) {
                            Vector3 toPos = new Vector3(toNode.PositionOnMap.x,
                                toNode.PositionOnMap.y + 0.4f, -0.1f);


                            LineRenderer line =  Instantiate(this.line).GetComponent<LineRenderer>();
                            line.SetPositions(new Vector3[] {
                                fromPos,
                                toPos,
                            });
                           // yield return null;
                        }
                    }
                }
            }
        }
        yield return null;
    }

    public List<PathNode> GetPathNodeAtDepth(int depth) {
        List<PathNode> pathNodes = new List<PathNode>();
        foreach (PathNode node in nodeTree[depth]) {
            if (node != null) {
                pathNodes.Add(node);
            }
        }

        return pathNodes;
    }

    
}
