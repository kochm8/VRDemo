using CpvrLab.ArcherGuardian.Scripts.PlayersAndIO;
using CpvrLab.VirtualTable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.Items
{
	public class TeleportEventArgs : CancelEventArgs
	{
		public TeleportEventArgs(float distance)
		{
			Distance = distance;
		}
		public float Distance { get; private set; }
	}
	public class Teleporter : MonoBehaviour
	{
		#region members
		private const float MaxTeleportDistance = 1000;
		private const float TouchpadPos = 0.35f;

		private bool _isTargeting = false;

		private LayerMask _teleportLayers;

		private Color _legalAreaColor = Color.green;
		private Color _illegalAreaColor = Color.red;

		private PlayAreaVis _playAreaVis;
		private GamePlayer _player;
		private PlayerInput _input;
		private Transform _transform;
		private bool _usesPlayArea;

		private LineRenderer _laserLine;

		private float _rotationAngle = 2.0f;
		private Vector3 _delta;

		private IList<Vector3> _hitPoints = new List<Vector3>();
		// Counter is used to define a max distence of all raycasts, total Length = maxCounter * lenght
		private int _maxCounter = 100;
		private float _length = 0.6f;
		#endregion members

		#region public
		public event EventHandler<TeleportEventArgs> Teleport;
		public event EventHandler<TeleportEventArgs> CanTeleport;
		/// <summary>
		/// Initializes the teleporter from the specified settings.
		/// </summary>
		/// <param name="settings">Player settings.</param>
		public void InitFromSettings(AGPlayerSettings settings)
		{
			_teleportLayers = settings.TeleportLayers;
			_legalAreaColor = settings.LegalAreaColor;
			_illegalAreaColor = settings.IllegalAreaColor;

			if (_playAreaVis != null)
				Destroy(_playAreaVis);

			_playAreaVis = Instantiate(settings.PlayAreaVisPrefab);
			_playAreaVis.gameObject.SetActive(false);
		}
		/// <summary>
		/// Initializes the teleporter.
		/// </summary>
		/// <param name="teleportLayers">Teleportation layer.</param>
		/// <param name="legalAreaColor">Color for legal areas.</param>
		/// <param name="illegalAreaColor">Color for illegal areas.</param>
		/// <param name="playAreaVisPrefab">Prefab for play area.</param>
		public void Init(LayerMask teleportLayers, Color legalAreaColor, Color illegalAreaColor, PlayAreaVis playAreaVisPrefab)
		{
			_teleportLayers = teleportLayers;
			_legalAreaColor = legalAreaColor;
			_illegalAreaColor = illegalAreaColor;

			if (_playAreaVis != null)
				Destroy(_playAreaVis);

			_playAreaVis = Instantiate(playAreaVisPrefab);
			_playAreaVis.gameObject.SetActive(false);
		}
		public void SetPlayerInput(GamePlayer player, PlayerInput input)
		{
			//var top = SteamVR_Render.Top();
			//if (top != null)
			//{
			//	_transform = top.head;
			//}
			_player = player;
			_input = input;
			_transform = input.GetTrackedTransform();
			if (_transform == null)
			{
				// No transform from the input
				// Find the transform with the camera on the player instead.
				var camera = _player.GetComponentInChildren<Camera>();
				_transform = camera == null ? _player.transform : camera.transform;
			}

			// TODO: Find a better way to support different devices.
			_usesPlayArea = _player is VivePlayer;
		}
		public void RemovePlayer()
		{
			_player = null;
			_input = null;
		}
		#endregion public

		private void Start()
		{
			_laserLine = gameObject.AddComponent<LineRenderer>();
			_laserLine.SetWidth(0.02f, 0.02f);
			_laserLine.enabled = false;
			_laserLine.material = new Material(Shader.Find("Particles/Additive"));
			_laserLine.SetColors(_illegalAreaColor, _illegalAreaColor);

			_delta = new Vector3(0.0f, Mathf.Sin(Mathf.Deg2Rad * _rotationAngle), 0.0f);
		}

		// just project the new playspace area for now
		private void Update()
		{
			if (_player == null || _input == null)
				return;

			if (_input.GetActionDown(PlayerInput.ActionCode.Teleport))
			{
				// check for press in the middle of the touchpad
				if (!_input.SupportsAxisVector(PlayerInput.AxisCode.Touchpad)
					|| _input.GetAxisVector(PlayerInput.AxisCode.Touchpad).magnitude < TouchpadPos)
				{
					_isTargeting = true;
				}
			}
			if (_isTargeting)
			{
				if (_input.GetActionUp(PlayerInput.ActionCode.Teleport))
				{
					UpdateTargetLocation(true);
					_isTargeting = false;
					_laserLine.enabled = false;
					_playAreaVis.gameObject.SetActive(false);
				}
				else
				{
					UpdateTargetLocation();
				}
			}
		}

		private void UpdateTargetLocation(bool teleport = false)
		{
			_hitPoints.Clear();
			_laserLine.SetPositions(new Vector3[0]);

			RaycastHit hit;
			Vector3 rayDirection = Vector3.ClampMagnitude(_transform.forward, _length);
			Vector3 position = _transform.position;
			Ray ray = new Ray(position, rayDirection);

			while (!Physics.Raycast(ray, out hit, _length, _teleportLayers) && _hitPoints.Count < _maxCounter)
			{
				_hitPoints.Add(position);

				position = position + rayDirection;
				rayDirection = Vector3.ClampMagnitude(rayDirection - _delta, _length);

				ray = new Ray(position, rayDirection);
			}

			Vector3 hitPoint;
			bool legalArea;
			float dist;
			if (_hitPoints.Count == _maxCounter)
			{
				legalArea = false;
				_hitPoints.Clear();

				rayDirection = Vector3.ClampMagnitude(_transform.forward, _length);
				position = _transform.position;
				ray = new Ray(position, rayDirection);

				var groundPlane = new Plane(Vector3.up, -_transform.position.y);
				while ((!groundPlane.Raycast(ray, out dist) || dist > _length) && _hitPoints.Count < _maxCounter)
				{
					_hitPoints.Add(position);

					position = position + rayDirection;
					rayDirection = Vector3.ClampMagnitude(rayDirection - _delta, _length);

					ray = new Ray(position, rayDirection);
				}
				// Check if the hit point is close enough (only happens if too many points have been calculated).
				hitPoint = (dist <= _length) ? ray.GetPoint(dist) : _transform.position;
			}
			else
			{
				legalArea = true;
				hitPoint = hit.point;
			}
			dist = (hitPoint - _transform.position).magnitude;

			if (dist > 0)
			{
				// hit point found
				_laserLine.SetVertexCount(_hitPoints.Count + 1);
				for (int i = 0; i < _hitPoints.Count; i++)
				{
					_laserLine.SetPosition(i, _hitPoints[i]);
				}

				_laserLine.SetPosition(_hitPoints.Count, hitPoint);
				_laserLine.enabled = true;

				var targetPosGround = hitPoint;//new Vector3(hitPoint.x, 0f, hitPoint.z);

				TeleportEventArgs args = null;
				if (legalArea)
				{
					args = new TeleportEventArgs(dist);
					if (CanTeleport != null)
						CanTeleport(this, args);

					legalArea = !args.Cancel;
				}
				if (_usesPlayArea)
				{
					_playAreaVis.transform.position = hitPoint;
					_playAreaVis.gameObject.SetActive(true);
					_playAreaVis.transform.position = targetPosGround;

					_playAreaVis.SetColor(legalArea ? _legalAreaColor : _illegalAreaColor);
				}
				if (legalArea)
				{
					_laserLine.SetColors(_legalAreaColor, _legalAreaColor);

					if (teleport)
					{
						if (Teleport != null)
						{
							Teleport(this, args);
							if (args.Cancel)
							{
								// Teleportation has been canceled.
								return;
							}
						}
						_player.transform.position = targetPosGround;
					}
				}
				else
				{
					_laserLine.SetColors(_illegalAreaColor, _illegalAreaColor);
				}
			}
			else
			{
				_laserLine.enabled = false;
				if (_usesPlayArea)
				{
					_playAreaVis.gameObject.SetActive(false);
				}
			}
		}
	}
}