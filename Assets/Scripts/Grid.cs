using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool diaplayFridGizmos;
    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    private LayerMask walkableMask;
    private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    public LayerMask unwalkableMask;

    private void Awake(){
        nodeDiameter = 2 * nodeRadius;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);

        foreach(TerrainType region in walkableRegions){
            walkableMask.value |= region.TerrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.TerrainMask.value, 2), region.TerrainPenalty);
        }

        CreateGrid();

    }

    public int MaxSize{
        get{
            return gridSizeX * gridSizeY;
        }
    }
    private void CreateGrid(){
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right*gridWorldSize.x/2 - Vector3.forward*gridWorldSize.y/2;

        for(int x = 0; x < gridSizeX; x++){
            for(int y = 0; y < gridSizeY; y++){
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                if(walkable){
                    Ray ray= new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if(Physics.Raycast(ray.origin, ray.direction, out hit, 100, walkableMask)){
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }
                }


                grid[x,y] = new Node(walkable, worldPoint, x, y, movementPenalty);
        }

        }
    }

    public List<Node> GetNeighbors(Node node){
        List<Node> neighbors = new List<Node>();
        for(int x = -1; x <= 1; x++){
            for(int y = -1; y <= 1; y++){
                if(x == 0 && y == 0){
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY){
                    neighbors.Add(grid[checkX,checkY]);

                }

            
            }
        }
        return neighbors;

    }

    public Node NodeFromWorldPoint(Vector3 worldPosition){
        float percentX = (worldPosition.x/gridWorldSize.x) + 0.5f;
        float percentY = (worldPosition.z/gridWorldSize.y) + 0.5f;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        
        return grid[x,y];
    }


    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            if(grid != null && diaplayFridGizmos){
                foreach(Node n in grid){
                    Gizmos.color = (n.walkable)?Color.white:Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-0.1f));
                }
            }
    }
    [System.Serializable]
    public class TerrainType{
        public LayerMask TerrainMask;
        public int TerrainPenalty;
    }
}
