using Controllers;
using Enemies.EnemyStates;
using Helpers;
using UnityEngine;
using Weapons;

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
			base.Start();
			
			if (!isServer)
			{
				return;
			}
			
			State = new IdleState(this);
			AttackController attackController = GetComponent<AttackController>();
			attackController.SwapWeapons(WeaponLibrary.GetRandomWeaponID());
		}

		public override void RemoveEffect()
		{
			if (!IsHovering)
			{
				return;
			}

			IsHovering = false;
			foreach (GameObject child in Children)
			{
				child.layer = LayerMask.NameToLayer("Enemy");
			}
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
