using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CpvrLab.VirtualTable.Scripts.ModelScripts
{
	public enum EquipmentSlotKey : int
	{
		Head = 1,
		Back = 2,
		// TODO: Add others...
	}
	[Serializable]
	public class EquipmentSlot
	{
		public EquipmentSlotKey Key;
		public GameObject AttachPoint;
	}
}
