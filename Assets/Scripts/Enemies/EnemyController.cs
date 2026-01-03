using System;
using Controllers;
using Enemies.EnemyStates;
using Helpers;
using Mirror;
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
		[SyncVar] public string stateString;

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

		[Server]
		void OnEnable()
		{
			State = new IdleState(this, 0f);
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
				stateString = State.ToString();
				State.Update(this);
			}
		}
	}
}
