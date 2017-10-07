using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class Reader : MonoBehaviour {

    public TextAsset file;
    public GameObject deadBlock, startBlock, goalBlock, pathBlock;
    public GameObject car;
    private GameObject map, modelCar;
    private Node newNode;
    private ArrayList nodeList, nodes, pathNodes;
    private Queue queue;
    private Node startNode, goalNode, nextStopNode;
    public Text start, end, player, nodeText;
    private bool goalNotFound = true, startFound = false, stopCar = false;
    private int length, width;
    Transform mapTransform;
    int Lowest_f = int.MaxValue, t = 0;
    float radius = 0.05f;
    public Vector3 velocity;
	// Use this for initialization
	void Start () {
        string[] lines = System.IO.File.ReadAllLines(file.name + ".txt");
        map = GameObject.Find("map");
        mapTransform = map.transform;
        nodeList = new ArrayList();
        nodes = new ArrayList();
        length = lines.Length;
        width = lines[0].Length;
        for (int i = lines.Length - 1, k = 0; i >= 0; i--, k++) 
        {
           
           for (int j = 0; j < lines[i].Length; j++) 
           {
                char c = lines[i][j];
                switch (c) {
                    case 'X':
                        Instantiate(deadBlock, new Vector3(j - lines[i].Length / 2, -i + lines.Length / 2, -10), new Quaternion(), mapTransform);
                        newNode = new Node(k, j, "Dead", j + (k * lines[i].Length));
                        
                        break;
                    case 'S':
                        Instantiate(startBlock, new Vector3(j - lines[i].Length / 2, -i + lines.Length / 2, -10), new Quaternion(), mapTransform);
                        newNode = new Node(k, j, "Start", j + (k * lines[i].Length))
                        {
                            visited = true
                        };
                        startNode = newNode;
                        start.text = "Start Pos: " + k + "," + j;
                        break;
                    case 'G':
                        Instantiate(goalBlock, new Vector3(j - lines[i].Length / 2, -i + lines.Length / 2, -10), new Quaternion(), mapTransform);
                        newNode = new Node(k, j, "Goal", j + (k * lines[i].Length));
                        goalNode = newNode;
                        end.text = "Goal Pos: " + k + "," + j;
                        break;
                    default:
                        newNode = new Node(k, j, "Open", j + (k * lines[i].Length));
                        break;
                }
                nodes.Add(newNode);
           }
           nodeList.Add(nodes);
           nodes = new ArrayList();
        }
        AStarPathFind(nodeList);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(modelCar.transform.position.x + radius + ">" + (nextStopNode.posX + (nodeList.Count - 1) / 2));
        double relativePosX = System.Math.Round(modelCar.transform.position.y + length / 2, 2);
        double relativePosY = System.Math.Round(modelCar.transform.position.x + width / 2, 2);
        player.text = "Player Pos: " + relativePosX + ","  + relativePosY;
        nodeText.text = "Next Node: " + nextStopNode.posX + "," + nextStopNode.posY;
        int temp = 0;
        if (relativePosY - radius < nextStopNode.posY &&
            relativePosY + radius > nextStopNode.posY &&
            relativePosX - radius < nextStopNode.posX &&
            relativePosX + radius > nextStopNode.posX)
        {
            if (t == pathNodes.Count - 1)
            {
                stopCar = true;
                Debug.Log("Goal Achieved");
            }  else
            {
                Debug.Log("Next Node");
                velocity.x = ((Node)pathNodes[t + 1]).posY - ((Node)pathNodes[t]).posY;
                velocity.y = ((Node)pathNodes[t + 1]).posX - ((Node)pathNodes[t]).posX;
                velocity.z = 0;
                velocity.Normalize();
                if (velocity.x == -1)
                {
                    temp = 180;
                } else
                {
                    temp = 0;
                }
                modelCar.transform.eulerAngles = new Vector3(0, 0, (90 * velocity.y) + temp);
                nextStopNode = (Node)pathNodes[++t];
            }
        }
        if (!stopCar)
            modelCar.transform.position += velocity * 2.0f * Time.deltaTime;
	}

    void AStarPathFind(ArrayList nodesList)
    {
        queue = new Queue();
        int currentPlaceX, currentPlaceY;
        Node nextNode = startNode;
        ArrayList list;
        bool noMoreNodes = false;
        Debug.Log("Finding Path");
        while (goalNotFound && !noMoreNodes)
        {
            currentPlaceX = nextNode.posX;
            currentPlaceY = nextNode.posY;
            nextNode.visited = true;
            if (currentPlaceX != 0)
            {
                list = (ArrayList)nodeList[currentPlaceX - 1];
                CheckNextBlock((Node)list[currentPlaceY], nextNode);
            }
            if (currentPlaceX != nodeList.Count - 1)
            {
                list = (ArrayList)nodeList[currentPlaceX + 1];
                CheckNextBlock((Node)list[currentPlaceY], nextNode);
            }
            if (currentPlaceY != 0)
            {
                list = (ArrayList)nodeList[currentPlaceX];
                CheckNextBlock((Node)list[currentPlaceY - 1], nextNode);
            }
            
            if (currentPlaceY != ((ArrayList)nodesList[currentPlaceX]).Count - 1)
            {
                list = (ArrayList)nodeList[currentPlaceX];
                CheckNextBlock((Node)list[currentPlaceY + 1], nextNode);
            }
            if (queue.Count == 0)
            {
                Lowest_f = int.MaxValue;
                noMoreNodes = true;
                for (int i = 0; i < nodeList.Count; i++)
                {
                    for (int j = 0; j < ((ArrayList)nodeList[i]).Count; j++)
                    {
                        Node tempNode = (Node)((ArrayList)nodeList[i])[j];
                        if (!tempNode.visited && tempNode._f != 0)
                        {
                            if (tempNode._f < Lowest_f)
                            {
                                queue.Clear();
                                Lowest_f = tempNode._f;
                                queue.Enqueue(tempNode);
                                noMoreNodes = false;
                            }
                            else if (tempNode._f == Lowest_f)
                            {
                                queue.Enqueue(tempNode);
                                noMoreNodes = false;
                            }
                        }
                    }
                }
            }
            if (!noMoreNodes)
                nextNode = (Node)queue.Dequeue();
        }
        if (!noMoreNodes)
        {
            Debug.Log("Path Found!");
            pathNodes = new ArrayList();
            PathToStart(goalNode);
            GenerateCarPath();
        } else
        {
            Debug.Log("No Path Can be Found");
        }
        
    }

    void CheckNextBlock(Node node, Node parent)
    {
        
        if (node.nodeName == "Goal")
        {
            goalNotFound = false;
            Debug.Log("Goal Found!!!!!");
            node.parentNode = parent;
            return;
        }
        if (node.nodeName == "Dead" || node.visited)
            return;
        node.parentNode = parent;
        foreach (Node n in queue)
        {
            if (n._ID == node._ID)
            {
                return;
            }
        }

        //Debug.Log(node.posX + " " + node.posY);
        int distanceX = Mathf.Abs(goalNode.posX - node.posX);
        int distanceY = Mathf.Abs(goalNode.posY - node.posY);
        
        node._h = distanceX + distanceY;
        node._g += parent._g + 1;

        node._f = node._h + node._g;
        
        if (node._f < Lowest_f) {
            foreach (Node n in queue)
            {
                n.visited = false;
            }
            queue.Clear();
            Lowest_f = node._f;
            queue.Enqueue(node);
        }
        else if (node._f == Lowest_f)
        {
            queue.Enqueue(node);
        }
    }
    void PathToStart(Node nextNode)
    {
        
        if (!startFound)
        {
            pathNodes.Add(nextNode);
            if (nextNode.nodeName == "Start")
            {
                startFound = true;
                pathNodes.Reverse();
                return;
            }
            Instantiate(pathBlock, new Vector3(nextNode.posY - ((ArrayList)nodeList[0]).Count / 2, nextNode.posX - nodeList.Count / 2, -10), new Quaternion(), mapTransform);
            PathToStart(nextNode.parentNode);
        }
    }

    void GenerateCarPath()
    {
        modelCar = (GameObject)Instantiate(car, new Vector3(startNode.posY - ((ArrayList)nodeList[0]).Count / 2, startNode.posX - nodeList.Count / 2, -10), new Quaternion());
        velocity.x = ((Node)pathNodes[t + 1]).posY - ((Node)pathNodes[t]).posY;
        velocity.y = ((Node)pathNodes[t + 1]).posX - ((Node)pathNodes[t]).posX;
        velocity.z = 0;
        velocity.Normalize();
        nextStopNode = (Node)pathNodes[++t];
    }
}


