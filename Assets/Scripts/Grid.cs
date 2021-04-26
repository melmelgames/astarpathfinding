using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool diaplayGridGizmos;
    public int blurWeightStrenght;
    public int obstacleProximityPenalty = 30;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;
    private LayerMask walkableMask;
    private Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    private int penaltyMax = int.MinValue;
    private int penaltyMin = int.MaxValue;

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

                
                Ray ray= new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if(Physics.Raycast(ray.origin, ray.direction, out hit, 100, walkableMask)){
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }
                if(!walkable){
                    movementPenalty += obstacleProximityPenalty;
                }


                grid[x,y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }
        BlurPenaltyMap(blurWeightStrenght);
    }

    private void BlurPenaltyMap(int blurSize){
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1)/2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for(int y = 0; y < gridSizeY; y++){
            for(int x= -kernelExtents; x <= kernelExtents; x++){
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0,y] += grid[sampleX, y].movementPenalty;
            }
            for (int x = 1; x < gridSizeX; x++){
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);
                penaltiesHorizontalPass[x,y] = penaltiesHorizontalPass[x-1,y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for(int x = 0; x < gridSizeX; x++){
            for(int y= -kernelExtents; y <= kernelExtents; y++){
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass [x,0] / (kernelSize * kernelSize));
            grid[x,0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++){
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);
                penaltiesVerticalPass[x,y] = penaltiesVerticalPass[x,y-1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass [x,y] / (kernelSize * kernelSize));
                grid[x,y].movementPenalty = blurredPenalty;

                if(blurredPenalty > penaltyMax){
                    penaltyMax = blurredPenalty;
                }
                if(blurredPenalty < penaltyMin){
                    penaltyMin = blurredPenalty;
                }
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
            if(grid != null && diaplayGridGizmos){
                foreach(Node n in grid){
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                    Gizmos.color = (n.walkable)?Gizmos.color:Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
                }
            }
    }
    [System.Serializable]
    public class TerrainType{
        public LayerMask TerrainMask;
        public int TerrainPenalty;
    }
}
