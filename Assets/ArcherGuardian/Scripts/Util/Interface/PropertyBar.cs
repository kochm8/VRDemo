using CpvrLab.VirtualTable;
using UnityEngine;
using UnityEngine.UI;

namespace CpvrLab.ArcherGuardian.Scripts.Util.Interface
{
	public class PropertyBar : MonoBehaviour
	{
		#region members
		#region public
		/// <summary>
		/// The key or identifier of the property.
		/// </summary>
		public GameObjectProperties.PropertyKey Key;
		/// <summary>
		/// The slider UI control.
		/// </summary>
		public Slider PropertyValueSlider;
		#endregion public
		#endregion members

		#region public
		/// <summary>
		/// Set the minimum and the maximum value of the slider.
		/// </summary>
		/// <param name="minValue">Minimum value</param>
		/// <param name="maxValue">Maximum value</param>
		public void SetMinMax(float minValue, float maxValue)
		{
			PropertyValueSlider.minValue = minValue;
			PropertyValueSlider.maxValue = maxValue;
		}
		/// <summary>
		/// Set the desired value of the property.
		/// </summary>
		/// <param name="value"></param>
		public void SetPropertyValue(float value)
		{
			PropertyValueSlider.value = value;
		}
		#endregion public
	}
}