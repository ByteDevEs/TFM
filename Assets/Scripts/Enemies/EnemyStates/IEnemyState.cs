using Controllers;
using Mirror;
using UnityEngine;
namespace Enemies.EnemyStates
{
	public interface IEnemyState
	{
		[Server]
		void Start(EnemyController enemyController);
		[Server]
		public void Update(EnemyController enemyController);
		
		[Server]
		protected static void LocatePlayers(EnemyController enemyController)
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject player in players)
			{
				if (player.GetComponent<PlayerController>().IsDead)
				{
					continue;
				}
			
				Vector3 forward = enemyController.transform.forward;
				Vector3 direction = player.transform.position - enemyController.transform.position;
				float angle = Vector3.Angle(forward, direction);
				float distance = Vector3.Distance(enemyController.transform.position, player.transform.position);

				bool isInAngle = enemyController.DetectionAngle > angle;
				bool isInRadius = enemyController.DetectionRadius > distance;
				
				if (isInAngle && isInRadius)
				{
					enemyController.State = new AttackState(enemyController, player);
					return;
				}
			}
		}
	}
}
