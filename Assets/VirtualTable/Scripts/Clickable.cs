using UnityEngine;
using UnityEngine.Events;

namespace CpvrLab.VirtualTable
{

    public class ClickHitEvent : UnityEvent<GamePlayer> { }

    public class Clickable : MonoBehaviour
    {
        public ClickHitEvent OnClick = new ClickHitEvent();
        

        public void Click(GamePlayer player)
        {
            if(OnClick != null)
                OnClick.Invoke(player);
        }
    }

}