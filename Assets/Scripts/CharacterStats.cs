public struct CharacterStats
{
	// Basic abilities
	public float Speed { get; private set; }
	public float Strength { get; private set; }
	public float Agility { get; private set; }

	// Extra abilities
	// public float sneak { get; private set; }

	public CharacterStats(float initialSpeed = 1f, float initialStrength = 1f, float initialAgility = 1f)
	{
		Speed = initialSpeed;
		Strength = initialStrength;
		Agility = initialAgility;
	}
}
