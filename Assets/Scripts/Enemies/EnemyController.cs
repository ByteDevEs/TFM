using Controllers;
using Enemies.EnemyStates;
using Helpers;
using UnityEngine;

namespace Enemies
{
	[RequireComponent(typeof(MovementController), typeof(AttackController))]
	public class EnemyController : HoverableObject
	{
		public float detectionRadius;
		public float detectionAngle;
		
		public IEnemyState state;

		new void Start()
		{
			state = new IdleState(this);
			AttackController attackController = GetComponent<AttackController>();
			attackController.SwapWeapons(Prefabs.GetInstance().weaponPool[Random.Range(0, Prefabs.GetInstance().weaponPool.Count)]);
			
			base.Start();
		}
		
		void Update()
		{
			if (isServer)
			{
				state.Update(this);
			}
		}
	}
}
