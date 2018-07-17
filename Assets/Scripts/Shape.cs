using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for handling information about objects (Row, Column, Type, BonusType)
/// </summary>
public class Shape : MonoBehaviour
{
	/// <summary>
	/// Bonustype of the object
	/// </summary>
	public BonusType Bonus { get; set; }

	/// <summary>
	/// Column of the object
	/// </summary>
	public int Column { get; set; }

	/// <summary>
	/// Row of the object
	/// </summary>
	public int Row { get; set; }

	/// <summary>
	/// Type of the object
	/// </summary>
	public string Type { get; set; }

	/// <summary>
	/// Constructor
	/// </summary>
	public Shape()
	{
		Bonus = BonusType.None;
	}

	/// <summary>
	/// Check if two shapes are of the same type
	/// </summary>
	/// <param name="otherShape">Shape to be compared to the current shape</param>
	/// <returns>True if both shapes are of the same type. Else, false.</returns>
	public bool IsSameType(Shape otherShape)
	{
		if ((otherShape == null) || !(otherShape is Shape))
		{
			throw new ArgumentException("otherShape");
		}

		return string.Compare(Type, (otherShape as Shape).Type) == 0;
	}

	/// <summary>
	/// Assignes information to the object
	/// </summary>
	/// <param name="type">Type of the object</param>
	/// <param name="row">Row of the object</param>
	/// <param name="column">Column of the object</param>
	/// <param name="bonusType">BonusType of the object</param>
	public void Assign(string type, int row, int column, BonusType bonusType = BonusType.None)
	{
		if (string.IsNullOrEmpty(type))
		{
			throw new ArgumentException("type");
		}

		Type = type;
		Row = row;
		Column = column;
		Bonus = bonusType;
	}

	/// <summary>
	/// Swaps the column and row information for two shapes
	/// </summary>
	/// <param name="a">First shape to be swapped</param>
	/// <param name="b">Second shape to be swapped</param>
	public static void SwapColumnRow(Shape a, Shape b)
	{
		int temp = a.Row;
		a.Row = b.Row;
		b.Row = temp;

		temp = a.Column;
		a.Column = b.Column;
		b.Column = temp;
	}
}
