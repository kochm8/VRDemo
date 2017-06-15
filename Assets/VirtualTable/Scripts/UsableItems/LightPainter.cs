using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace CpvrLab.VirtualTable {


    /// <summary>
    /// Simple "light painter" that allows the user to draw in the air.
    ///
    /// todo:    Implement a custom line renderer that behaves like googles tilt brush line strokes
    ///          The lines shouldn't try to orientate themselves towards the camera but just stay flat
    ///          Or we could implement actual 3d brush strokes. Not sure yet. Has to be decided
    ///          when we implement the light painting game.
    ///          As of now this is just a proof of concept
    /// 
    /// todo:    Network optimizations if needed: We currently send a command every time we add
    ///          a point to the line renderer. This could bog down the network. A better approach 
    ///          would be to use the position on each client to draw the line. And periodically
    ///          send a list of correct line points to the server for the other clients to 
    ///          correct their local drawings.
    /// </summary>
    public class LightPainter : UsableItem {

        public bool clearOnDrop = false;

        [SyncVar]
        private bool _drawing = false;

        // new method
        public float lineWidth = 0.1f;
        public float minLineWidth = 0.005f;
        public float maxLineWidth = 1.0f;
        public float minDistance = 0.01f;
        public float increment = 0.05f;
        public float singleClickTime = 0.4f;
        public Material lineMaterial;
        public GameObject paintCylinder;

        private List<GameObject> _meshLines = new List<GameObject>();
        private MeshLineRenderer _curMeshLine;
        private float _lastStartTime;


        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        void Update()
        {
            if(_input == null)
                return;

            // todo: this seems tedious, can't we just subscribe to buttons and receive input?
            if(_input.GetActionDown(PlayerInput.ActionCode.Button0))
            {
                StartNewLine();
                _lastStartTime = Time.time;
            }

            if(_input.GetActionUp(PlayerInput.ActionCode.Button0))
            {
                bool isSingleClick = Time.time - _lastStartTime < singleClickTime;

                if (isSingleClick)
                    ClearLastLine();
            }


            bool drawing = _input.GetAction(PlayerInput.ActionCode.Button0);
            if(_drawing != drawing) {
                _drawing = drawing;
                CmdSetDrawing(_drawing);
            }

            if (_drawing)
            {
                Vector3 posCenter = paintCylinder.transform.position;
                Vector3 right     = paintCylinder.transform.up;
                float   scaleY    = paintCylinder.transform.localScale.y;
                Vector3 posRight  = posCenter + right * scaleY;
                Vector3 posLeft   = posCenter - right * scaleY;

                if (_curMeshLine.lastPointCenter == Vector3.zero || Vector3.Distance(_curMeshLine.lastPointCenter, posCenter) > minDistance)
                {
                    AddLinePoint(posCenter, posLeft, posRight);
                }
            }

            if(_input.GetAction(PlayerInput.ActionCode.Button1)) {
                ClearAllLines();
            }
        }
        [Command] void CmdSetDrawing(bool value) { _drawing = value; }


        
  
        void StartNewLine()
        {
            var color = Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
            StartNewLineClient(color);
            CmdStartNewLine(color);
        }
        [Command] void CmdStartNewLine(Color color) { RpcStartNewLine(color); }
        [ClientRpc] void RpcStartNewLine(Color color) { if(!hasAuthority) StartNewLineClient(color); }
        void StartNewLineClient(Color color)
        {
            Debug.Log("StartNewLineClient: _meshLines.Count: " + _meshLines.Count);
            GameObject go = new GameObject("MeshLine");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            _meshLines.Add(go);

            _curMeshLine = go.AddComponent<MeshLineRenderer>();
            _curMeshLine.lineWidth = lineWidth;
            _curMeshLine.material = lineMaterial;
        }
        
        void AddLinePoint(Vector3 posCenter, Vector3 posLeft, Vector3 posRight)
        {
            AddLinePointClient(posCenter, posLeft, posRight);
            CmdAddLinePoint(posCenter, posLeft, posRight);
        }
        [Command] void CmdAddLinePoint(Vector3 posCenter, Vector3 posLeft, Vector3 posRight) { RpcAddLinePoint(posCenter, posLeft, posRight); }
        [ClientRpc] void RpcAddLinePoint(Vector3 posCenter, Vector3 posLeft, Vector3 posRight) { if(!hasAuthority) AddLinePointClient(posCenter, posLeft, posRight); }
        void AddLinePointClient(Vector3 posCenter, Vector3 posLeft, Vector3 posRight)
        {
            _curMeshLine.AddQuad(posCenter, posLeft, posRight);
        }
        
        void ClearLastLine()
        {
            ClearLastLineClient();
            //CmdClearLastLine();
        }
        [Command] void CmdClearLastLine() { RpcClearLastLine(); }
        [ClientRpc] void RpcClearLastLine() { if(!hasAuthority) ClearLastLineClient(); }
        void ClearLastLineClient()
        {
            Debug.Log("ClearLastLineClient: _meshLines.Count: " + _meshLines.Count);
            if (_meshLines.Count > 1)
            {
                DestroyImmediate(_meshLines[_meshLines.Count-1]);
                DestroyImmediate(_meshLines[_meshLines.Count-2]);
                _meshLines.RemoveAt(_meshLines.Count-1);
                _meshLines.RemoveAt(_meshLines.Count-1);
            }
        }

        void ClearAllLines()
        {
            ClearAllLinesClient();
            CmdClearAllLines();
        }
        [Command] void CmdClearAllLines() { RpcClearAllLines(); }
        [ClientRpc] void RpcClearAllLines() { if(!hasAuthority) ClearAllLinesClient(); }
        void ClearAllLinesClient()
        {
            foreach(var go in _meshLines) 
                DestroyImmediate(go);
            _meshLines.Clear();
        }

        // clear the drawing when this item is dropped
        protected override void OnUnequip()
        {
            base.OnUnequip();

            if(clearOnDrop)
                ClearAllLines();
        }
            
    }

}