using System.Reflection;
using Mirror;
using UnityEngine;
public class CharacterStats : NetworkBehaviour
{
	[field: SyncVar] public int Speed { get; private set; }
	[field: SyncVar] public int Strength { get; private set; }
	[field: SyncVar] public int Health { get; private set; }
	[SyncVar] int levelUpXp;
	[field: SyncVar] public int CanLevelUp { get; private set; }

	public void Awake()
	{
		Speed = 1;
		Strength = 1;
		Health = 1;
		levelUpXp = 0;
		CanLevelUp = 0;
	}

	public void AddXp()
	{
		levelUpXp++;
		if (levelUpXp < Speed + Strength + Health)
		{
			return;
		}
		
		levelUpXp = 0;
		CanLevelUp++;
		Debug.Log("Leveled Up");
	}

	public void LevelUpProperty(string propertyName)
	{
		if (propertyName == null)
		{
			return;
		}

		if (isServer)
		{
			SrvLevelUpProperty(propertyName);
		}
		else
		{
			CmdLevelUpProperty(propertyName);
		}
	}
	
	[Command]
	void CmdLevelUpProperty(string propertyName) => SrvLevelUpProperty(propertyName);

	[Server]
	void SrvLevelUpProperty(string propertyName)
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
