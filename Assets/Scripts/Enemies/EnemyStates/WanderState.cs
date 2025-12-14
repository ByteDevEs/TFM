using Controllers;
using UnityEngine;
namespace Enemies.EnemyStates
{
	public class WanderState : IEnemyState
	{
		MovementController movementController;

		public WanderState(EnemyController enemyController)
		{
			Start(enemyController);
		}
		
		public void Start(EnemyController enemyController)
		{
			movementController = enemyController.GetComponent<MovementController>();
			const int maxTries = 100;
			LayerMask groundLayerMask = LayerMask.GetMask("Ground");
			
			for (int tries = 0; tries < maxTries; tries++)
			{
				Vector2 randomDistance = Random.insideUnitCircle * Random.Range(1.0f, 10.0f);
				Vector3 surroundingPosition = enemyController.transform.position + new Vector3(randomDistance.x, 0, randomDistance.y);

				Vector3 upperVector = surroundingPosition + Vector3.up * 9f;
				
				Physics.Raycast(upperVector, surroundingPosition - upperVector, out RaycastHit raycastHit, 10.0f, groundLayerMask);

				if (!raycastHit.collider)
				{
					continue;
				}
				
				movementController.SrvMove(raycastHit.point);
				return;
			}
		}
		
		public void Update(EnemyController enemyController)
		{
			IEnemyState.LocatePlayers(enemyController);

			if (movementController.RemainingDistance <= movementController.StoppingDistance)
			{
				enemyController.State = new IdleState(enemyController);
			}
		}
	}
}
