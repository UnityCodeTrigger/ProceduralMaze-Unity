using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 class MazeNode
{
    public int x, y;
    public bool isVisited = false;
    public bool[] walls = { false, false, false, false };  //Top - Down - Right - Left
    public GameObject nodeObject;

    public MazeNode(int _x, int _y,GameObject _nodeObject, bool top, bool down, bool right, bool left)
    {
        x = _x;
        y = _y;

        bool[] _walls = { top, down, right, left };
        walls = _walls;

        nodeObject = _nodeObject;

        SetupWalls();
    }

    public void SetupWalls()
    {
        if (walls[0])
            nodeObject.transform.GetChild(0).gameObject.SetActive(true);
        if (walls[1])
            nodeObject.transform.GetChild(1).gameObject.SetActive(true);
        if (walls[2])
            nodeObject.transform.GetChild(2).gameObject.SetActive(true);
        if (walls[3])
            nodeObject.transform.GetChild(3).gameObject.SetActive(true);
    }
}

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width;
    public int height;

    [Header("Maze Node")]
    public GameObject mazeNodePrefab;
    public Material currentMaterial;
    public Material visitedMaterial;

    [Header("Debug")]
    public bool mazeFinished = false;
    public int steps = 0;

    MazeNode[,] mazeNodes;
    MazeNode currentNode;
    Queue<MazeNode> stack = new Queue<MazeNode>();

    float time;

    private void Start()
    {
        GenerateMazeNodes();
        CheckNeighbors();
    }

    private void Update()
    {
        if (mazeFinished)
            return;

        time += Time.deltaTime;
        if (time > 0.001f)
        {
            time = 0;
            CheckNeighbors();
        }
    }

    void GenerateMazeNodes()
    {
        mazeNodes = new MazeNode[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject newNodeObject = Instantiate(mazeNodePrefab);
                mazeNodes[x, y] = new MazeNode(x,y,newNodeObject,true,true,true,true);

                mazeNodes[x, y].nodeObject.transform.position = new Vector3(x, y, 0);
                mazeNodes[x, y].nodeObject.transform.parent = transform;
            }
        }
        currentNode = mazeNodes[0, 0];
        currentNode.isVisited = true;

        RefreshMazeNodes();
    }

    void RefreshMazeNodes()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                if (mazeNodes[x, y].isVisited)
                    ChangeNodeMaterial(visitedMaterial, x, y);

                if (mazeNodes[x, y] == currentNode)
                    ChangeNodeMaterial(currentMaterial, x, y);

                Transform nodeTrans = mazeNodes[x, y].nodeObject.transform;
                nodeTrans.GetChild(0).gameObject.SetActive(mazeNodes[x, y].walls[0]);
                nodeTrans.GetChild(1).gameObject.SetActive(mazeNodes[x, y].walls[1]);
                nodeTrans.GetChild(2).gameObject.SetActive(mazeNodes[x, y].walls[2]);
                nodeTrans.GetChild(3).gameObject.SetActive(mazeNodes[x, y].walls[3]);
            }
        }

    }

    void CheckNeighbors()
    {
        //Check if is visited or is inside grid
        int x, y;
        x = currentNode.x;
        y = currentNode.y;

        List<MazeNode> avaliableNeighbors = new List<MazeNode>();


        if (y + 1 < height)
            if (!mazeNodes[x, y + 1].isVisited)
                avaliableNeighbors.Add(mazeNodes[x, y + 1]);

        if (y - 1 >= 0)
            if (!mazeNodes[x, y - 1].isVisited)
                avaliableNeighbors.Add(mazeNodes[x, y - 1]);

        if (x + 1 < width)
            if (!mazeNodes[x + 1, y].isVisited)
                avaliableNeighbors.Add(mazeNodes[x + 1, y]);

        if(x - 1 >= 0)
            if (!mazeNodes[x - 1, y].isVisited)
                avaliableNeighbors.Add(mazeNodes[x - 1, y]);

        if (avaliableNeighbors.Count > 0)
        {
            int r = Random.Range(0, avaliableNeighbors.Count);

            stack.Enqueue(avaliableNeighbors[r]);

            MoveCurrentNode(avaliableNeighbors[r].x, avaliableNeighbors[r].y);
        }
        else
        {
            if (stack.Count > 0)
                currentNode = stack.Dequeue();
            else
            {
                currentNode = mazeNodes[0, 0];
                mazeFinished = true;
            }
        }

        RefreshMazeNodes();
    }

    void MoveCurrentNode(int x, int y)
    {
        int xDif = x - currentNode.x;
        int yDif = y - currentNode.y;

        if (yDif > 0)
        {
            currentNode.walls[0] = false;
            mazeNodes[x, y].walls[1] = false;
        }
        else if (yDif < 0)
        {
            currentNode.walls[1] = false;
            mazeNodes[x, y].walls[0] = false;
        }
        if (xDif > 0)
        {
            currentNode.walls[2] = false;
            mazeNodes[x, y].walls[3] = false;
        }
        else if (xDif < 0)
        {
            currentNode.walls[3] = false;
            mazeNodes[x, y].walls[2] = false;
        }

        mazeNodes[x, y].isVisited = true;
        mazeNodes[x, y].nodeObject.transform.GetChild(4).gameObject.SetActive(false);
        currentNode = mazeNodes[x,y];
        steps++;
    }

    void ChangeNodeMaterial(Material mat,int x, int y)
    {
        for (int i = 0; i < mazeNodes[x,y].nodeObject.transform.childCount; i++)
        {
            mazeNodes[x, y].nodeObject.transform.GetChild(i).GetComponent<MeshRenderer>().material = mat;

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(width * 0.5f, height * 0.5f), new Vector3(width,height));

        if (currentNode != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(currentNode.x, currentNode.y), 0.25f);
        }
    }

}
