public enum GameState
{
	None,
	SelectionStarted,
	Animating
}

public enum BonusType
{
	None,
	DestroyWholeRowColumn
}

public static class BonusTypeUtilities
{
	public static bool ContainsDestroyWholeRowColumn(BonusType bt)
	{
		return (bt & BonusType.DestroyWholeRowColumn) == BonusType.DestroyWholeRowColumn;
	}
}

