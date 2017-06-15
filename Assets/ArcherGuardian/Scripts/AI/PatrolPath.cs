using UnityEngine;
using System.Collections.Generic;

public class PatrolPath : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right
    }

    // Initial direction of this path
    public Direction direction = Direction.Right;

    public List<Transform> waypoints;

    // weather or not to connect start and end point togeter
    public bool closedPath = false;

    // current direction indicator
    private bool _goingRight;

    private int _currentIndex = 0;

    void Awake()
    {
        _goingRight = (direction == Direction.Right);
    }

    // property to get the current waypoint if it exists
    public Transform currentWaypoint
    {
        get
        {
            if (waypoints.Count > 0)
                return waypoints[_currentIndex];

            return null;
        }
    }

    // proerty to get the last waypoint if it exists
    public Transform lastWaypoint
    {
        get
        {
            if (waypoints.Count > 0)
                return waypoints[waypoints.Count - 1];

            return null;
        }
    }

    // adds a new waypoint
    public void AddWaypoint()
    {
        // create a new gameobject and call it waypoint 
        // note: this will not generate unique names.
        GameObject go = new GameObject("Waypoint " + waypoints.Count);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;

        if (waypoints.Count > 0)
        {
            Vector3 direction = Vector3.forward;
            // get direction of last edge
            if (waypoints.Count > 1)
            {
                Vector3 from = waypoints[waypoints.Count - 2].position;
                Vector3 to = lastWaypoint.position;
                direction = (to - from).normalized;
            }

            go.transform.position = lastWaypoint.position + direction * 0.5f;
        }

        // Add a waypoint component to the gameobject and add it's reference to our list
        waypoints.Add(go.transform);
    }

    public void Next()
    {
        IncrementIndex();
    }

    // increment the index basd on closePath etc.
    void IncrementIndex()
    {
        if (!closedPath &&
                (_goingRight && _currentIndex == waypoints.Count - 1
                || !_goingRight && _currentIndex == 0))
        {
            _goingRight = !_goingRight;
        }

        if (_goingRight) _currentIndex++;
        else _currentIndex--;

        while (_currentIndex < 0)
            _currentIndex += waypoints.Count;

        _currentIndex %= waypoints.Count;
    }
    /*
    // visualize the current path
    void OnDrawGizmos()
    {
        if (waypoints.Count < 1)
            return;

        Vector3 prevPos = lastWaypoint.position;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 pos = waypoints[i].position;

            if (i > 0 || waypoints.Count > 2 && closedPath)
                EditorDrawPathEdge(prevPos, pos);

            prevPos = pos;
        }
    }

    // draw a connection line with a directional arrow
    void EditorDrawPathEdge(Vector3 from, Vector3 to)
    {
        Gizmos.color = Color.magenta;
        UnityEditor.Handles.color = Color.magenta;

        Gizmos.DrawLine(from, to);

        // draw arrow handle in the center of the line
        Vector3 lineDir = (to - from).normalized;
        Vector3 arrowPos = from;
        float arrowLength = 1.5f;

        if (direction == Direction.Left)
        {
            lineDir = (from - to).normalized;
            arrowPos = to;
        }
        float distance = Vector3.Distance(from, to);

        // scale the arrow down if we can't fit the desired arrowLength on the current edge
        if (distance * 0.5f >= arrowLength)
            arrowPos += (0.5f * distance - arrowLength) * lineDir;
        else
            arrowLength = 0.5f * distance;

        float angle = Vector3.Angle(lineDir, Vector3.forward);
        Vector3 axis = Vector3.Cross(lineDir, Vector3.forward);
        angle *= (Vector3.Dot(lineDir, axis) > 0) ? 1.0f : -1.0f;
        UnityEditor.Handles.ArrowCap(0, arrowPos, Quaternion.AngleAxis(angle, axis), arrowLength);
    }*/
}
