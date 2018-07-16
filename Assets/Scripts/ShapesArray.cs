using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapesArray
{
	private GameObject[,] shapes = new GameObject[Constants.Rows, Constants.Columns];
	private GameObject backupG1;
	private GameObject backupG2;

	public GameObject this[int row, int column]
	{
		get
		{
			// throws an error if index is out of array range
			try
			{
				return shapes[row, column];
			}
			catch (Exception ex)
			{
				Debug.Log(row + " " + column);
				throw ex;
			}
		}
		set
		{
			shapes[row, column] = value;
		}
	}

	public bool IsSameType(int row, int column, int rowOffset, int columnOffset)
	{
		string type1 = shapes[row, column].GetComponent<Shape>().Type;
		string type2 = shapes[row + rowOffset, column + columnOffset].GetComponent<Shape>().Type;
		return string.Compare(type1, type2) == 0;
	}

	public void Swap(GameObject g1, GameObject g2)
	{
		// hold a backup in case no match is produced
		backupG1 = g1;
		backupG2 = g2;

		var g1Shape = g1.GetComponent<Shape>();
		var g2Shape = g2.GetComponent<Shape>();

		// get array indexes
		int g1Row = g1Shape.Row;
		int g1Column = g1Shape.Column;
		int g2Row = g2Shape.Row;
		int g2Column = g2Shape.Column;

		// swap them in the array
		var temp = shapes[g1Row, g1Column];
		shapes[g1Row, g1Column] = shapes[g2Row, g2Column];
		shapes[g2Row, g2Column] = temp;

		// set ultimate bonus type. needed for the rare case of combining an ultimate with the last object of a type which has no bonus type
		if (g1Shape.Type == "Ultimate")
		{
			g1Shape.Bonus = BonusType.Ultimate;
		}
		if (g2Shape.Type == "Ultimate")
		{
			g2Shape.Bonus = BonusType.Ultimate;
		}

		// swap their respective properties
		Shape.SwapColumnRow(g1Shape, g2Shape);
	}

	public void UndoSwap()
	{
		if ((backupG1 == null) || (backupG2 == null))
		{
			throw new Exception("Backup is null");
		}

		Swap(backupG1, backupG2);
	}

	public List<MatchesInfo> GetMatchesInfos(IEnumerable<GameObject> gos)
	{
		List<MatchesInfo> matchesInfos = new List<MatchesInfo>();

		foreach (var item in gos)
		{
			matchesInfos.Add(GetMatches(item));
		}

		return matchesInfos;
	}

	private IEnumerable<GameObject> ContainsBonus(IEnumerable<GameObject> matches)
	{
		List<GameObject> candiesWithBonus = new List<GameObject>();

		foreach (var item in matches)
		{
			if (item.GetComponent<Shape>().Bonus != BonusType.None)
			{
				candiesWithBonus.Add(item);
			}
		}

		return candiesWithBonus.Distinct();
	}

	private IEnumerable<GameObject> GetEntireRow(GameObject go)
	{
		List<GameObject> matches = new List<GameObject>();
		int row = go.GetComponent<Shape>().Row;
		for (int column = 0; column < Constants.Columns; column++)
		{
			matches.Add(shapes[row, column]);
		}

		return matches;
	}

	private IEnumerable<GameObject> GetEntireColumn(GameObject go)
	{
		List<GameObject> matches = new List<GameObject>();
		int column = go.GetComponent<Shape>().Column;
		for (int row = 0; row < Constants.Rows; row++)
		{
			matches.Add(shapes[row, column]);
		}

		return matches;
	}

	private IEnumerable<GameObject> GetBombRadius(GameObject go, int radius)
	{
		int goRow = go.GetComponent<Shape>().Row;
		int goColumn = go.GetComponent<Shape>().Column;

		List<GameObject> matches = new List<GameObject>();

		for (int rowIndex = -1 * radius; rowIndex <= 1 * radius; rowIndex++)
		{
			for (int columnIndex = -1 * radius; columnIndex <= 1 * radius; columnIndex++)
			{
				if (!((rowIndex == 0) && (columnIndex == 0)))
				{
					int newRow = goRow + rowIndex;
					int newColumn = goColumn + columnIndex;
					if ((newRow >= 0) && (newRow < Constants.Rows) && (newColumn >= 0) && (newColumn < Constants.Columns))
					{
						matches.Add(shapes[newRow, newColumn]);
					}
				}
			}
		}

		return matches;
	}

	public MatchesInfo GetAllShapes()
	{
		MatchesInfo matchesInfo = new MatchesInfo();

		foreach (var item in shapes)
		{
			matchesInfo.AddObject(item);
		}

		return matchesInfo;
	}

	public MatchesInfo GetMatches(GameObject go, GameObject go2 = null)
	{
		BonusType goBonus = go.GetComponent<Shape>().Bonus;
		BonusType go2Bonus = BonusType.None;
		if (go2 != null)
		{
			go2Bonus = go2.GetComponent<Shape>().Bonus;
		}

		MatchesInfo matchesInfo = new MatchesInfo();
		matchesInfo.OriginGameObject = go;

		bool[,] bonusUsed = new bool[Constants.Rows, Constants.Columns];
		for (int i = 0; i < Constants.Rows; i++)
		{
			for (int j = 0; j < Constants.Columns; j++)
			{
				bonusUsed[i, j] = false;
			}
		}
		if (goBonus == BonusType.Ultimate)
		{
			if (go2Bonus == BonusType.Ultimate)
			{
				matchesInfo = GetAllShapes();
			}
		}
		else
		{
			if (go2Bonus == BonusType.Ultimate)
			{
				matchesInfo.AddObject(go2);
				foreach (var item in shapes)
				{
					if (item.GetComponent<Shape>().Type == go.GetComponent<Shape>().Type)
					{
						matchesInfo.AddObject(item);
					}
				}
			}
			else if ((goBonus == BonusType.Bomb) && (go2Bonus == BonusType.Bomb))
			{
				go.GetComponent<Shape>().Bonus = BonusType.None;
				go2.GetComponent<Shape>().Bonus = BonusType.None;
				matchesInfo.AddObjectRange(GetBombRadius(go, 2));
				matchesInfo.AddObjectRange(GetBombRadius(go2, 2));
			}
			else if (((goBonus == BonusType.Vertical) || (goBonus == BonusType.Horizontal)) &&
					 ((go2Bonus == BonusType.Vertical) || (go2Bonus == BonusType.Horizontal)))
			{
				go.GetComponent<Shape>().Bonus = BonusType.None;
				go2.GetComponent<Shape>().Bonus = BonusType.None;
				matchesInfo.AddObjectRange(GetEntireColumn(go));
				matchesInfo.AddObjectRange(GetEntireRow(go));
			}
			else if (((goBonus == BonusType.Vertical) || (goBonus == BonusType.Horizontal)) &&
					 (go2Bonus == BonusType.Bomb))
			{
				matchesInfo.AddObjectRange(GetBombAndVerticalOrHorizontal(go, go2));
			}
			else if ((goBonus == BonusType.Bomb) &&
					 ((go2Bonus == BonusType.Vertical) || (go2Bonus == BonusType.Horizontal)))
			{
				matchesInfo.AddObjectRange(GetBombAndVerticalOrHorizontal(go, go2));
			}
			else
			{
				var horizontalMatches = GetMatchesHorizontally(go);
				matchesInfo.AddObjectRange(horizontalMatches);
				matchesInfo.HorizontalMatches = horizontalMatches.Count();

				var verticalMatches = GetMatchesVertically(go);
				matchesInfo.AddObjectRange(verticalMatches);
				matchesInfo.VerticalMatches = verticalMatches.Count();
			}

			bool containsBonuses = false;

			do
			{
				containsBonuses = false;

				List<GameObject> temp = new List<GameObject>(matchesInfo.MatchedCandy);
				foreach (var item in temp)
				{
					Shape shape = item.GetComponent<Shape>();
					switch (shape.Bonus)
					{
						case BonusType.Horizontal:
							bonusUsed[shape.Row, shape.Column] = true;
							shape.Bonus = BonusType.None;
							matchesInfo.AddObjectRange(GetEntireRow(item));
							break;

						case BonusType.Vertical:
							bonusUsed[shape.Row, shape.Column] = true;
							shape.Bonus = BonusType.None;
							matchesInfo.AddObjectRange(GetEntireColumn(item));
							break;

						case BonusType.Bomb:
							bonusUsed[shape.Row, shape.Column] = true;
							shape.Bonus = BonusType.None;
							matchesInfo.AddObjectRange(GetBombRadius(item, 1));
							break;

						case BonusType.Ultimate:
							bonusUsed[shape.Row, shape.Column] = true;

							break;

						default:
							break;
					}
				}

				foreach (var item in matchesInfo.MatchedCandy)
				{
					Shape shape = item.GetComponent<Shape>();

					if (bonusUsed[shape.Row, shape.Column] == true)
					{
						shape.Bonus = BonusType.None;
					}

					if (shape.Bonus != BonusType.None)
					{
						containsBonuses = true;
					}
				}
			} while (containsBonuses == true);
		}

		return matchesInfo;
	}

	private List<GameObject> GetBombAndVerticalOrHorizontal(GameObject go, GameObject go2)
	{
		List<GameObject> matches = new List<GameObject>();

		int goRow = go.GetComponent<Shape>().Row;
		int goColumn = go.GetComponent<Shape>().Column;

		go.GetComponent<Shape>().Bonus = BonusType.None;
		go2.GetComponent<Shape>().Bonus = BonusType.None;

		matches.AddRange(GetEntireRow(go));
		matches.AddRange(GetEntireColumn(go));

		for (int rowIndex = -1; rowIndex <= 1; rowIndex += 2)
		{
			int newRow = goRow + rowIndex;
			if ((newRow >= 0) && (newRow < Constants.Rows))
			{
				matches.AddRange(GetEntireRow(shapes[newRow, goColumn]));
			}
		}
		for (int columnIndex = -1; columnIndex <= 1; columnIndex += 2)
		{
			int newColumn = goColumn + columnIndex;
			if ((newColumn >= 0) && (newColumn < Constants.Columns))
			{
				matches.AddRange(GetEntireColumn(shapes[goRow, newColumn]));
			}
		}

		return matches;
	}

	private IEnumerable<GameObject> GetMatchesHorizontally(GameObject go)
	{
		List<GameObject> matches = new List<GameObject>();
		matches.Add(go);
		var shape = go.GetComponent<Shape>();
		// check left
		if (shape.Column != 0)
		{
			for (int column = shape.Column - 1; column >= 0; column--)
			{
				if ((shapes[shape.Row, column] != null) && (shapes[shape.Row, column].GetComponent<Shape>().IsSameType(shape)))
				{
					matches.Add(shapes[shape.Row, column]);
				}
				else
				{
					break;
				}
			}
		}

		// check right
		if (shape.Column != Constants.Columns - 1)
		{
			for (int column = shape.Column + 1; column < Constants.Columns; column ++)
			{
				if ((shapes[shape.Row, column] != null) && (shapes[shape.Row, column].GetComponent<Shape>().IsSameType(shape)))
				{
					matches.Add(shapes[shape.Row, column]);
				}
				else
				{
					break;
				}
			}
		}

		// we want more than three matches
		if (matches.Count < Constants.MinimumMatches)
		{
			matches.Clear();
		}

		return matches.Distinct();
	}

	private IEnumerable<GameObject> GetMatchesVertically(GameObject go)

	{
		List<GameObject> matches = new List<GameObject>();
		matches.Add(go);
		var shape = go.GetComponent<Shape>();
		// check bottom
		if (shape.Row != 0)
		{
			for (int row = shape.Row - 1; row >= 0; row--)
			{
				if ((shapes[row, shape.Column] != null) && (shapes[row, shape.Column].GetComponent<Shape>().IsSameType(shape)))
				{
					matches.Add(shapes[row, shape.Column]);
				}
				else
				{
					break;
				}
			}
		}
		// check top
		if (shape.Row != Constants.Rows - 1)
		{
			for (int row = shape.Row + 1; row < Constants.Rows; row++)
			{
				if ((shapes[row, shape.Column] != null) && (shapes[row, shape.Column].GetComponent<Shape>().IsSameType(shape)))
				{
					matches.Add(shapes[row, shape.Column]);
				}
				else
				{
					break;
				}
			}
		}

		if (matches.Count < Constants.MinimumMatches)
		{
			matches.Clear();
		}

		return matches.Distinct();
	}

	public void Remove(GameObject item)
	{
		shapes[item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column] = null;
	}

	public AlteredCandyInfo Collapse(IEnumerable<int> columns)
	{
		AlteredCandyInfo collapseInfo = new AlteredCandyInfo();

		// search in every column
		foreach (var column in columns)
		{
			// begin from bottom row
			for (int row = 0; row < Constants.Rows - 1; row++)
			{
				// null item found
				if (shapes[row, column] == null)
				{
					// start searching for the first non-null item
					for (int row2 = row + 1; row2 < Constants.Rows; row2++)
					{
						// let it fall down
						if (shapes[row2, column] != null)
						{
							shapes[row, column] = shapes[row2, column];
							shapes[row2, column] = null;

							// assign new row and column
							shapes[row, column].GetComponent<Shape>().Row = row;
							shapes[row, column].GetComponent<Shape>().Column = column;

							collapseInfo.AddCandy(shapes[row, column]);
							break;
						}
					}
				}
			}
		}

		return collapseInfo;
	}

	public IEnumerable<ShapeInfo> GetEmptyItemsOnColumn(int column)
	{
		List<ShapeInfo> emptyItems = new List<ShapeInfo>();
		for (int row = 0; row < Constants.Rows; row++)
		{
			if (shapes[row, column] == null)
			{
				emptyItems.Add(new ShapeInfo() { Row = row, Column = column });
			}
		}

		return emptyItems;
	}
}
