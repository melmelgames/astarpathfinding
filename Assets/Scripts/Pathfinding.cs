using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{

    public Transform seeker;

    public Transform target;
    Grid grid;

    private void Awake(){
        grid = GetComponent<Grid>();
    }

    private void Update(){
        if(Input.GetButtonDown("Jump")){
            FindPath(seeker.position, target.position);
        }
    }
    private void FindPath(Vector3 startPos, Vector3 targetPos){
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        //List<Node> openSet = new List<Node>();
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count() > 0){
            /*
            Node currentNode = openSet[0];
            for(int i = 1; i < openSet.Count; i++){
                if(openSet[i].fCost < currentNode.fCost | openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost){
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            */
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if(currentNode == targetNode){
                sw.Stop();
                print("Path found in " + sw.ElapsedMilliseconds + " ms.");
                RetracePath(startNode, targetNode);
                return;
            }

            foreach(Node neighbor in grid.GetNeighbors(currentNode)){
                if(!neighbor.walkable || closedSet.Contains(neighbor)){
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)){
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor)){
                        openSet.Add(neighbor);
                    }
                }
            }

        }

    }

    private void RetracePath(Node startNode, Node endNode){
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode){
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        grid.path = path;

    }

    private int GetDistance(Node nodeA, Node nodeB){
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(distX > distY){
            return 14 * distY + 10 * (distX - distY);
        }else{
            return 14 * distX + 10 * (distY - distX); 
        }
    }
}
