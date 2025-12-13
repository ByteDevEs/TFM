using Controllers;
using Enemies.EnemyStates;
using Helpers;
using UnityEngine;

namespace Enemies
{
	[RequireComponent(typeof(MovementController), typeof(AttackController))]
	public class EnemyController : HoverableObject
	{
		public float DetectionRadius;
		public float DetectionAngle;
		
		public IEnemyState State;

		new void Start()
		{
			State = new IdleState(this);
			AttackController attackController = GetComponent<AttackController>();
			attackController.SwapWeapons(Prefabs.GetInstance().WeaponPool[Random.Range(0, Prefabs.GetInstance().WeaponPool.Count)]);
			
			base.Start();
		}
		
		void Update()
		{
			if (isServer)
			{
				State.Update(this);
			}
		}
	}
}
