using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonCreator : MonoBehaviour
{
    [Header("Room Configuration")]
    private int seed;
    [SerializeField] private int width;
    [SerializeField] private int length;
    [SerializeField] private int roomMinWidth, roomMinLength;
    [SerializeField] private int maxIterations;
    [SerializeField] private int corridorWidth;
    [SerializeField] private Material mat;

    [Range(0.0f,0.3f)]
    [SerializeField] private float roomBotCornerModifier;
    [Range(0.7f, 1f)]
    [SerializeField] private float roomTopCornerModifier;
    [Range(0f, 2f)]
    [SerializeField] private int roomOffset;

    [SerializeField] private GameObject wallVertical, wallHorizontal;
    [SerializeField] private GameObject column;

    [Header("Populating Rooms")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject buffPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject interactiblePrefab;

    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleWallVerticalPosition;

    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;

    List<Vector3Int> possibleColumnLocation;
    List<GameObject> RoomObjects = new List<GameObject>();

    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        CreateDungeon();
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();
        seed = DataPersistenceManager.instance.GetSeed();
        Random.InitState(seed);
        Debug.Log("Seed used: " + seed);


        DungeonGenerator generator = new DungeonGenerator(width, length);
        var roomList = generator.CalculateDungeon(
            maxIterations, roomMinWidth, roomMinLength, corridorWidth,
            roomBotCornerModifier, roomTopCornerModifier,
            roomOffset);

        GameObject wallParent = new GameObject("WallParent");
        wallParent.transform.parent = transform;
        GameObject colParent = new GameObject("CollumnParent");
        colParent.transform.parent = transform;
        GameObject roomParent = new GameObject("RoomParent");
        roomParent.transform.parent = transform;

        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleColumnLocation = new List<Vector3Int>();
        

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomObjects.Add(CreateMesh(roomList[i].botLeftCorner, roomList[i].topRightCorner, roomParent));
        }
        CreateWalls(wallParent, colParent);

        transform.GetComponent<NavMeshSurface>().BuildNavMesh();

        RoomNode playerSpawnRoom = generator.roomList[Random.Range(0, generator.roomList.Count)];
        player = RandomizeObjectPositionInRoom(playerPrefab, playerSpawnRoom.botLeftCorner, playerSpawnRoom.topRightCorner, 10, .3f);
        float[] position = DataPersistenceManager.instance.gameData.playerData.position;
        Vector3 convPos = new Vector3(position[0], position[1], position[2]);
        if (convPos != Vector3.zero)
        {
            player.transform.position = convPos;
        }
        if (player == null) Debug.LogError("Failed to place player");


        for (int i = 0; i < roomList.Count; i++)
        {
            PopulateRoomWithObject(buffPrefab, RoomObjects[i], Random.Range(0, 5), roomList[i], 3, .2f);
            PopulateRoomWithObject(chestPrefab, RoomObjects[i], Random.Range(0, 5), roomList[i], 3, 1f);
            PopulateRoomWithObject(enemyPrefab, RoomObjects[i], Random.Range(0, 5), roomList[i], 3, 1f);
        }

        DataPersistenceManager.instance.LoadGame();

        DataPersistenceManager.instance.UpdateSeed(seed);
    }


    private List<GameObject> PopulateRoomWithObject(GameObject prefab, GameObject parent, int amount, Node room, int attempts, float radius)
    {
        List<GameObject> placedGameObjects = new List<GameObject>();
        for (int i = 0; i < amount; i++)
        {
            Debug.Log("Placing " + prefab.name + i);
            GameObject go = RandomizeObjectPositionInRoom(prefab, room.botLeftCorner, room.topRightCorner, attempts, radius);
            if (go == null) continue;
            go.transform.parent = parent.transform;
            go.name = prefab.name + i.ToString();
            go.GetComponent<Collectible>().GenerateGuid();
            placedGameObjects.Add(go);
        }
        return placedGameObjects;
    }
    private GameObject RandomizeObjectPositionInRoom(GameObject prefab, Vector2Int botLeftRoomCorner, Vector2Int topRightRoomCorner, int maxIterations, 
        float radius)
    {
        GameObject spawnedObject = null;
        bool canSpawn = false;
        int attempts = 0;
        while(!canSpawn)
        {
            Vector3 pos;
            float randX = Random.Range(botLeftRoomCorner.x, topRightRoomCorner.x);
            float randZ = Random.Range(botLeftRoomCorner.y, topRightRoomCorner.y);

            pos = new Vector3(randX, 1f, randZ);
            canSpawn = PreventSpawnOverlap(pos, radius, .1f);
            if (canSpawn)
            {
                spawnedObject = Instantiate(prefab, pos, Quaternion.identity);
                break;
            }
            if (maxIterations > 0)//less than 0 means "tries until it works, otherwise there's no game
            {
                attempts += 1;
                if (attempts >= maxIterations) 
                {
                    Debug.Log($"Too many attemps, canceling spawning of {prefab.name} instance");
                    break; 
                }
            }
        }
        return spawnedObject;
    }
    private bool PreventSpawnOverlap(Vector3 spawnPos, float radius, float safetyMargin)
    {
        Collider[] overlappingObjects = Physics.OverlapBox(spawnPos,new Vector3(radius/2, .5f,radius/2));
        for (int i = 0; i < overlappingObjects.Length; i++)
        {
            Vector3 centerPoint = overlappingObjects[i].bounds.center;
            float width = overlappingObjects[i].bounds.extents.x;
            float length = overlappingObjects[i].bounds.extents.z;

            float leftExtent = centerPoint.x - width - safetyMargin;
            float rightExtent = centerPoint.x + width + safetyMargin;
            float forwExtent = centerPoint.z + length + safetyMargin;
            float backExtent = centerPoint.z - length - safetyMargin;

            if(spawnPos.x >= leftExtent && spawnPos.x <= rightExtent)
            {
                if(spawnPos.z >= backExtent && spawnPos.z <= forwExtent)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private void CreateWalls(GameObject wallParent, GameObject colParent)
    {
        foreach (var wallPos in possibleWallHorizontalPosition)
        {
            CreateWall(wallParent, wallPos, wallHorizontal, false, false);
        }
        foreach (var wallPos in possibleWallVerticalPosition)
        {
            CreateWall(wallParent, wallPos, wallVertical, true, false);
        }
        foreach(var colPos in possibleColumnLocation)
        {
            CreateWall(colParent, colPos, column, false, true);
        }
    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPos, GameObject wallPrefab, bool isVertical, bool isColumn)
    {
        GameObject newWall;
        if (!isColumn)
        {
            if (isVertical)
            {
                newWall = Instantiate(wallPrefab, wallPos + Vector3.up, Quaternion.Euler(-90, 90, 0), wallParent.transform);
                newWall.isStatic = true;
                newWall.transform.localScale = new Vector3(51, 51, 80);

            }
            else
            {
                newWall = Instantiate(wallPrefab, wallPos + Vector3.up, Quaternion.Euler(-90, 0, 0), wallParent.transform);
                newWall.isStatic = true;
                newWall.transform.localScale = new Vector3(51, 51, 80);
            }
        }
        else
        {
            newWall = Instantiate(wallPrefab, wallPos + Vector3.up, Quaternion.Euler(-90, 0, 0), wallParent.transform);
            newWall.isStatic = true;
            newWall.transform.localScale = new Vector3(62, 62, 80);
        }
        
    }

    private GameObject CreateMesh(Vector2 botLeftCorner, Vector2 topRightCorner, GameObject parent)
    {
        Vector3 botLeftV = new Vector3(botLeftCorner.x, 0, botLeftCorner.y);
        Vector3 botRightV = new Vector3(topRightCorner.x, 0, botLeftCorner.y);

        Vector3 topLeftV = new Vector3(botLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);


        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            botLeftV,
            botRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,1,2,
            2,1,3
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GameObject dungeonFloor = new GameObject("Mesh"+botLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = mat;
        dungeonFloor.transform.parent = transform;
        dungeonFloor.isStatic = true;
        
        dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;

        dungeonFloor.transform.parent = parent.transform;


        for (int row = (int)botLeftV.x; row <= (int)botRightV.x; row++)
        {
            var wallPos = new Vector3(row, 0, botLeftV.z);
            if(wallPos == botLeftV || wallPos == botRightV) AddWallPositionToList(wallPos, possibleColumnLocation, possibleDoorHorizontalPosition);
            else AddWallPositionToList(wallPos, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int row = (int)topLeftV.x; row <= (int)topRightCorner.x; row++)
        {
            var wallPos = new Vector3(row, 0, topRightV.z);
            if (wallPos == topLeftV || wallPos == topRightV) AddWallPositionToList(wallPos, possibleColumnLocation, possibleDoorHorizontalPosition);
            else AddWallPositionToList(wallPos, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)botLeftV.z+1; col < (int)topLeftV.z; col++)
        {
            var wallPos = new Vector3(botLeftV.x, 0, col);
            AddWallPositionToList(wallPos, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)botRightV.z+1; col < (int)topRightV.z; col++)
        {
            var wallPos = new Vector3(botRightV.x, 0, col);
            AddWallPositionToList(wallPos, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        return dungeonFloor;
    }

    private void AddWallPositionToList(Vector3 wallPos, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPos);
        if (wallList.Contains(point))
        {
            doorList.Add(point);
            wallList.Remove(point);
        }
        else
        {
            wallList.Add(point);
        }
    }

    public class DungeonGenerator
    {
        List<RoomNode> allNodesCollection = new List<RoomNode>();
        private int dungeonWidth;
        private int dungeonLength;
        public List<RoomNode> roomList;
        public DungeonGenerator(int width, int length)
        {
            this.dungeonLength = length;
            this.dungeonWidth = width;
        }
        public List<Node> CalculateDungeon(int maxIterations, int roomMinWidth, int roomMinLength, int corridorWidth,
            float roomBotCornerModifier, float roomTopCornerModifier, int roomOffset)
        {
            BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
            allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomMinWidth, roomMinLength);
            List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);

            RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomMinLength, roomMinWidth);
            roomList = roomGenerator.GenerateRoomsInGivenSpace(roomSpaces, roomBotCornerModifier, roomTopCornerModifier, roomOffset);

            CorridorGenerator corridorGenerator = new CorridorGenerator();
            var corridorList = corridorGenerator.CreateCorridors(allNodesCollection, corridorWidth);


            return new List<Node>(roomList).Concat(corridorList).ToList();
        }
        
    }
    public static class StructureHelper
    {
        public static List<Node> TraverseGraphToExtractLowestLeaves(Node parentNode)
        {
            Queue<Node> nodesToCheck = new Queue<Node>();
            List<Node> listToReturn = new List<Node>();
            if (parentNode.ChildrenNodeList.Count == 0)
            {
                return new List<Node>() { parentNode };
            }
            foreach (var child in parentNode.ChildrenNodeList)
            {
                nodesToCheck.Enqueue(child);
            }
            while (nodesToCheck.Count > 0)
            {
                var currentNode = nodesToCheck.Dequeue();
                if(currentNode.ChildrenNodeList.Count == 0)
                {
                    listToReturn.Add(currentNode);
                }
                else
                {
                    foreach (var child in currentNode.ChildrenNodeList)
                    {
                        nodesToCheck.Enqueue(child);
                    }
                }
            }
            return listToReturn;
        }
        public static Vector2Int GenerateBotLeftCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset)
        {
            int minX = boundaryLeftPoint.x + offset;
            int maxX = boundaryRightPoint.x - offset;

            int minY = boundaryLeftPoint.y + offset; ;
            int maxY = boundaryRightPoint.y + offset;

            return new Vector2Int(
                Random.Range(minX, (int)(minX + (maxX - minX) * pointModifier)),
                Random.Range(minY, (int)(minY + (maxY - minY) * pointModifier)));
        }
        public static Vector2Int GenerateTopRightCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset)
        {
            int minX = boundaryLeftPoint.x + offset;
            int maxX = boundaryRightPoint.x - offset;

            int minY = boundaryLeftPoint.y + offset; ;
            int maxY = boundaryRightPoint.y + offset;

            return new Vector2Int(
                Random.Range((int)(minX + (maxX - minX) * pointModifier), maxX),
                Random.Range((int)(minY + (maxY - minY) * pointModifier),maxY));
        }
        public static Vector2Int CalculateMiddlePoint(Vector2Int v1, Vector2Int v2)
        {
            Vector2 sum = v1 + v2;
            Vector2 tempV = sum / 2;
            return new Vector2Int((int)tempV.x, (int)tempV.y);
        }
    }
    public class RoomGenerator
    {
        private int maxIterations;
        private int roomMinLength;
        private int roomMinWidth;

        public RoomGenerator(int maxIterations, int roomMinLength, int roomMinWidth)
        {
            this.maxIterations = maxIterations;
            this.roomMinLength = roomMinLength;
            this.roomMinWidth = roomMinWidth;
        }

        public List<RoomNode> GenerateRoomsInGivenSpace(List<Node> roomSpaces, float roomBotCornerModifier, float roomTopCornerModifier, int roomOffset)
        {
            List<RoomNode> listToReturn = new List<RoomNode>();

            foreach (var space in roomSpaces)
            {
                Vector2Int newBotLeftPoint = StructureHelper.GenerateBotLeftCornerBetween(
                    space.botLeftCorner, space.topRightCorner, roomBotCornerModifier, roomOffset);
                Vector2Int newTopRightPoint = StructureHelper.GenerateTopRightCornerBetween(
                    space.botLeftCorner, space.topRightCorner, roomTopCornerModifier, roomOffset);

                space.botLeftCorner = newBotLeftPoint;
                space.topRightCorner = newTopRightPoint;
                space.botRightCorner = new Vector2Int(newTopRightPoint.x, newBotLeftPoint.y);
                space.topLeftCorner = new Vector2Int(newBotLeftPoint.x, newTopRightPoint.y);

                listToReturn.Add((RoomNode)space);
            }

            return listToReturn;
        }
    }
    public class CorridorGenerator
    {
        public List<Node> CreateCorridors(List<RoomNode> allNodesCollection, int corridorWidth)
        {
            List<Node> corridorList = new List<Node>();
            Queue<RoomNode> structuresToCheck = new Queue<RoomNode>(
                allNodesCollection.OrderByDescending(node => node.TreeLayerIndex).ToList()
                );

            while (structuresToCheck.Count > 0)
            {
                var node = structuresToCheck.Dequeue();
                if (node.ChildrenNodeList.Count == 0)
                {
                    continue;
                }
                CorridorNode corridor = new CorridorNode(node.ChildrenNodeList[0], node.ChildrenNodeList[1], corridorWidth);
                corridorList.Add(corridor);
            }

            return corridorList;
        }
    }
    public class BinarySpacePartitioner
    {
        RoomNode rootNode;
        public RoomNode RootNode { get => rootNode; }

        public BinarySpacePartitioner(int dungeonWidth, int dungeonLength)
        {
            this.rootNode = new RoomNode(new Vector2Int(0, 0), new Vector2Int(dungeonWidth, dungeonLength), null, 0);
        }

        public List<RoomNode> PrepareNodesCollection(int maxIterations, int roomMinWidth, int roomMinLength)
        {
            Queue<RoomNode> graph = new Queue<RoomNode>();
            List<RoomNode> listToReturn = new List<RoomNode>();

            graph.Enqueue(this.rootNode);
            listToReturn.Add(this.rootNode);
            int iterations = 0;
            while(iterations<maxIterations && graph.Count > 0)
            {
                iterations++;
                RoomNode currentNode = graph.Dequeue();
                if(currentNode.Width >= roomMinWidth*2 || currentNode.Length >= roomMinLength * 2)
                {
                    SplitRoomSpace(currentNode, listToReturn, roomMinLength, roomMinWidth, graph);
                }
            }
            return listToReturn;
        }

        public void SplitRoomSpace(RoomNode currentNode, List<RoomNode> listToReturn, int roomMinLength, int roomMinWidth, Queue<RoomNode> graph)
        {
            Line line = GetLineDividingSpace(currentNode.botLeftCorner, currentNode.topRightCorner, roomMinWidth, roomMinLength);
            RoomNode node1, node2;
            if (line.Orientation == Orientation.Horizontal)
            {
                node1 = new RoomNode(
                    currentNode.botLeftCorner,
                    new Vector2Int(currentNode.topRightCorner.x, line.Coordinates.y),
                    currentNode,
                    currentNode.TreeLayerIndex + 1); 
                node2 = new RoomNode(
                    new Vector2Int(currentNode.botLeftCorner.x, line.Coordinates.y),
                    currentNode.topRightCorner,
                    currentNode,
                    currentNode.TreeLayerIndex + 1);
            }
            else
            {
                node1 = new RoomNode(
                    currentNode.botLeftCorner,
                    new Vector2Int(line.Coordinates.x, currentNode.topRightCorner.y),
                    currentNode,
                    currentNode.TreeLayerIndex + 1);
                node2 = new RoomNode(
                    new Vector2Int(line.Coordinates.x, currentNode.botLeftCorner.y),
                    currentNode.topRightCorner,
                    currentNode,
                    currentNode.TreeLayerIndex + 1);
            }
            AddNewNodeToCollections(listToReturn, graph, node1);
            AddNewNodeToCollections(listToReturn, graph, node2);
        }

        public void AddNewNodeToCollections(List<RoomNode> listToReturn, Queue<RoomNode> graph, RoomNode node)
        {
            listToReturn.Add(node);
            graph.Enqueue(node);
        }

        public Line GetLineDividingSpace(Vector2Int botLeftCorner, Vector2Int topRightCorner, int roomMinWidth, int roomMinLength)
        {
            Orientation orientation;
            bool lengthStatus = (topRightCorner.y - botLeftCorner.y) >= 2 * roomMinWidth;
            bool widthStatus = (topRightCorner.x - botLeftCorner.x) >= 2 * roomMinLength;
            if(lengthStatus && widthStatus)
            {
                orientation = (Orientation)(Random.Range(0, 2));
            }else if (widthStatus)
            {
                orientation = Orientation.Vertical;
            }else
            {
                orientation = Orientation.Horizontal;
            }
            return new Line(orientation, 
                GetCoordinatesForOrientation(
                    orientation, 
                    botLeftCorner, 
                    topRightCorner, 
                    roomMinWidth, 
                    roomMinLength)
                );
        }

        public Vector2Int GetCoordinatesForOrientation(Orientation orientation, Vector2Int botLeftCorner, Vector2Int topRightCorner, int roomMinWidth, int roomMinLength)
        {
            Vector2Int coordinates = Vector2Int.zero;
            if(orientation == Orientation.Horizontal)
            {
                coordinates = new Vector2Int(0, Random.Range(botLeftCorner.y + roomMinLength, 
                                                            topRightCorner.y - roomMinLength));
            }
            else
            {
                coordinates = new Vector2Int(Random.Range(botLeftCorner.x + roomMinWidth,
                                                            topRightCorner.x - roomMinWidth), 0);
            }
            return coordinates;
        }
    }
    public class Line
    {
        Orientation orientation;
        Vector2Int coordinates;

        public Line(Orientation orientation, Vector2Int coordinates)
        {
            this.Orientation = orientation; 
            this.Coordinates = coordinates;
        }

        public Vector2Int Coordinates { get => coordinates; set => coordinates = value; }
        public Orientation Orientation { get => orientation; set => orientation = value; }
    }
    public enum Orientation
    {
        Horizontal = 0,
        Vertical = 1,
    }
    public enum RelativePosition
    {
        Up, Down, Right, Left
    }
    public class RoomNode : Node
    {
        public RoomNode(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, Node parentNode, int index) : base(parentNode)
        {
            this.botLeftCorner = bottomLeftAreaCorner;
            this.topRightCorner = topRightAreaCorner;
            this.botRightCorner = new Vector2Int(topRightAreaCorner.x, bottomLeftAreaCorner.y);
            this.topLeftCorner = new Vector2Int(bottomLeftAreaCorner.x, topRightAreaCorner.y);

            this.TreeLayerIndex = index;
        }
        public Vector3 GetMiddlePoint()
        {
            return new Vector3((botLeftCorner.x + botRightCorner.x)/2, 0, (topLeftCorner.y - botRightCorner.y)/2);
        }
        public int Width { get => (int)(topRightCorner.x - botLeftCorner.x); }
        public int Length { get => (int)(topRightCorner.y - botLeftCorner.y); }
    }
    public class CorridorNode : Node
    {
        private Node structure1;
        private Node structure2;
        private int corridorWidth;
        private int modifierDistanceFromWall = 1;

        public CorridorNode(Node node1, Node node2, int corridorWidth) : base(null)
        {
            this.structure1 = node1;
            this.structure2 = node2;
            this.corridorWidth = corridorWidth;
            GenerateCorridor();
        }

        private void GenerateCorridor()
        {
            RelativePosition relativePosOfStructure2 = CheckRelativePosition();
            switch (relativePosOfStructure2)
            {
                case RelativePosition.Up:
                    ProcessRoomVertical(this.structure1, this.structure2);
                    break;
                case RelativePosition.Down:
                    ProcessRoomVertical(this.structure2, this.structure1);
                    break;
                case RelativePosition.Right:
                    ProcessRoomHorizontal(this.structure1, this.structure2);
                    break;
                case RelativePosition.Left:
                    ProcessRoomHorizontal(this.structure2, this.structure1);
                    break;
            }
        }

        private void ProcessRoomHorizontal(Node structure1, Node structure2)
        {
            Node leftStructure = null;
            List<Node> leftStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure1);
            Node rightStructure = null;
            List<Node> rightStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure2);

            var sortedLeftStructures = leftStructureChildren.OrderByDescending(child => child.topRightCorner.x).ToList();
            if (sortedLeftStructures.Count == 1)
            {
                leftStructure = sortedLeftStructures[0];
            }
            else
            {
                int maxX = sortedLeftStructures[0].topRightCorner.x;
                sortedLeftStructures = sortedLeftStructures.Where(children => Math.Abs(maxX - children.topRightCorner.x) < 10).ToList();
                int index = Random.Range(0, sortedLeftStructures.Count);
                leftStructure = sortedLeftStructures[index];
            }

            var possibleNeighboursInRightStructureList = rightStructureChildren.Where(
                child => GetValidYForNeighbourLeftRight(
                    leftStructure.topRightCorner,
                    leftStructure.botRightCorner,
                    child.topLeftCorner,
                    child.botLeftCorner
                    )!=-1
                ).OrderBy(child => child.botRightCorner.x).ToList();
            if (possibleNeighboursInRightStructureList.Count <= 0)
            {
                rightStructure = structure2;
            }
            else
            {
                rightStructure = possibleNeighboursInRightStructureList[0];
            }
            int y = GetValidYForNeighbourLeftRight(
                leftStructure.topLeftCorner,
                leftStructure.botRightCorner,
                rightStructure.topLeftCorner,
                rightStructure.botLeftCorner);
            while (y == -1 && sortedLeftStructures.Count>0)
            {
                sortedLeftStructures = sortedLeftStructures.Where(child => child.topLeftCorner.y != leftStructure.topLeftCorner.y).ToList();
                leftStructure = sortedLeftStructures[0];
                y = GetValidYForNeighbourLeftRight(
                    leftStructure.topLeftCorner,
                    leftStructure.botRightCorner,
                    rightStructure.topLeftCorner,
                    rightStructure.botLeftCorner);
            }
            botLeftCorner = new Vector2Int(leftStructure.botRightCorner.x, y);
            topRightCorner = new Vector2Int(rightStructure.topLeftCorner.x, y + this.corridorWidth);


        }

        private int GetValidYForNeighbourLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown)
        {
            if(rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    leftNodeDown + new Vector2Int(0, modifierDistanceFromWall),
                    leftNodeUp - new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                    ).y;
            }
            if(rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <= rightNodeDown.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    rightNodeDown+new Vector2Int(0,modifierDistanceFromWall),
                    rightNodeUp-new Vector2Int(0,modifierDistanceFromWall + this.corridorWidth)
                    ).y;

            }
            if(leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    rightNodeDown+ new Vector2Int(0, modifierDistanceFromWall),
                    leftNodeUp- new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                    ).y;
            }
            if (leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
            {
                return StructureHelper.CalculateMiddlePoint(
                    leftNodeDown+ new Vector2Int(0, modifierDistanceFromWall),
                    rightNodeUp- new Vector2Int(0, modifierDistanceFromWall + this.corridorWidth)
                    ).y;
            }
            return -1;
        }

        private void ProcessRoomVertical(Node structure1, Node structure2)
        {
            Node botStructure = null;
            List<Node> structureBotChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure1);
            Node topStructure = null;
            List<Node> structureTopChildren = StructureHelper.TraverseGraphToExtractLowestLeaves(structure2);

            var sortedBotStructures = structureBotChildren.OrderByDescending(child => child.topRightCorner.y).ToList();

            if(sortedBotStructures.Count == 1)
            {
                botStructure = structureBotChildren[0];
            }
            else
            {
                int maxY = sortedBotStructures[0].topLeftCorner.y;
                sortedBotStructures = sortedBotStructures.Where(child => Mathf.Abs(maxY - child.topLeftCorner.y) < 10).ToList();
                int index = Random.Range(0, sortedBotStructures.Count);
                botStructure = sortedBotStructures[index];
            }
            var possibleNeighboursInTopStructureList = structureTopChildren.Where(
                child => GetValidXNeighbourUpDown(
                botStructure.topLeftCorner,
                botStructure.topRightCorner,
                child.botLeftCorner,
                child.botRightCorner)
                != -1).OrderBy(child => child.botRightCorner.y).ToList();
            if(possibleNeighboursInTopStructureList.Count == 0)
            {
                topStructure = structure2;
            }
            else
            {
                topStructure = possibleNeighboursInTopStructureList[0];
            }
            int x = GetValidXNeighbourUpDown(
                botStructure.topLeftCorner,
                botStructure.topRightCorner,
                topStructure.botLeftCorner,
                topStructure.botRightCorner);
            while(x==-1 && sortedBotStructures.Count > 1)
            {
                sortedBotStructures = sortedBotStructures.Where(child => child.topLeftCorner.x != topStructure.topLeftCorner.x).ToList();
                botStructure = sortedBotStructures[0];
                x = GetValidXNeighbourUpDown(
                    botStructure.topLeftCorner,
                    botStructure.topRightCorner,
                    topStructure.botLeftCorner,
                    topStructure.botRightCorner);
            }
            botLeftCorner = new Vector2Int(x, botStructure.topLeftCorner.y);
            topRightCorner = new Vector2Int(x + this.corridorWidth, topStructure.botLeftCorner.y);
        }

        private int GetValidXNeighbourUpDown(Vector2Int botNodeLeft, Vector2Int botNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight)
        {
            if(topNodeLeft.x < botNodeLeft.x && botNodeRight.x < topNodeRight.x)
            {
                return StructureHelper.CalculateMiddlePoint(
                    botNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                    botNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                    ).x;
            }
            if(topNodeLeft.x >= botNodeLeft.x && botNodeRight.x >= topNodeRight.x)
            {
                return StructureHelper.CalculateMiddlePoint(
                    topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                    topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                    ).x;
            }
            if(botNodeLeft.x >= topNodeLeft.x && botNodeLeft.x <= topNodeRight.x)
            {
                return StructureHelper.CalculateMiddlePoint(
                    botNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                    topNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                    ).x;
            }
            if(botNodeRight.x <= topNodeRight.x && botNodeRight.x >= topNodeLeft.x)
            {
                return StructureHelper.CalculateMiddlePoint(
                    topNodeLeft + new Vector2Int(modifierDistanceFromWall, 0),
                    botNodeRight - new Vector2Int(this.corridorWidth + modifierDistanceFromWall, 0)
                    ).x;
            }
            return -1;
        }

        private RelativePosition CheckRelativePosition()
        {
            Vector2 midPointStructure1Temp = ((Vector2)structure1.topRightCorner + structure1.botLeftCorner) / 2;
            Vector2 midPointStructure2Temp = ((Vector2)structure2.topRightCorner + structure2.botLeftCorner) / 2;
            float angle = CalculateAngle(midPointStructure1Temp, midPointStructure2Temp);

            if ((angle < 45f && angle >= 0) || (angle > -45 && angle < 0)) return RelativePosition.Right;
            else if ((angle > 45f && angle < 135)) return RelativePosition.Up;
            else if ((angle > -135f && angle < -45f)) return RelativePosition.Down;
            else return RelativePosition.Left;
        }

        private float CalculateAngle(Vector2 midPointStructure1Temp, Vector2 midPointStructure2Temp)
        {
            return Mathf.Atan2(midPointStructure2Temp.y - midPointStructure1Temp.y,
                midPointStructure2Temp.x - midPointStructure1Temp.x) * Mathf.Rad2Deg;
        }
    }
    public abstract class Node
    {
        private List<Node> childrenNodeList;
        public Node Parent { get; set; }
        public List<Node> ChildrenNodeList { get => childrenNodeList; }
        public bool visited { get; set; }
        public Vector2Int botLeftCorner { get; set; }
        public Vector2Int botRightCorner { get; set; }
        public Vector2Int topLeftCorner { get; set; }
        public Vector2Int topRightCorner { get; set; }

        public int TreeLayerIndex { get; set; }
        public Node(Node parentNode)
        {
            childrenNodeList = new List<Node>();
            Parent = parentNode;
            if (Parent != null)
            {
                parentNode.AddChild(this);
            }
        }

        public void AddChild(Node node)
        {
            childrenNodeList.Add(node);
        }
        public void RemoveChild(Node node)
        {
            childrenNodeList.Remove(node);
        }
    }
    private void DestroyAllChildren()
    {
        DestroyImmediate(player);
        transform.GetComponent<NavMeshSurface>().RemoveData();
        while (transform.childCount != 0)
        {
            foreach (Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}

