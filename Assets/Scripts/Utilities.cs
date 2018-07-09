using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class Utilities
{
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


	public static bool AreVerticalOrHorizontalNeighbors(Shape s1, Shape s2)
	{
		return (((s1.Column == s2.Column) || (s1.Row == s2.Row)) && (Mathf.Abs(s1.Column - s2.Column) <= 1) && (Mathf.Abs(s1.Row - s2.Row) <= 1));
	}

	public static IEnumerable<GameObject> GetPotentialMatches(ShapesArray shapes)
	{
		// list that will contain all matches we find
		List<List<GameObject>> match3 = new List<List<GameObject>>();
		List<List<GameObject>> match4 = new List<List<GameObject>>();
		List<List<GameObject>> match5 = new List<List<GameObject>>();
		List<List<GameObject>> match6 = new List<List<GameObject>>();
		List<List<GameObject>> match7 = new List<List<GameObject>>();
		List<List<GameObject>> match8 = new List<List<GameObject>>();

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

					if (match.Count == 3)
					{
						match3.Add(match);
					}
					else if (match.Count == 4)
					{
						match4.Add(match);
					}
					else if (match.Count == 5)
					{
						match5.Add(match);
					}
					else if (match.Count == 6)
					{
						match6.Add(match);
					}
					else if (match.Count == 7)
					{
						match7.Add(match);
					}
					else if (match.Count == 8)
					{
						match8.Add(match);
					}
				}

				// tutorial code below commented out.
				// todo: check if optimization is really needed.

				//// if matches >= 3 OR we are in the middle of the calcuation loop and we have less than 3 matches -> return a random one
				//if ((matches.Count >= 3) ||
				//	((row >= Constants.Rows / 2) && (matches.Count > 0) && (matches.Count <= 2)))
				//{
				//	return matches[UnityEngine.Random.Range(0, matches.Count - 1)];
				//}
			}
		}

		// return random found match
		if (match8.Count > 0)
		{
			return match8[UnityEngine.Random.Range(0, match8.Count - 1)];
		}
		else if (match7.Count > 0)
		{
			return match7[UnityEngine.Random.Range(0, match7.Count - 1)];
		}
		else if (match6.Count > 0)
		{
			return match6[UnityEngine.Random.Range(0, match6.Count - 1)];
		}
		else if (match5.Count > 0)
		{
			return match5[UnityEngine.Random.Range(0, match5.Count - 1)];
		}
		else if (match4.Count > 0)
		{
			return match4[UnityEngine.Random.Range(0, match4.Count - 1)];
		}
		else if (match3.Count > 0)
		{
			return match3[UnityEngine.Random.Range(0, match3.Count - 1)];
		}
		else
		{
			return null;
		}
	}

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
