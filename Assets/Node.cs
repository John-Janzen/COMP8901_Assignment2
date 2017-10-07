using UnityEngine;
using System.Collections;

public class Node {

    public Node parentNode;
    public int _f = 0, _g, _h, _ID, distance = 0;
    public int posX, posY;
    public string nodeName;
    public bool visited = false;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public Node(int x, int y, string s, int id)
    {
        posX = x; posY = y;
        nodeName = s;
        _ID = id;
    }
}
