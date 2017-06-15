using UnityEngine;
using UnityEngine.Events;

namespace CpvrLab.VirtualTable
{

    public class ShooterHitEvent : UnityEvent<Vector3, GamePlayer> { }

    public class Shootable : MonoBehaviour
    {
        public ShooterHitEvent OnHit = new ShooterHitEvent();
        

        public void Hit(Vector3 position, GamePlayer shooter)
        {
            if(OnHit != null)
                OnHit.Invoke(position, shooter);
        }
    }

}