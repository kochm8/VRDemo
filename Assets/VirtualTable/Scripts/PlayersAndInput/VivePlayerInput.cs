using UnityEngine;
using System.Collections.Generic;
using System;

using Valve.VR;

namespace CpvrLab.VirtualTable {

    /// <summary>
    /// VivePlayerInput, we map certain buttons from the vive controller to the generic PlayerInput axes and actions. 
    /// </summary>
    public class VivePlayerInput : PlayerInput {
        
        [HideInInspector]
        public SteamVR_TrackedObject trackedObj;

        private Dictionary<ActionCode, EVRButtonId> _actionMapping;
        private Dictionary<AxisCode, EVRButtonId> _axisMapping;

        void Awake()
        {
            _actionMapping = new Dictionary<ActionCode, EVRButtonId>();
            _axisMapping = new Dictionary<AxisCode, EVRButtonId>();

            _actionMapping.Add(ActionCode.Button0, EVRButtonId.k_EButton_SteamVR_Trigger);

			_axisMapping.Add(AxisCode.Axis0, EVRButtonId.k_EButton_SteamVR_Trigger);

			//AGGame
			_actionMapping.Add(ActionCode.OpenInventory, EVRButtonId.k_EButton_SteamVR_Touchpad);
			_actionMapping.Add(ActionCode.Reload, EVRButtonId.k_EButton_Grip);
			_actionMapping.Add(ActionCode.Shoot, EVRButtonId.k_EButton_SteamVR_Trigger);
			_actionMapping.Add(ActionCode.Teleport, EVRButtonId.k_EButton_SteamVR_Touchpad);
            _actionMapping.Add(ActionCode.AdjustPlayerSize, EVRButtonId.k_EButton_ApplicationMenu);
			_actionMapping.Add(ActionCode.ShowPlayerInfo, EVRButtonId.k_EButton_ApplicationMenu);

            _actionMapping.Add(ActionCode.zombieAbility, EVRButtonId.k_EButton_SteamVR_Trigger);
            _actionMapping.Add(ActionCode.meteorAbility, EVRButtonId.k_EButton_Grip);

            _axisMapping.Add(AxisCode.Touchpad, EVRButtonId.k_EButton_Axis0);
        }


        public override bool GetAction(ActionCode ac)
        {
            if(!_actionMapping.ContainsKey(ac))
                return false;

            // todo: make this a one time initialization process
            //       device isn't always initialized in the Start method
            //       so we currently retrieve it every time. 
            //       check if this is inefficient and implement something better if needed.
            if(trackedObj.index == SteamVR_TrackedObject.EIndex.None)
                return false;

            var device = SteamVR_Controller.Input((int)trackedObj.index);

            return device.GetPress(_actionMapping[ac]);
        }

        public override bool GetActionDown(ActionCode ac)
        {
            if(!_actionMapping.ContainsKey(ac))
                return false;
            if(trackedObj.index == SteamVR_TrackedObject.EIndex.None)
                return false;

            var device = SteamVR_Controller.Input((int)trackedObj.index);
            
            return device.GetPressDown(_actionMapping[ac]);
        }

        public override bool GetActionUp(ActionCode ac)
        {
            if(!_actionMapping.ContainsKey(ac))
                return false;
            if(trackedObj.index == SteamVR_TrackedObject.EIndex.None)
                return false;

            var device = SteamVR_Controller.Input((int)trackedObj.index);

            
            return device.GetPressUp(_actionMapping[ac]);
        }

        public override float GetAxis(AxisCode ac)
        {
            if(!_axisMapping.ContainsKey(ac))
                return 0.0f;
            if(trackedObj.index == SteamVR_TrackedObject.EIndex.None)
                return 0.0f;

            var device = SteamVR_Controller.Input((int)trackedObj.index);


            // todo:    use a struct for us to define if we want to map x or y from the device.GetAxis method
            return device.GetAxis(_axisMapping[ac]).x;
		}
		public override Vector2 GetAxisVector(AxisCode ac)
		{
			if (!_axisMapping.ContainsKey(ac) || trackedObj.index == SteamVR_TrackedObject.EIndex.None)
				return Vector2.zero;

			var device = SteamVR_Controller.Input((int)trackedObj.index);
			
			return device.GetAxis(_axisMapping[ac]);
		}

		public override bool SupportsAxisVector(AxisCode ac)
		{
			return _axisMapping.ContainsKey(ac);
		}

		public override Vector3 GetLeftAimDirection()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetLookDirection()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetRightAimDirection()
        {
            throw new NotImplementedException();
        }

        public override bool IsRightHandInput()
        {
            int rightIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
            int leftIndex = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
            return trackedObj.index == (SteamVR_TrackedObject.EIndex)rightIndex;
        }

        public override Transform GetTrackedTransform()
        {
            return trackedObj.transform;
        }

        public override void HideModel(bool hide)
        {
            GameObject model = Utils.getChildRec(trackedObj.transform.gameObject, "Model");
            if (model)
                model.SetActive(!hide);
        }
    }

}