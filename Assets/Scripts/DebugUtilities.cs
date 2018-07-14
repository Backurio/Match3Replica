﻿using System;
using UnityEngine;

public class DebugUtilities : MonoBehaviour
{
	public static string[,] FillShapesArrayFromResourceData()
	{
		string[,] shapes = new string[Constants.Rows, Constants.Columns];

		TextAsset txt = Resources.Load("level") as TextAsset;
		string level = txt.text;

		string[] lines = level.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		for (int row = Constants.Rows - 1; row >= 0 ; row--)
		{
			string[] items = lines[row].Split('|');
			for (int column = 0; column < Constants.Columns; column++)
			{
				shapes[row, column] = items[column];
			}
		}

		return shapes;
	}

	public static void DebugRotate(GameObject go)
	{
		go.transform.Rotate(0.0f, 0.0f, 80.0f);
	}

	public static void DebugAlpha(GameObject go)
	{
		Color c = go.GetComponent<SpriteRenderer>().color;
		c.a = 0.6f;
		go.GetComponent<SpriteRenderer>().color = c;
	}

	public static void DebugPositions(GameObject hitGo, GameObject hitGo2)
	{
		string lala = hitGo.GetComponent<Shape>().Row + "-" +
					  hitGo.GetComponent<Shape>().Column + "-" +
					  hitGo2.GetComponent<Shape>().Row + "-" +
					  hitGo2.GetComponent<Shape>().Column;
		print(lala);
	}

	public static string GetArrayContents(ShapesArray shapes)
	{
		string x = string.Empty;
		for (int row = Constants.Rows - 1; row >= 0; row--)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				if (shapes[row, column] == null)
				{
					x += "NULL  |";
				}
				else
				{
					var shape = shapes[row, column].GetComponent<Shape>();
					x += shape.Row.ToString("D2") + "-" + shape.Column.ToString("D2");
					x += shape.Type.Substring(5, 2);
					x += " ";
					x += " | ";
				}
			}

			x += Environment.NewLine;
		}

		return x;
	}

	public static void ShowArray(ShapesArray shapes)
	{
		Debug.Log(GetArrayContents(shapes));
	}
}
