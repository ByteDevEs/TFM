using Controllers;
using UnityEngine;
namespace Enemies.EnemyStates
{
	public class AttackState : IEnemyState
	{
		AttackController attackController;
		MovementController movementController;
		readonly GameObject target;

		float timer;
		const float MaxTime = 5f;

		public AttackState(EnemyController enemyController, GameObject target)
		{
			this.target = target;
			timer = MaxTime;
			Start(enemyController);
		}
		
		public void Start(EnemyController enemyController)
		{
			attackController = enemyController.GetComponent<AttackController>();
			movementController = enemyController.GetComponent<MovementController>();
		}
		
		public void Update(EnemyController enemyController)
		{
			if (!target || !movementController)
			{
				attackController.SrvStopAttacking();
				enemyController.State = new IdleState(enemyController, 0f);
				return;
			}
			
			if (movementController.Destination != Vector3.zero
			    && Vector3.Distance(movementController.Destination, target.transform.position) > enemyController.DetectionRadius)
			{
				timer -= Time.deltaTime;

				if (timer <= 0f)
				{
					attackController.SrvStopAttacking();
					enemyController.State = new IdleState(enemyController, 0f);
				}
				
				return;
			}

			timer = MaxTime;

			if (target.GetComponent<PlayerController>().IsDead)
			{
				attackController.SrvStopAttacking();
				enemyController.State = new IdleState(enemyController, 0f);
				return;
			}

			attackController.SrvEnemyAttack(target);
		}
	}
}
