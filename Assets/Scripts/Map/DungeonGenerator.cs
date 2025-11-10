using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
        public int ownerId = -1; // id of the room occupying this cell, -1 = none
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;
        [Tooltip("Room footprint in grid cells (width x height). Default 1x1")]
        public Vector2Int roomSize = new Vector2Int(1,1);

        public bool obligatory;

        public int ProbabilityOfSpawning(int x, int y)
        {
            // 0 - cannot spawn 1 - can spawn 2 - HAS to spawn

            if (x>= minPosition.x && x<=maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
            {
                return obligatory ? 2 : 1;
            }

            return 0;
        }

    }

    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public Vector2 offset;

    public GameObject corridorPrefab; // prefab for corridor between rooms (assign in Inspector)

    List<Cell> board;

    // Start is called before the first frame update
    void Start()
    {
        MazeGenerator();
    }

    void GenerateDungeon()
    {

        int placedRoomCounter = 0; // to assign ownerId

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Cell currentCell = board[(i + j * size.x)];
                // Only attempt to place a room on visited cells that are not already occupied by a previously placed larger room
                if (currentCell.visited && currentCell.ownerId == -1)
                {
                    int randomRoom = -1;
                    List<int> availableRooms = new List<int>();

                    for (int k = 0; k < rooms.Length; k++)
                    {
                        int p = rooms[k].ProbabilityOfSpawning(i, j);

                        if(p == 2)
                        {
                            randomRoom = k;
                            break;
                        } else if (p == 1)
                        {
                            availableRooms.Add(k);
                        }
                    }

                    if(randomRoom == -1)
                    {
                        if (availableRooms.Count > 0)
                        {
                            randomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                        }
                        else
                        {
                            randomRoom = 0;
                        }
                    }

                    // Try to place the chosen room, but ensure its footprint (roomSize) fits within bounds and doesn't overlap existing placed rooms
                    Rule chosenRule = rooms[randomRoom];
                    Vector2Int rSize = chosenRule.roomSize;

                    // If the chosen room doesn't fit (goes out of bounds or overlaps occupied cells), try to find a fallback room that fits
                    bool fits = true;
                    if (i + rSize.x > size.x || j + rSize.y > size.y)
                        fits = false;
                    else
                    {
                        for (int rx = 0; rx < rSize.x && fits; rx++)
                        {
                            for (int ry = 0; ry < rSize.y; ry++)
                            {
                                var c = board[(i + rx) + (j + ry) * size.x];
                                if (c.ownerId != -1) { fits = false; break; }
                            }
                        }
                    }

                    if (!fits)
                    {
                        int found = -1;
                        for (int a = 0; a < availableRooms.Count; a++)
                        {
                            var alt = rooms[availableRooms[a]];
                            var altSize = alt.roomSize;
                            bool altFits = true;
                            if (i + altSize.x > size.x || j + altSize.y > size.y)
                                altFits = false;
                            else
                            {
                                for (int rx = 0; rx < altSize.x && altFits; rx++)
                                {
                                    for (int ry = 0; ry < altSize.y; ry++)
                                    {
                                        var c = board[(i + rx) + (j + ry) * size.x];
                                        if (c.ownerId != -1) { altFits = false; break; }
                                    }
                                }
                            }
                            if (altFits) { found = availableRooms[a]; break; }
                        }
                        if (found != -1)
                        {
                            randomRoom = found;
                            chosenRule = rooms[randomRoom];
                            rSize = chosenRule.roomSize;
                            fits = true;
                        }
                    }

                    // If still doesn't fit, fallback to a 1x1 placement at this cell (use room 0 if necessary)
                    if (!fits)
                    {
                        // Try to find any room that is 1x1
                        int oneByOne = -1;
                        for (int rr = 0; rr < rooms.Length; rr++) if (rooms[rr].roomSize.x == 1 && rooms[rr].roomSize.y == 1) { oneByOne = rr; break; }
                        if (oneByOne != -1)
                        {
                            randomRoom = oneByOne;
                            chosenRule = rooms[randomRoom];
                            rSize = chosenRule.roomSize;
                            fits = true;
                        }
                        else
                        {
                            // force fallback to single cell using rooms[randomRoom] but will occupy only this cell
                            rSize = new Vector2Int(1,1);
                            fits = true;
                        }
                    }

                    // Now place the room occupying rSize cells anchored at (i,j) (top-left corner)
                    var newRoom = Instantiate(chosenRule.room, new Vector3((i + (rSize.x - 1) * 0.5f) * offset.x, 0, -(j + (rSize.y - 1) * 0.5f) * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                    // compute aggregated status for the whole footprint so the room knows where doors should be
                    bool[] aggStatus = new bool[4];
                    // Up (0): check cells along top edge (x in i..i+rSize.x-1) for any status[0] that goes outside footprint
                    for (int rx = 0; rx < rSize.x; rx++)
                    {
                        int cx = i + rx; int cy = j;
                        var cell = board[cx + cy * size.x];
                        if (cell.status.Length > 0 && cell.status[0])
                        {
                            // neighbor above is at (cx, cy-1); if it's outside the room footprint, it's a connection
                            int nbx = cx; int nby = cy - 1;
                            bool neighborInside = (nbx >= i && nbx < i + rSize.x && nby >= j && nby < j + rSize.y);
                            if (!neighborInside) aggStatus[0] = true;
                        }
                    }
                    // Down (1): bottom edge
                    for (int rx = 0; rx < rSize.x; rx++)
                    {
                        int cx = i + rx; int cy = j + rSize.y - 1;
                        var cell = board[cx + cy * size.x];
                        if (cell.status.Length > 1 && cell.status[1])
                        {
                            int nbx = cx; int nby = cy + 1;
                            bool neighborInside = (nbx >= i && nbx < i + rSize.x && nby >= j && nby < j + rSize.y);
                            if (!neighborInside) aggStatus[1] = true;
                        }
                    }
                    // Right (2): right edge
                    for (int ry = 0; ry < rSize.y; ry++)
                    {
                        int cx = i + rSize.x - 1; int cy = j + ry;
                        var cell = board[cx + cy * size.x];
                        if (cell.status.Length > 2 && cell.status[2])
                        {
                            int nbx = cx + 1; int nby = cy;
                            bool neighborInside = (nbx >= i && nbx < i + rSize.x && nby >= j && nby < j + rSize.y);
                            if (!neighborInside) aggStatus[2] = true;
                        }
                    }
                    // Left (3): left edge
                    for (int ry = 0; ry < rSize.y; ry++)
                    {
                        int cx = i; int cy = j + ry;
                        var cell = board[cx + cy * size.x];
                        if (cell.status.Length > 3 && cell.status[3])
                        {
                            int nbx = cx - 1; int nby = cy;
                            bool neighborInside = (nbx >= i && nbx < i + rSize.x && nby >= j && nby < j + rSize.y);
                            if (!neighborInside) aggStatus[3] = true;
                        }
                    }

                    newRoom.UpdateRoom(aggStatus);
                    newRoom.name += " " + i + "-" + j;

                    // mark footprint cells as owned by this placed room
                    for (int rx = 0; rx < rSize.x; rx++)
                    {
                        for (int ry = 0; ry < rSize.y; ry++)
                        {
                            board[(i + rx) + (j + ry) * size.x].ownerId = placedRoomCounter;
                        }
                    }
                    placedRoomCounter++;

                    // continue to next iteration
                    continue;
                }
            }
        }

        // After placing rooms, instantiate corridors but skip corridors between cells that belong to same room
        if (corridorPrefab != null)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Cell currentCell = board[i + j * size.x];
                    if (!currentCell.visited) continue;

                    // Right corridor
                    if (currentCell.status.Length > 2 && currentCell.status[2])
                    {
                        int ni = i + 1; int nj = j;
                        if (ni < size.x)
                        {
                            Cell neighbor = board[ni + nj * size.x];
                            // skip corridor if both cells are owned by same room (and ownerId != -1)
                            if (!(currentCell.ownerId != -1 && currentCell.ownerId == neighbor.ownerId))
                            {
                                Vector3 a = new Vector3(i * offset.x, 0, -j * offset.y);
                                Vector3 b = new Vector3(ni * offset.x, 0, -nj * offset.y);
                                Vector3 mid = (a + b) * 0.5f;
                                var corridor = Instantiate(corridorPrefab, mid, Quaternion.identity, transform);
                                corridor.name = $"Corridor {i}-{j}_to_{ni}-{nj}";
                                corridor.transform.LookAt(b);
                            }
                        }
                    }

                    // Down corridor
                    if (currentCell.status.Length > 1 && currentCell.status[1])
                    {
                        int ni = i; int nj = j + 1;
                        if (nj < size.y)
                        {
                            Cell neighbor = board[ni + nj * size.x];
                            if (!(currentCell.ownerId != -1 && currentCell.ownerId == neighbor.ownerId))
                            {
                                Vector3 a = new Vector3(i * offset.x, 0, -j * offset.y);
                                Vector3 b = new Vector3(ni * offset.x, 0, -nj * offset.y);
                                Vector3 mid = (a + b) * 0.5f;
                                var corridor = Instantiate(corridorPrefab, mid, Quaternion.identity, transform);
                                corridor.name = $"Corridor {i}-{j}_to_{ni}-{nj}";
                                corridor.transform.LookAt(b);
                            }
                        }
                    }
                }
            }
        }

    }

    void MazeGenerator()
    {
        board = new List<Cell>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }

        int currentCell = startPos;

        Stack<int> path = new Stack<int>();

        int k = 0;

        while (k<1000)
        {
            k++;

            board[currentCell].visited = true;

            if(currentCell == board.Count - 1)
            {
                break;
            }

            //Check the cell's neighbors
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
            else
            {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell)
                {
                    //down or right
                    if (newCell - 1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    //up or left
                    if (newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }

            }

        }
        GenerateDungeon();
    }

    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        //check up neighbor
        if (cell - size.x >= 0 && !board[(cell-size.x)].visited)
        {
            neighbors.Add((cell - size.x));
        }

        //check down neighbor
        if (cell + size.x < board.Count && !board[(cell + size.x)].visited)
        {
            neighbors.Add((cell + size.x));
        }

        //check right neighbor
        if ((cell+1) % size.x != 0 && !board[(cell +1)].visited)
        {
            neighbors.Add((cell +1));
        }

        //check left neighbor
        if (cell % size.x != 0 && !board[(cell - 1)].visited)
        {
            neighbors.Add((cell -1));
        }

        return neighbors;
    }
}
