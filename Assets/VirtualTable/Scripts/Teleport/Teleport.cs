using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour {


    public bool teleportOnClick = false;
    public LayerMask teleportLayers;
    public PlayAreaVis playAreaVisPrefab;

    public Color legalAreaColor = Color.green;
    public Color illegalAreaColor = Color.red;
    
    private PlayAreaVis _playAreaVis;
    private SteamVR_TrackedController _trackedController;

    Transform reference
    {
        get
        {
            var top = SteamVR_Render.Top();
            return (top != null) ? top.origin : null;
        }
    }

    void Start()
    {
        _trackedController = GetComponent<SteamVR_TrackedController>();
        if(_trackedController == null) {
            _trackedController = gameObject.AddComponent<SteamVR_TrackedController>();
        }
        
        _trackedController.PadUnclicked += new ClickedEventHandler(DoClick);

        _playAreaVis = Instantiate(playAreaVisPrefab);
        _playAreaVis.gameObject.SetActive(false);
    }

    // just project the new playspace area for now
    void Update()
    {
        if(_trackedController.padPressed)
            UpdateTargetLocation();
        else
            _playAreaVis.gameObject.SetActive(false);

    }

    void UpdateTargetLocation(bool teleport = false)
    {
        var t = reference;
        if(t == null)
            return;

        //Debug.Log("New Position: " + t.position.ToString());

        float refY = t.position.y;
        Plane groundPlane = new Plane(Vector3.up, -refY);

        Ray ray = new Ray(this.transform.position, transform.forward);


        RaycastHit hitInfo;
        bool legalArea = Physics.Raycast(ray, out hitInfo, 2000, teleportLayers);        
        float dist = hitInfo.distance;

        // for now just display the illegal area teleport by using the current ground plane (not so nice)
        if(!legalArea)
            groundPlane.Raycast(ray, out dist);

        if(dist > 0) {

            Vector3 headPosOnGround = new Vector3(SteamVR_Render.Top().head.localPosition.x, 0.0f, SteamVR_Render.Top().head.localPosition.z);
            Vector3 targetPosition = transform.position + dist * transform.forward - headPosOnGround;

            _playAreaVis.gameObject.SetActive(true);
            _playAreaVis.transform.position = targetPosition;

            if(legalArea) {
                _playAreaVis.SetColor(legalAreaColor);

                if(teleport)
                    t.position = targetPosition;
            }
            else
                _playAreaVis.SetColor(illegalAreaColor);

        }
        else {
            _playAreaVis.gameObject.SetActive(false);
        }
    }

    void DoClick(object sender, ClickedEventArgs e)
    {
        // make sure our target is updated
        UpdateTargetLocation(true);
    }
}
