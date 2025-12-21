using System.Reflection;
using Mirror;
using UnityEngine;
public class CharacterStats : NetworkBehaviour
{
	// Basic abilities
	[field: SyncVar] public int Speed { get; private set; }
	[field: SyncVar] public int Strength { get; private set; }
	[field: SyncVar] public int Agility { get; private set; }
	[field: SyncVar] public int LevelUpXp { get; private set; }
	[field: SyncVar] public int CanLevelUp { get; private set; }

	// Extra abilities
	// public float sneak { get; private set; }

	public void Start()
	{
		Speed = 1;
		Strength = 1;
		Agility = 1;
		LevelUpXp = 0;
		CanLevelUp = 0;
	}

	public void AddXp()
	{
		LevelUpXp++;
		if (LevelUpXp < Speed + Strength + Agility)
		{
			return;
		}
		
		LevelUpXp = 0;
		CanLevelUp++;
		Debug.Log("Leveled Up");
	}

	public void LevelUpProperty(string propertyName)
	{
		PropertyInfo propertyInfo = typeof(CharacterStats).GetProperty(propertyName);
		if (!int.TryParse(propertyInfo?.GetValue(this).ToString(), out int value))
		{
			value = 0;
		}
		
		propertyInfo?.SetValue(this, ++value);
		CanLevelUp--;
	}
}
