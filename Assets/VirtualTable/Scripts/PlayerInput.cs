using UnityEngine;
using System.Collections;
using System;


namespace CpvrLab.VirtualTable {
    
    // todo:    would it be a good idea to use an other input propagation system?
    //          instead of querying the input class with GetActionDown etc could we
    //          send the events to subscribers? 

    /// <summary>
    /// Interface for all PlayerInput used by GamePlayers. 
    /// 
    /// todo:   Does this need to be a MonoBehaviour? I don't think it does.
    /// </summary>
    public abstract class PlayerInput : MonoBehaviour {

        // ??? player for the access of the other PlayerInput
        public GamePlayer player = null;
		private ControllerRole _role = ControllerRole.None;

		// todo:    Don't know how to call these at this point
		//          But this needs to be replaced by something more readable 
		//          ASAP!
		public enum ActionCode {
            Button0,
            Button1,
            Button2,
            Button3,
            Button4,
            Button5,
            Button6,
            Button7,
            Button8,
            Button9,

			//AGGame
			OpenInventory,
			Reload,
			Shoot,
			Teleport,
            AdjustPlayerSize,
			ShowPlayerInfo,

            zombieAbility,
            meteorAbility
        }
        public enum AxisCode {
            Axis0,
            Axis1,
            Axis2,
            Axis3,
            Axis4,
            Axis5,
            Axis6,
            Axis7,

			//AGGame
			Touchpad
        }
		/// <summary>
		/// Controller role, input may have a combination of roles.
		/// </summary>
		public enum ControllerRole : int
		{
			None = 0,
			Main = 1 << 0,
			LeftHand = 1 << 1,
			RightHand = 1 << 2,
		}

		// is this action currently active
		public abstract bool GetAction(ActionCode ac);

        // was this action pressed during the current frame?
        public abstract bool GetActionDown(ActionCode ac);
        // was this action released during the current frame?
        public abstract bool GetActionUp(ActionCode ac);

        // get value of this axis
        public abstract float GetAxis(AxisCode ac);
		public abstract Vector2 GetAxisVector(AxisCode ac);
		/// <summary>
		/// Check if the input supports axis vectors for the specified code.
		/// </summary>
		/// <param name="ac">AxisCode</param>
		/// <returns>Is supported</returns>
		public abstract bool SupportsAxisVector(AxisCode ac);

		// not sure if we need these here...
		public abstract Vector3 GetLookDirection();
        public abstract Vector3 GetLeftAimDirection();
        public abstract Vector3 GetRightAimDirection();

		public void SetControllerRole(ControllerRole role)
		{
			_role = role;
		}
		public bool IsInControllerRole(ControllerRole role)
		{
			return (_role & role) == role;
		}

		// ??? Access the transform of the controller
        public abstract Transform GetTrackedTransform();

		// ??? Check if an input is in the right hand. Only for Vive and Rift
		[Obsolete("use GetControllerRole instead")]
		public abstract bool IsRightHandInput();

        // ??? Hide or show the visual representation of the input
        public abstract void HideModel(bool hide);

    }

}