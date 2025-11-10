using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Direction
{
    Up = 0,
    Down = 1,
    Right = 2,
    Left = 3,
    // Extendable if needed
}

[System.Serializable]
public class WallDoorEntry
{
    public Direction direction;
    [Tooltip("The wall GameObject for this direction (will be activated when there is NO connection)")]
    public GameObject wall;
    [Tooltip("The door GameObject for this direction (will be activated when there IS a connection)")]
    public GameObject door;
}

public class RoomBehaviour : MonoBehaviour
{
    [Tooltip("Configure wall/door pairs freely in the inspector and assign a Direction for each entry.")]
    public List<WallDoorEntry> entries = new List<WallDoorEntry>();

    /// <summary>
    /// Update room visuals according to status array.
    /// status is expected to follow the convention: index 0=Up, 1=Down, 2=Right, 3=Left.
    /// This method is robust: if status is null or shorter than required, missing values are treated as false.
    /// </summary>
    /// <param name="status">bool[] status indicating whether there is a connection in each direction</param>
    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            int idx = (int)entry.direction;
            bool connected = false;
            if (status != null && idx >= 0 && idx < status.Length)
            {
                connected = status[idx];
            }

            if (entry.door != null)
                entry.door.SetActive(connected);

            if (entry.wall != null)
                entry.wall.SetActive(!connected);
        }
    }

    // Backwards-compatible helper if someone still wants to use arrays (kept for convenience)
    public void UpdateRoomFromArrays(GameObject[] walls, GameObject[] doors, bool[] status)
    {
        // If arrays provided, update according to conventional indices
        for (int d = 0; d < 4; d++)
        {
            bool connected = (status != null && d < status.Length) ? status[d] : false;
            if (doors != null && d < doors.Length && doors[d] != null)
                doors[d].SetActive(connected);
            if (walls != null && d < walls.Length && walls[d] != null)
                walls[d].SetActive(!connected);
        }
    }
}
