using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
	public BonusType Bonus { get; set; }
	public int Column { get; set; }
	public int Row { get; set; }

	public string Type { get; set; }

	public Shape()
	{
		Bonus = BonusType.None;
	}

	public bool IsSameType(Shape otherShape)
	{
		if ((otherShape == null) || !(otherShape is Shape))
		{
			throw new ArgumentException("otherShape");
		}

		return string.Compare(Type, (otherShape as Shape).Type) == 0;
	}

	public void Assign(string type, int row, int column)
	{
		if (string.IsNullOrEmpty(type))
		{
			throw new ArgumentException("type");
		}

		Type = type;
		Row = row;
		Column = column;
	}

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
