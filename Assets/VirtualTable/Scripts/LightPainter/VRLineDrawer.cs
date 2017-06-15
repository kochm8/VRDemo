using UnityEngine;
using System.Collections.Generic;

public class VRLineDrawer : MonoBehaviour {

    public SteamVR_TrackedObject trackedObj;
    public float lineWidth = 0.05f;
    public float minLineWidth = 0.005f;
    public float maxLineWidth = 1.0f;
    public float minDistance = 0.01f;
    public float increment = 0.05f;
    public Material lineMaterial;
    public GameObject paintCylinder;
    
    private List<GameObject> _meshLines = new List<GameObject>();
    private MeshLineRenderer _curMeshLine;
    private Vector2 _lastTouchAxis;
    private float _lastDeletePress;


	
	// Update is called once per frame
	void Update ()
    {
	    SteamVR_Controller.Device device = SteamVR_Controller.Input((int)trackedObj.index);

        // Create new mesh line on trigger down
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            GameObject go = new GameObject("MeshLine");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            _meshLines.Add(go);

            _curMeshLine = go.AddComponent<MeshLineRenderer>();
            _curMeshLine.lineWidth = lineWidth;
            _curMeshLine.material = lineMaterial;
        }
        else // continue drawing by adding new points to the current mesh line
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
        {
            //Vector3 posNow = trackedObj.transform.position;
            Vector3 posCenter = paintCylinder.transform.position;
            Vector3 right     = paintCylinder.transform.up;
            float   scaleY    = paintCylinder.transform.localScale.y;
            Debug.Log(scaleY);
            Vector3 posRight  = posCenter + right * scaleY;
            Vector3 posLeft   = posCenter - right * scaleY;

            if (_curMeshLine.lastPointCenter == Vector3.zero || Vector3.Distance(_curMeshLine.lastPointCenter, posCenter) > minDistance)
                _curMeshLine.AddQuad(posCenter, posLeft, posRight);
        }
        else // clear mesh lines on grip press 
        if(device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            float deltaTime = Time.time - _lastDeletePress;
            Debug.Log(deltaTime);

            // Delete all on double press
            if (deltaTime < 0.3f)
            {   foreach(var go in _meshLines) 
                    DestroyImmediate(go);
                _meshLines.Clear();
            } else
            {
                if (_meshLines.Count > 0)
                {   DestroyImmediate(_meshLines[_meshLines.Count-1]);
                    _meshLines.RemoveAt(_meshLines.Count-1);
                }
            }
            
            _lastDeletePress = Time.time;
        }
        else  // Increase the brush width over touchpad swipes
        if(device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Vector2 touchAxis = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            Vector2 deltaTouchAxis = touchAxis - _lastTouchAxis;

            if (deltaTouchAxis.magnitude > 0.02f)
            {
                Vector3 scale = paintCylinder.transform.localScale;
                float newScale = scale.y * (1.0f + increment * Mathf.Sign(deltaTouchAxis.x));
            
                newScale = Mathf.Clamp(newScale, minLineWidth, maxLineWidth);

                //Debug.Log("deltaTouchAxis" + deltaTouchAxis);

                paintCylinder.transform.localScale = new Vector3(scale.x, newScale, scale.z);
            }

            _lastTouchAxis = touchAxis;
        }
	}
}
