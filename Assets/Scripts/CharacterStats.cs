using Mirror;
public class CharacterStats : NetworkBehaviour
{
	// Basic abilities
	[field: SyncVar] public float Speed { get; private set; }
	[field: SyncVar] public float Strength { get; private set; }
	[field: SyncVar] public float Agility { get; private set; }

	// Extra abilities
	// public float sneak { get; private set; }

	public CharacterStats(float initialSpeed = 1f, float initialStrength = 1f, float initialAgility = 1f)
	{
		Speed = initialSpeed;
		Strength = initialStrength;
		Agility = initialAgility;
	}
}
