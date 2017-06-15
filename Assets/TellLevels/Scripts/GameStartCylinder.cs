using CpvrLab.VirtualTable;
using UnityEngine;

namespace CpvrLab.TellLevels.Scripts
{
	public class GameStartCylinder : MonoBehaviour
	{
		// Use this for initialization
		void Start()
		{

		}
		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				GameManager.instance.StartGame(0);
			}
		}
		void OnTriggerEnter(Collider other)
		{
			//if (other.gameObject.tag.Equals("walter", StringComparison.InvariantCultureIgnoreCase))
			//{
			var gameManager = TestGameManager.instance;
			gameManager.StartGame(0);
			//}
		}
	}
}