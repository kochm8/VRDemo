using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace CpvrLab.VirtualTable {

    /// <summary>
    /// LeapMotion VR player. Not sure if this is the way we want to do it yet, but it's here.
    /// </summary>
    public class LeapPlayer : GamePlayer {
        protected override PlayerInput GetMainInput()
        {
            throw new NotImplementedException();
        }

        public override PlayerInput GetOtherInput(PlayerInput input)
        {
            throw new NotImplementedException();
        }

        protected override void OnEquip(AttachmentSlot slot)
        {
            throw new NotImplementedException();
        }

        protected override void OnUnequip(UsableItem item)
        {
            throw new NotImplementedException();
        }
		public override IEnumerable<PlayerInput> GetAllInputs()
		{
			throw new NotImplementedException();
		}
	}

}