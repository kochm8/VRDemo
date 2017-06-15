using CpvrLab.VirtualTable;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CpvrLab.ArcherGuardian.Scripts.Util.Interface
{
	/// <summary>
	/// Class that handles all visible properties of a game object.
	/// </summary>
	public class GameObjectProperties : MonoBehaviour
	{
		public enum PropertyKey
		{
			Health,
			Stamina
		}
		#region members
		#region public
		public GameObject InfoGO;
		public Text DisplayName;
		#endregion public
		// Use a dictionary so each property has to be unique.
		private Dictionary<PropertyKey, PropertyBar> _propertyBars;
		private PlayerInput _input;
		#endregion members

		private void Awake()
		{
			_propertyBars = new Dictionary<PropertyKey, PropertyBar>();
			var propertyBars = InfoGO.GetComponentsInChildren<PropertyBar>();
			if (propertyBars != null)
			{
				foreach (var propBar in propertyBars)
				{
					_propertyBars.Add(propBar.Key, propBar);
				}
			}
		}
		private void Update()
		{
			if (_input == null)
				return;

			if (_input.GetActionDown(PlayerInput.ActionCode.ShowPlayerInfo))
			{
				InfoGO.SetActive(true);
			}
			else if (_input.GetActionUp(PlayerInput.ActionCode.ShowPlayerInfo))
			{
				InfoGO.SetActive(false);
			}
		}

		public void SetPlayerInput(PlayerInput input)
		{
			_input = input;
		}

		/// <summary>
		/// Set the display name
		/// </summary>
		/// <param name="displayName"></param>
		public void SetDisplayName(string displayName)
		{
			if (DisplayName != null)
				DisplayName.text = displayName;
		}
		/// <summary>
		/// Set the minimum and the maximum value of a property.
		/// </summary>
		/// <param name="propertyKey">Key/Identification of property <seealso cref="PropertyKey"/></param>
		/// <param name="minValue">Minimum value</param>
		/// <param name="maxValue">Maximum value</param>
		public void SetPropertyBarMinMax(PropertyKey propertyKey, float minValue, float maxValue)
		{
			if (_propertyBars.ContainsKey(propertyKey))
				_propertyBars[propertyKey].SetMinMax(minValue, maxValue);
		}
		/// <summary>
		/// Sets a property value.
		/// </summary>
		/// <param name="propertyKey">Key/Identification of property <seealso cref="PropertyKey"/></param>
		/// <param name="propertyValue">Value</param>
		public void SetPropertyBarValue(PropertyKey propertyKey, float propertyValue)
		{
			if (_propertyBars.ContainsKey(propertyKey))
				_propertyBars[propertyKey].SetPropertyValue(propertyValue);
		}
	}
}
