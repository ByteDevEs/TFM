using UnityEngine;
namespace Enemies.EnemyStates
{
	public class IdleState : IEnemyState
	{
		float? timeToWait;

		public IdleState(EnemyController enemyController, float? t = null)
		{
			timeToWait = t;
			Start(enemyController);
		}
		
		public void Start(EnemyController enemyController)
		{
			timeToWait ??= Random.Range(5.0f, 10.0f);
		}
		
		public void Update(EnemyController enemyController)
		{
			IEnemyState.LocatePlayers(enemyController);
			
			timeToWait -= Time.deltaTime;

			if (timeToWait <= 0)
			{
				enemyController.state = new WanderState(enemyController);
			}
		}
	}
}
