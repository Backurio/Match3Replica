using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Class which handles the finding and animation of potential matches
/// </summary>
public static class Utilities
{
	/// <summary>
	/// Coroutine to animate potential matches
	/// </summary>
	/// <param name="potentialMatches">List of potential matches</param>
	/// <returns></returns>
	public static IEnumerator AnimatePotentialMatches(IEnumerable<GameObject> potentialMatches)
	{
		Vector3 candySize = Vector3.one * Constants.CandySize / 0.7f;

		foreach (var item in potentialMatches)
		{
			item.transform.DOScale(candySize * 1.2f, Constants.ScaleAnimationDuration);
		}
		yield return new WaitForSeconds(Constants.ScaleAnimationDuration);

		foreach (var item in potentialMatches)
		{
			item.transform.DOScale(candySize, Constants.ScaleAnimationDuration);
		}
		yield return new WaitForSeconds(Constants.ScaleAnimationDuration);
	}

	/// <summary>
	/// Checks if two objects are next to each other
	/// </summary>
	/// <param name="s1">First object to be checked</param>
	/// <param name="s2">Second object to be checked</param>
	/// <returns>True if both objects are next to each other. Else, false.</returns>
	public static bool AreVerticalOrHorizontalNeighbors(Shape s1, Shape s2)
	{
		return (((s1.Column == s2.Column) || (s1.Row == s2.Row)) && (Mathf.Abs(s1.Column - s2.Column) <= 1) && (Mathf.Abs(s1.Row - s2.Row) <= 1));
	}

	/// <summary>
	/// Checks entire play area for potential matches and picks the best one (most matched objects)
	/// </summary>
	/// <param name="shapes">shapes array</param>
	/// <returns>List of best objects in the best potential match</returns>
	public static IEnumerable<GameObject> GetPotentialMatches(ShapesArray shapes)
	{
		// array of list that will contain all matches we find, sorted by amount of involved objects
		// index 0 = 8 objects, index 5 = 3 objects
		List<List<GameObject>>[] matches = new List<List<GameObject>>[6];

		for (int i = 0; i < matches.Length; i++)
		{
			matches[i] = new List<List<GameObject>>();
		}

		for (int row = 0; row < Constants.Rows; row++)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				List<GameObject>[] foundmatches = new List<GameObject>[8];

				foundmatches[0] = CheckHorizontal1(row, column, shapes);
				foundmatches[1] = CheckHorizontal2(row, column, shapes);
				foundmatches[2] = CheckHorizontal3(row, column, shapes);
				foundmatches[3] = CheckHorizontal4(row, column, shapes);
				foundmatches[4] = CheckVertical1(row, column, shapes);
				foundmatches[5] = CheckVertical2(row, column, shapes);
				foundmatches[6] = CheckVertical3(row, column, shapes);
				foundmatches[7] = CheckVertical4(row, column, shapes);

				foreach (var match in foundmatches)
				{
					match.Add(shapes[row, column]);

					for (int i = 0; i < matches.Length; i++)
					{
						if ((match.Count > 2) && (match.Count == 8 - i))
						{
							matches[i].Add(match);
						}
					}
				}
			}
		}

		// return random found match
		for (int i = 0; i < matches.Length; i++)
		{
			if (matches[i].Count > 0)
			{
				return matches[i][UnityEngine.Random.Range(0, matches[i].Count - 1)];
			}
		}

		return null;
	}

	/// <summary>
	/// Check #1 for horizontal potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckHorizontal1(int row, int column, ShapesArray shapes)
	{
		List<GameObject> match = new List<GameObject>();

		if ((column <= Constants.Columns - 4) && shapes.IsSameType(row, column, 0, 3))
		{
			match.Add(shapes[row, column + 3]);

			if (shapes.IsSameType(row, column, 0, 1))
			{
				match.Add(shapes[row, column + 1]);
				/* * * * * *
				 * 1 3 * 2 *
				 * * * * * */

				if ((row <= Constants.Rows - 2) && shapes.IsSameType(row, column, 1, 2))
				{
					match.Add(shapes[row + 1, column + 2]);
					/* * * * * *
					 * * * 4 * *
					 * 1 3 * 2 *
					 * * * * * */

					if ((row <= Constants.Rows - 3) && shapes.IsSameType(row, column, 2, 2))
					{
						match.Add(shapes[row + 2, column + 2]);
						/* * * * * *
						 * * * 5 * *
						 * * * 4 * *
						 * 1 3 * 2 *
						 * * * * * */
					}

					if ((column <= Constants.Columns - 5) && shapes.IsSameType(row, column, 0, 4))
					{
						match.Add(shapes[row, column + 4]);
						/* * * * * * *
						 * * * ? * * *
						 * * * 4 * * *
						 * 1 3 * 2 5 *
						 * * * * * * */
					}
				}
				if ((row >= 1) && shapes.IsSameType(row, column, -1, 2))
				{
					match.Add(shapes[row - 1, column + 2]);
					/* * * * * * *
					 * * * ? * * *
					 * * * ? * * *
					 * 1 3 * 2 ? *
					 * * * 4 * * *
					 * * * * * * */

					if((row >= 2) && shapes.IsSameType(row, column, -2, 2))
					{
						match.Add(shapes[row - 2, column + 2]);
						/* * * * * * *
						 * * * ? * * *
						 * * * ? * * *
						 * 1 3 * 2 ? *
						 * * * 4 * * *
						 * * * 5 * * *
						 * * * * * * */
					}

					if ((column <= Constants.Columns - 5) && shapes.IsSameType(row, column, 0, 4))
					{
						match.Add(shapes[row, column + 4]);
						/* * * * * * *
						 * * * ? * * *
						 * * * ? * * *
						 * 1 3 * 2 5 *
						 * * * 4 * * *
						 * * * ? * * *
						 * * * * * * */
					}
				}
			}

			else if (shapes.IsSameType(row, column, 0, 2))
			{
				match.Add(shapes[row, column + 2]);
				/* * * * * *
				 * 1 * 3 2 *
				 * * * * * */

				if ((row <= Constants.Rows - 2) && shapes.IsSameType(row, column, 1, 1))
				{
					match.Add(shapes[row + 1, column + 1]);
					/* * * * * *
					 * * 4 * * *
		 			 * 1 * 3 2 *
		 			 * * * * * */

					if ((row <= Constants.Rows - 3) && shapes.IsSameType(row, column, 2, 1))
					{
						match.Add(shapes[row + 2, column + 1]);
						/* * * * * *
						 * * 5 * * *
						 * * 4 * * *
						 * 1 * 3 2 *
						 * * * * * */
					}
				}
				if ((row >= 1) && shapes.IsSameType(row, column, -1, 1))
				{
					match.Add(shapes[row - 1, column + 1]);
					/* * * * * *
					 * * ? * * *
					 * * ? * * *
					 * 1 * 3 2 *
					 * * 4 * * *
					 * * * * * */

					if ((row >= 2) && shapes.IsSameType(row, column, -2, 1))
					{
						match.Add(shapes[row - 2, column + 1]);
						/* * * * * *
						 * * ? * * *
						 * * ? * * *
						 * 1 * 3 2 *
						 * * 4 * * *
						 * * 5 * * *
						 * * * * * */
					}
				}
			}
		}

		return match;
	}

	/// <summary>
	/// Check #2 for horizontal potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckHorizontal2(int row, int column, ShapesArray shapes)
	{
		List<GameObject> match = new List<GameObject>();

		if ((column <= Constants.Columns - 3) && shapes.IsSameType(row, column, 0, 1))
		{
			match.Add(shapes[row, column + 1]);

			if ((row >= 1) && shapes.IsSameType(row, column, -1, 2))
			{
				match.Add(shapes[row - 1, column + 2]);
				/* * * * *
				 * 1 2 * *
				 * * * 3 *
				 * * * * */
			}

			else if ((row <= Constants.Rows - 2) && shapes.IsSameType(row, column, 1, 2))
			{
				match.Add(shapes[row + 1, column + 2]);
				/* * * * *
				 * * * 3 *
				 * 1 2 * *
				 * * * * */
			}
		}

		return match;
	}

	/// <summary>
	/// Check #3 for horizontal potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckHorizontal3(int row, int column, ShapesArray shapes)
	{
		List<GameObject> match = new List<GameObject>();

		if ((column >= 2) && shapes.IsSameType(row, column, 0, -1))
		{
			match.Add(shapes[row, column - 1]);

			if ((row >= 1) && shapes.IsSameType(row, column, -1, -2))
			{
				match.Add(shapes[row - 1, column - 2]);
				/* * * * *
				 * * 2 1 *
				 * 3 * * *
				 * * * * */
			}

			else if ((row <= Constants.Rows - 2)  && shapes.IsSameType(row, column, 1, -2))
			{
				match.Add(shapes[row + 1, column - 2]);
				/* * * * *
				 * 3 * * *
				 * * 2 1 *
				 * * * * */
			}
		}

		return match;
	}

	/// <summary>
	/// Check #4 for horizontal potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckHorizontal4(int row, int column, ShapesArray shapes)
	{
		List<GameObject> match = new List<GameObject>();

		if ((column <= Constants.Columns - 3) && shapes.IsSameType(row, column, 0, 2))
		{
			match.Add(shapes[row, column + 2]);

			if ((row <= Constants.Rows - 2) && shapes.IsSameType(row, column, 1, 1))
			{
				match.Add(shapes[row + 1, column + 1]);
				/* * * * *
				 * * 3 * *
				 * 1 * 2 *
				 * * * * */
			}

			else if ((row >= 1) && shapes.IsSameType(row, column, -1, 1))
			{
				match.Add(shapes[row - 1, column + 1]);
				/* * * * *
				 * 1 * 2 *
				 * * 3 * *
				 * * * * */
			}
		}

		return match;
	}

	/// <summary>
	/// Check #1 for vertical potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckVertical1(int row, int column, ShapesArray shapes)
	{
		List<GameObject> match = new List<GameObject>();

		if ((row <= Constants.Rows - 4) && shapes.IsSameType(row, column, 3, 0))
		{
			match.Add(shapes[row + 3, column]);

			if (shapes.IsSameType(row, column, 1, 0))
			{
				match.Add(shapes[row + 1, column]);
				/* * *
				 * 2 *
				 * * *
				 * 3 *
				 * 1 *
				 * * */

				if ((column <= Constants.Columns - 2) && shapes.IsSameType(row, column, 2, 1))
				{
					match.Add(shapes[row + 2, column + 1]);
					/* * * *
					 * 2 * *
					 * * 4 *
					 * 3 * *
					 * 1 * *
					 * * * */

					if ((column <= Constants.Columns - 3) && shapes.IsSameType(row, column, 2, 2))
					{
						match.Add(shapes[row + 2, column + 2]);
						/* * * * *
						 * 2 * * *
						 * * 4 5 *
						 * 3 * * *
						 * 1 * * *
						 * * * * */
					}

					if ((row <= Constants.Rows - 5) && shapes.IsSameType(row, column, 4, 0))
					{
						match.Add(shapes[row + 4, column]);
						/* * * * *
						 * 5 * * *
						 * 2 * * *
						 * * 4 ? *
						 * 3 * * *
						 * 1 * * *
						 * * * * */
					}
				}

				if ((column >= 1) && shapes.IsSameType(row, column, 2, -1))
				{
					match.Add(shapes[row + 2, column - 1]);
					/* * * * * *
					 * * ? * * *
					 * * 2 * * *
					 * 4 * ? ? *
					 * * 3 * * *
					 * * 1 * * *
					 * * * * * */

					if ((column >= 2) && shapes.IsSameType(row, column, 2, -2))
					{
						match.Add(shapes[row + 2, column - 2]);
						/* * * * * * *
						 * * * ? * * *
						 * * * 2 * * *
						 * 5 4 * ? ? *
						 * * * 3 * * *
						 * * * 1 * * *
						 * * * * * * */
					}

					if ((row <= Constants.Rows - 5) && shapes.IsSameType(row, column, 4, 0))
					{
						match.Add(shapes[row + 4, column]);
						/* * * * * * *
						 * * * 5 * * *
						 * * * 2 * * *
						 * ? 4 * ? ? *
						 * * * 3 * * *
						 * * * 1 * * *
						 * * * * * * */
					}
				}
			}

			else if (shapes.IsSameType(row, column, 2, 0))
			{
				match.Add(shapes[row + 2, column]);
				/* * *
				 * 2 *
				 * 3 *
				 * * *
				 * 1 *
				 * * */

				if ((column <= Constants.Columns - 2) && shapes.IsSameType(row, column, 1, 1))
				{
					match.Add(shapes[row + 1, column + 1]);
					/* * * *
					 * 2 * *
					 * 3 * *
					 * * 4 *
					 * 1 * *
					 * * * */

					if ((column <= Constants.Columns - 3) && shapes.IsSameType(row, column, 1, 2))
					{
						match.Add(shapes[row + 1, column + 2]);
						/* * * * *
						 * 2 * * *
						 * 3 * * *
						 * * 4 5 *
						 * 1 * * *
						 * * * * */
					}
				}

				if ((column >= 1) && shapes.IsSameType(row, column, 1, -1))
				{
					match.Add(shapes[row + 1, column - 1]);
					/* * * * * *
					 * * 2 * * *
					 * * 3 * * *
					 * 4 * ? ? *
					 * * 1 * * *
					 * * * * * */

					if ((column >= 2) && shapes.IsSameType(row, column, 1, -2))
					{
						match.Add(shapes[row + 1, column - 2]);
						/* * * * * * *
						 * * * 2 * * *
						 * * * 3 * * *
						 * 5 4 * ? ? *
						 * * * 1 * * *
						 * * * * * * */
					}
				}
			}
		}

		return match;
	}

	/// <summary>
	/// Check #2 for vertical potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckVertical2(int row, int column, ShapesArray shapes)
	{
		/* examples
		 * * * *	* * * *
		 * 3 * *	* * 3 *
		 * * 2 *	* 2 * *
		 * * 1 *	* 1 * *
		 * * * *	* * * *
		 */
						List<GameObject> match = new List<GameObject>();

		if ((row <= Constants.Rows - 3) && shapes.IsSameType(row, column, 1, 0))
		{
			match.Add(shapes[row + 1, column]);

			if ((column >= 1) && shapes.IsSameType(row, column, 2, -1))
			{
				match.Add(shapes[row + 2, column - 1]);
			}

			else if ((column <= Constants.Columns - 2) && shapes.IsSameType(row, column, 2, 1))
			{
				match.Add(shapes[row + 2, column + 1]);
			}
		}

		return match;
	}

	/// <summary>
	/// Check #3 for vertical potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckVertical3(int row, int column, ShapesArray shapes)
	{
		/* example
		 * * * *	* * * *
		 * * 1 *	* 1 * *
		 * * 2 *	* 2 * *
		 * 3 * *	* * 3 *
		 * * * *	* * * *
		 */
		List<GameObject> match = new List<GameObject>();

		if ((row >= 2) && shapes.IsSameType(row, column, -1, 0))
		{
			match.Add(shapes[row - 1, column]);

			if ((column >= 1) && shapes.IsSameType(row, column, -2, -1))
			{
				match.Add(shapes[row - 2, column - 1]);
			}

			else if ((column <= Constants.Columns - 2) && shapes.IsSameType(row, column, -2, 1))
			{
				match.Add(shapes[row - 2, column + 1]);
			}
		}

		return match;
	}

	/// <summary>
	/// Check #4 for vertical potential matches
	/// </summary>
	/// <param name="row">Row of the object from which the check will be performed</param>
	/// <param name="column">Column of the object from which the check will be performed</param>
	/// <param name="shapes">Shapes array</param>
	/// <returns>List of objects for a potential match</returns>
	public static List<GameObject> CheckVertical4(int row, int column, ShapesArray shapes)
	{
		/* example
		 * * * *     * * * *
		 * * 2 *	 * 2 * *
		 * 3 * *	 * * 3 *
		 * * 1 *	 * 1 * *
		 * * * *	 * * * *
		 */
		List<GameObject> match = new List<GameObject>();

		if ((row <= Constants.Rows - 3) && shapes.IsSameType(row, column, 2, 0))
		{
			match.Add(shapes[row + 2, column]);

			if ((column >= 1) && shapes.IsSameType(row, column, 1, -1))
			{
				match.Add(shapes[row + 1, column - 1]);
			}

			else if ((column <= Constants.Columns - 2) && shapes.IsSameType(row, column, 1, 1))
			{
				match.Add(shapes[row + 1, column + 1]);
			}
		}

		return match;
	}
}
