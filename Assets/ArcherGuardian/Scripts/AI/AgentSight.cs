using CpvrLab.ArcherGuardian.Scripts.PlayersAndIO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// The AgentSight class simulates the agent's sight by using a sphere
/// collider to determine if the player is closer than the colliders radius.
/// If so it checks whether the player is inside the view cone defined by the 
/// look direction and a view angle. If so it checks whether the player is 
/// actually visible by casting a ray to the player. If we survived all tests 
/// we have direct sight to the player and we can call the event handler 
/// playerSpottet with the player’s position.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class AgentSight : MonoBehaviour
{
    public delegate void PlayerSpottedDelegate(Vector3 position);
    public event PlayerSpottedDelegate playerSpotted;
    
    [Range(0, 360)]
    public float viewAngle = 70;

    [Range(0, 100)]
    public float viewRadius = 10.0f;
    
    // Reference to the sphere collider trigger
    private SphereCollider _sightCollider;

    // Current head look vector
    private Vector3 _headLook = Vector3.forward;

    // Player in sight flag
    private bool _playerInSight = false;
    public bool playerInSight
    {
        get { return _playerInSight; }
    }

    // Last world position the player was seen at
    private Vector3 _lastPlayerSighting;
    public Vector3 lastPlayerSighting {get {return _lastPlayerSighting;}}

    // Current look at direction in world space
    public Vector3 headLookWS {get {return transform.rotation * _headLook;}}

    private GameObject _player;

    void Awake()
    {
        _sightCollider = GetComponent<SphereCollider>();
        _sightCollider.isTrigger = true;
        _sightCollider.radius = viewRadius;

        var archer = FindObjectOfType<Archer>();
        if (archer != null) _player = archer.gameObject;
    }


    /// <summary>
    /// Lets the _headLook vector point to the given position in world space
    /// </summary>
    public void LookAtPosition(Vector3 positionWS)
    {
        // transform from world to local space and normalize
        _headLook = transform.InverseTransformPoint(positionWS).normalized;
    }

    /// <summary>
    /// Resets the local _headLook vector to the positive z-axis
    /// </summary>
    public void LookForward()
    {
        _headLook = Vector3.forward;

        // Sync the collider radius with the view radius in case of change
        if (_sightCollider.radius != viewRadius)
            _sightCollider.radius = viewRadius;
    }
        
    // Gets called whenever another collider collides or is inside the sphere collider.
    void OnTriggerStay(Collider other)
    {
        if (_player == null) return;

        Vector3 agentPosition = transform.position;
        agentPosition.y = 1f;

        // Early out for everything other than the player object
        //if (other.gameObject.name != _player.name) return;
        if (other.gameObject.GetInstanceID() != _player.GetInstanceID()) return;
            

        _playerInSight = false;

        // Calculate the angle between the agent's look vector & the player's position
        Vector3 direction = other.transform.position - agentPosition;
        float angle = Vector3.Angle(direction, headLookWS);

        // Return if the player is outside of our view angle
        if (angle > (0.5f * viewAngle)) return;

        // If the player is inside our view cone, test for occlusion by shooting a ray
        RaycastHit hit;        
        if (Physics.Raycast(agentPosition, direction.normalized, out hit, _sightCollider.radius))
        {
            if (hit.collider.gameObject.GetInstanceID() != _player.GetInstanceID())
            {
                // visualize the occluded ray
                Debug.DrawLine(agentPosition, hit.point, Color.red);
                return;
            }

            // visualize the ray that hit the player
            Debug.DrawLine(agentPosition, hit.point, Color.green);
            
            _lastPlayerSighting = other.transform.position;
            _playerInSight = true;

            // invoke the callback if it has subscribers
            if (playerSpotted != null)
                playerSpotted(other.transform.position);
        }
    }

    // Gets called when another collider exits the sphere collider
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetInstanceID() != _player.GetInstanceID())
            return;

        _playerInSight = false;
    }
}
