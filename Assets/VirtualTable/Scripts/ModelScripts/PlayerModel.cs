using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CpvrLab.VirtualTable.Scripts.ModelScripts;

namespace CpvrLab.VirtualTable
{
    /// <summary>
    /// Base class for visual player representations. GamePlayer contains multiple of
    /// these for local and remote clients to use.
    /// </summary>
    public abstract class PlayerModel : MonoBehaviour
    {
		#region members
		public EquipmentSlot[] EquipmentSlots;
        /// <summary>
        /// The name tag billboard that will normally be displayed above the player models head.
        /// </summary>
        public Text playerText;
		#endregion members
		/// <summary>
		/// Get the object to which equipment should be attached to.
		/// </summary>
		/// <param name="slotKey">EquipmentSlotKey</param>
		/// <returns>Parent object for all equipment in this slot.</returns>
		public GameObject GetEquipmentAttachObj(EquipmentSlotKey slotKey)
		{
			foreach(var slot in EquipmentSlots)
			{
				if (slot.Key == slotKey)
				{
					return slot.AttachPoint;
				}
			}
			return null;
		}
		
		public abstract void RenderPreview(RenderTexture target);
        /// <summary>
        /// Called by the owning GamePlayer
        /// </summary>
        /// <param name="player"></param>
        public abstract void InitializeModel(GamePlayer player);
        
    }

}