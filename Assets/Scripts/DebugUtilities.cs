using System;
using UnityEngine;

/// <summary>
/// Class which helps with debugging the code
/// </summary>
public class DebugUtilities : MonoBehaviour
{
	/// <summary>
	/// Reads the premade level information from /Resources/level.txt and saves it in a string array
	/// </summary>
	/// <returns>string array with premade level information</returns>
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

	/// <summary>
	/// Create a string with the content of the shapes array
	/// </summary>
	/// <param name="shapes">shapes array</param>
	/// <returns>returns the string with the shapes array content</returns>
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

	/// <summary>
	/// Writes the result of GetArrayContents function to the Debug Console
	/// </summary>
	/// <param name="shapes">shapes array</param>
	public static void ShowArray(ShapesArray shapes)
	{
		Debug.Log(GetArrayContents(shapes));
	}
}
