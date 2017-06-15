using UnityEngine;
using System.Collections.Generic;
using System;

namespace CpvrLab.VirtualTable {

    /// <summary>
    /// For the first person input we simply map unity keys and axes to 
    /// the ones defined in PlayerInput.
    /// </summary>
    public class FirstPersonPlayerInput : PlayerInput {

        private Dictionary<ActionCode, KeyCode> _actionMapping;
        private Dictionary<AxisCode, string> _axisMapping;
        private Camera _camera;

        void Awake()
        {
            _actionMapping = new Dictionary<ActionCode, KeyCode>();
            _axisMapping = new Dictionary<AxisCode, string>();

            // map key codes to our internal button codes
            _actionMapping.Add(ActionCode.Button0, KeyCode.Mouse0);
            _actionMapping.Add(ActionCode.Button1, KeyCode.Mouse1);

			//AGGame
			_actionMapping.Add(ActionCode.OpenInventory, KeyCode.Tab);
			_actionMapping.Add(ActionCode.Reload, KeyCode.R);
			_actionMapping.Add(ActionCode.Shoot, KeyCode.Mouse0);
			_actionMapping.Add(ActionCode.Teleport, KeyCode.T);
            _actionMapping.Add(ActionCode.AdjustPlayerSize, KeyCode.Q);
            _actionMapping.Add(ActionCode.zombieAbility, KeyCode.Z);
            _actionMapping.Add(ActionCode.meteorAbility, KeyCode.M);
            

            _camera = Camera.main;
        }

        public override bool GetAction(ActionCode ac)
        {
            if(!_actionMapping.ContainsKey(ac))
                return false;

            return Input.GetKey(_actionMapping[ac]);
        }

        public override bool GetActionDown(ActionCode ac)
        {
            if(!_actionMapping.ContainsKey(ac))
                return false;

            return Input.GetKeyDown(_actionMapping[ac]);
        }

        public override bool GetActionUp(ActionCode ac)
        {
            if(!_actionMapping.ContainsKey(ac))
                return false;

            return Input.GetKeyUp(_actionMapping[ac]);
        }

        public override float GetAxis(AxisCode ac)
        {
            if(!_axisMapping.ContainsKey(ac))
                return 0.0f;

            return Input.GetAxis(_axisMapping[ac]);
		}

		public override Vector2 GetAxisVector(AxisCode ac)
		{
			return Vector2.zero;
		}

		public override bool SupportsAxisVector(AxisCode ac)
		{
			return false;
		}

		public override Vector3 GetLookDirection()
        {
            return _camera.transform.forward;
        }

        public override Vector3 GetLeftAimDirection()
        {
            return _camera.transform.forward;
        }

        public override Vector3 GetRightAimDirection()
        {
            return _camera.transform.forward;
        }

        public override bool IsRightHandInput()
        {
            return false;
        }

        public override Transform GetTrackedTransform()
        {
            return null;
        }

        public override void HideModel(bool hide)
        {
			//there is no model to hide...
        }
	}

}