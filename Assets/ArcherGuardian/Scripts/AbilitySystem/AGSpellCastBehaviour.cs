using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using CpvrLab.VirtualTable;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{

    public class AGSpellCastBehaviour : NetworkBehaviour
    {
        [HideInInspector]
        public Ability ability;
        [HideInInspector]
        public PlayerInput input;

        public GameObject pointerPrefab;

        private GameObject _pointer;
        private LineRenderer _line;
        private GameObject _sphere;

        private Vector3 _spawnPos;
        private bool canSpawn = false;
        private bool useAbility = false;

        public void Cast(Vector3 spawnPt)
        {
            if (isLocalPlayer)
            {
                Debug.Log("SpellCastBehaviour:Case Spell");
                CmdSpawn(spawnPt);
            }
            CleanUp();
        }

        [Command]
        public void CmdSpawn(Vector3 spawnPt)
        {
            var obj = Instantiate(ability.prefab, spawnPt, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(obj);
        }

        public void Initialize()
        {
            if (!isLocalPlayer) return;
            if (isServer) return;

            _pointer = Instantiate(pointerPrefab);

            _line = _pointer.GetComponentInChildren<LineRenderer>();
            _line.material = new Material(Shader.Find("Particles/Additive"));
            _line.SetColors(Color.yellow, Color.red);
            _sphere = _pointer.GetComponentInChildren<MeshRenderer>().gameObject;
        }


        void FixedUpdate()
        {

            if (!isLocalPlayer) return;
            if (!useAbility) return;

            if (input.GetAction(ability.actioncode))
            {

                if (input.GetTrackedTransform() != null)
                {

                    RaycastHit hitInfo;
                    if (Physics.Raycast(input.GetTrackedTransform().position, input.GetTrackedTransform().forward, out hitInfo, 50))
                    {
                        Debug.DrawLine(input.GetTrackedTransform().position, hitInfo.point, Color.cyan);

                        _line.SetPosition(0, input.GetTrackedTransform().position);
                        _line.SetPosition(1, hitInfo.point);
                        _sphere.transform.position = hitInfo.point;
                        _spawnPos = hitInfo.point;
                    }
                    else
                    {
                        _line.SetPosition(0, Vector3.zero);
                        _line.SetPosition(1, Vector3.zero);
                    }
                }
                else
                {
                    _spawnPos = Vector3.zero;
                }

                canSpawn = true;
            }

            if (!canSpawn) return;

            if (input.GetActionUp(ability.actioncode))
            {
                Cast(_spawnPos);
                canSpawn = false;
                useAbility = false;
            }
        }

        private void CleanUp()
        {
            Destroy(_pointer);
        }

        public void Use()
        {
            useAbility = true;
        }
    }
}