using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace CpvrLab.VirtualTable {

    /// <summary>
    /// Base class for any item that can be picked up by the player. 
    /// 
    /// note:   at the time of writing this comment this class is still unfinished and unused.
    /// 
    /// todo:   there is a lot of shared functionality between UsableItem and MovableItem
    ///         we should consider to maybe combine the two into a single base class.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(VelocityInfo))]
    public class MovableItem : NetworkBehaviour {
        // todo:    implement functionality to be grabbed
        //          one should be able to grab an object already held by someone
        //          else. Or for example pass an object from one hand to an other
        //          the object should remember who's holding it and provide functions to
        //          be picked up.
        //          (or should it not?)

        // todo:    furthermore we should be able to restrict an item to only be picked up
        //          by a specific GamePlayer. However that functionality should be implemented
        //          in a child class. The base class should provide the necessary methods
        //          to achieve that altered functionality.

        public void Attach(Rigidbody rb)
        {
            // todo, leave this joint as a component ( i mean just require the component in the first place)
            var joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rb;
        }

        public void Detach()
        {
            var joint = gameObject.GetComponent<FixedJoint>();
            var velInfo = GetComponent<VelocityInfo>();
            var rb = GetComponent<Rigidbody>();

            DestroyImmediate(joint);
            rb.velocity = velInfo.avrgVelocity;
            rb.angularVelocity = velInfo.avrgAngularVelocity;
        }
    }
}