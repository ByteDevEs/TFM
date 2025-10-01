public struct CharacterStats
{
	// Basic abilities
	public float speed { get; private set; }
	public float strength { get; private set; }
	public float agility { get; private set; }

	// Extra abilities
	// public float sneak { get; private set; }

	public CharacterStats(float initialSpeed = 1f, float initialStrength = 1f, float initialAgility = 1f)
	{
		speed = initialSpeed;
		strength = initialStrength;
		agility = initialAgility;
	}
}
