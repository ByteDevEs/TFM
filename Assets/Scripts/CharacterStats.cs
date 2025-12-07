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
}
