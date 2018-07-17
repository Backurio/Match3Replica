/// <summary>
/// Enum for the game state machine
/// </summary>
public enum GameState
{
	None,
	SelectionStarted,
	Animating
}

/// <summary>
/// Enum for the bonus types
/// </summary>
public enum BonusType
{
	None,
	Horizontal,
	Vertical,
	Bomb,
	Ultimate
}

/// <summary>
/// Enum for the object types
/// </summary>
public enum ObjectType
{
	None,
	Coin,
	Gold,
	Iron,
	Stone,
	Wood,
	Ultimate
}

/// <summary>
/// Enum for the explosion types
/// </summary>
public enum ExplosionType
{
	Object,
	Vertical,
	Horizontal,
	Bomb
}