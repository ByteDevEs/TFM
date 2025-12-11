using System.Reflection;
using Mirror;
public class CharacterStats : NetworkBehaviour
{
	// Basic abilities
	[field: SyncVar] public float Speed { get; private set; }
	[field: SyncVar] public float Strength { get; private set; }
	[field: SyncVar] public float Agility { get; private set; }

	// Extra abilities
	// public float sneak { get; private set; }

	public void Start()
	{
		Speed = 1f;
		Strength = 1f;
		Agility = 1f;
	}

	public void LevelUpProperty(string propertyName)
	{
		PropertyInfo propertyInfo = typeof(CharacterStats).GetProperty(propertyName);
		if (!int.TryParse(propertyInfo?.GetValue(this).ToString(), out int value))
		{
			value = 0;
		}
		
		propertyInfo?.SetValue(this, ++value);
	}
}
