using UnityEngine;
using System.Collections.Generic;
using System;

namespace CpvrLab.VirtualTable {


    /// <summary>
    /// LeapMotion PlayerInput. Not sure how we will do this yet, but we'll see.
    /// </summary>
    public class LeapPlayerInput : PlayerInput {

        public Collider indexTrigger;

        class ActionState
        {
            public bool state = false;
            public bool prevState = false;
            public int frame;
        }

        Dictionary<ActionCode, ActionState> _actionStates = new Dictionary<ActionCode, ActionState>();

        void Start()
        {
            _actionStates.Add(ActionCode.Button0, new ActionState());
            
        }

        void OnTriggerEnter(Collider other)
        {
            if (other == indexTrigger)
            {
                _actionStates[ActionCode.Button0].state = true;
                _actionStates[ActionCode.Button0].frame = Time.frameCount;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other == indexTrigger)
            {
                _actionStates[ActionCode.Button0].state = false;
                _actionStates[ActionCode.Button0].frame = Time.frameCount;
            }
        }

        void Update()
        {
            foreach(var state in _actionStates)
            {
                // reset the prev state if it hasn't changed this frame
                if(state.Value.state != state.Value.prevState && state.Value.frame != Time.frameCount)
                {
                    state.Value.prevState = state.Value.state;
                }
            }            
        }

        public override bool GetAction(ActionCode ac)
        {
            ActionState state;
            if(_actionStates.TryGetValue(ac, out state))
            {
                return state.state;
            }
            return false;
        }

        public override bool GetActionDown(ActionCode ac)
        {
            ActionState state;
            if (_actionStates.TryGetValue(ac, out state))
            {
                return state.state && (state.state != state.prevState);
            }
            return false;
        }

        public override bool GetActionUp(ActionCode ac)
        {
            ActionState state;
            if (_actionStates.TryGetValue(ac, out state))
            {
                return !state.state && (state.state != state.prevState);
            }
            return false;
        }

        public override float GetAxis(AxisCode ac)
        {
            return 0.0f;
		}

		public override Vector2 GetAxisVector(AxisCode ac)
		{
			return Vector2.zero;
		}

		public override bool SupportsAxisVector(AxisCode ac)
		{
			return false;
		}

		public override Vector3 GetLeftAimDirection()
        {
            return Vector3.forward;
        }

        public override Vector3 GetLookDirection()
        {
            return Vector3.forward;
        }

        public override Vector3 GetRightAimDirection()
        {
            return Vector3.forward;
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
            throw new NotImplementedException();
        }
    }

}