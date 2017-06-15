using CpvrLab.VirtualTable;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.GameLogic
{
	public class GameStartCylinder : NetworkBehaviour
	{
		public int AutoStartTime = 5;
		// Use this for initialization
		void Start()
		{
			StartCoroutine(Countdown());
		}
		private IEnumerator Countdown()
		{
			yield return new WaitForSeconds(AutoStartTime);
			StartGame();
		}
		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				StartGame();
			}
		}
		void OnTriggerEnter(Collider other)
		{
			StartGame();
		}
		[Server]
		private void StartGame()
		{
			StopAllCoroutines();
			GameManager.instance.StartGame(0);
			Destroy(gameObject);
		}
	}
}