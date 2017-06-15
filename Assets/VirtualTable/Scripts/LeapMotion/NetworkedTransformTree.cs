using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


namespace CpvrLab.VirtualTable
{

    public class TransformTreeMsg : MessageBase
    {
        public Vector3 rootPos;
        public Quaternion rootRot;
        public Quaternion[] children;
        public NetworkInstanceId netId;
        public int num;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
            writer.Write(num);
            writer.Write(rootPos);
            writer.Write(rootRot);

            writer.Write(children.Length);

            // todo: we should probably be compressing this data?
            for (int i = 0; i < children.Length; i++)
                writer.Write(children[i]);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            netId = reader.ReadNetworkId();
            num = reader.ReadInt32();

            rootPos = reader.ReadVector3();
            rootRot = reader.ReadQuaternion();

            int length = reader.ReadInt32();
            children = new Quaternion[length];

            for (int i = 0; i < children.Length; i++)
                children[i] = reader.ReadQuaternion();
        }        
    }

    /// <summary>
    /// Used for a networked transform tree
    /// </summary>
    public class NetworkedTransformTree : NetworkBehaviour
    {
        static bool receiverInit = false;
        public Transform root;
        private Transform[] _children;
        public int num = 0;

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (!receiverInit)
            {
                receiverInit = true;
                NetworkServer.RegisterHandler(VTMsgType.NetworkTransformTree, HandleNetworkTransformTree);
            }
        }

        void LateUpdate()
        {
            // always sync 
            // todo: only update if something changed
            if (!hasAuthority)
                return;


            if (isServer)
            {
                SetDirtyBit(1);
            }
            else
            {
                // send new state to the server
                var msg = new TransformTreeMsg();

                msg.netId = netId;
                msg.num = num;
                
                msg.rootPos = root.position;
                msg.rootRot = root.rotation;

                msg.children = new Quaternion[_children.Length];
                for (int i = 0; i < _children.Length; i++)
                    msg.children[i] = _children[i].rotation;

                connectionToServer.Send(VTMsgType.NetworkTransformTree, msg);
                //CmdHandSyncTest(msg);
            }
        }
        

        static void HandleNetworkTransformTree(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<TransformTreeMsg>();
            var foundObject = NetworkServer.FindLocalObject(msg.netId);
            var ntt = foundObject.GetComponents<NetworkedTransformTree>();
            
            for(int i = 0; i < ntt.Length; i++)
            {
                if(ntt[i].num == msg.num)
                {
                    ntt[i].root.position = msg.rootPos;
                    ntt[i].root.rotation = msg.rootRot;

                    for (int j = 0; j < ntt[i]._children.Length; j++)
                        ntt[i]._children[j].rotation = msg.children[j];

                    ntt[i].SetDirtyBit(1);
                    break;
                }
            }
            
        }


        // todo: optimize the syncing by only syncing the rotations that actually changed!
        //       
        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (_children == null)
                _children = (Transform[])root.GetComponentsInChildren<Transform>();
            
            if (initialState)
            {
                // always write initial state, no dirty bits
            }
            else if (syncVarDirtyBits == 0)
            {
                writer.WritePackedUInt32(0);
                return false;
            }
            else
            {
                // dirty bits
                writer.WritePackedUInt32(1);
            }

            writer.Write(root.position);
            writer.Write(root.rotation);

            // todo: we should probably be compressing this data?
            for (int i = 0; i < _children.Length; i++)
                writer.Write(_children[i].rotation);


            return true;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (_children == null)
                _children = (Transform[])root.GetComponentsInChildren<Transform>();

            if (isServer && NetworkServer.localClientActive)
                return;

            if (!initialState)
            {
                if (reader.ReadPackedUInt32() == 0)
                    return;
            }


            root.position = reader.ReadVector3();
            root.rotation = reader.ReadQuaternion();

            for (int i = 0; i < _children.Length; i++)
                _children[i].rotation = reader.ReadQuaternion();
        }
    }

}