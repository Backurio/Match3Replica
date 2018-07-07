using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
	public static IEnumerator AnimatePotentialMatches(IEnumerable<GameObject> potentialMatches)
	{
		for (float i = 1.0f; i >= 0.3f; i -= 0.1f)
		{
			foreach (var item in potentialMatches)
			{
				Color c = item.GetComponent<SpriteRenderer>().color;
				c.a = i;
				item.GetComponent<SpriteRenderer>().color = c;
			}
			yield return new WaitForSeconds(Constants.OpacityAnimationFrameDelay);
		}

		for (float i = 0.3f; i <= 1.0f; i += 0.1f)
		{
			foreach (var item in potentialMatches)
			{
				Color c = item.GetComponent<SpriteRenderer>().color;
				c.a = i;
				item.GetComponent<SpriteRenderer>().color = c;
			}
			yield return new WaitForSeconds(Constants.OpacityAnimationFrameDelay);
		}
	}


	public static bool AreVerticalOrHorizontalNeighbors(Shape s1, Shape s2)
	{
		return (((s1.Column == s2.Column) || (s1.Row == s2.Row)) && (Mathf.Abs(s1.Column - s2.Column) <= 1) && (Mathf.Abs(s1.Row - s2.Row) <= 1));
	}

	public static IEnumerable<GameObject> GetPotentialMatches(ShapesArray shapes)
	{
		// list that will contain all matches we find
		List<List<GameObject>> matches = new List<List<GameObject>>();

		for (int row = 0; row < Constants.Rows; row++)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				List<GameObject>[] foundmatches = new List<GameObject>[6];

				foundmatches[0] = CheckHorizontal1(row, column, shapes);
				foundmatches[1] = CheckHorizontal2(row, column, shapes);
				foundmatches[2] = CheckHorizontal3(row, column, shapes);
				foundmatches[3] = CheckVertical1(row, column, shapes);
				foundmatches[4] = CheckVertical2(row, column, shapes);
				foundmatches[5] = CheckVertical3(row, column, shapes);


				foreach (var match in foundmatches)
				{
					if (match != null)
					{
						matches.Add(match);
					}
				}

				// if matches >= 2, return a random one
				if (matches.Count >= 2)
				{
					return matches[UnityEngine.Random.Range(0, matches.Count - 1)];
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

		return null;
	}

	public static List<GameObject> CheckHorizontal1(int row, int column, ShapesArray shapes)
	{
		if (column >= 2)
		{
			if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row, column - 1].GetComponent<Shape>()))
			{
				if (row >= 1)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row - 1, column - 2].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row, column - 1],
							shapes[row - 1, column - 2]
						};
						/* example
						 * * * * *
						 * * 2 1 *
						 * 3 * * *
						 * * * * *
						 */
					}
				}

				if (row <= Constants.Rows - 2)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 1, column - 2].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row, column - 1],
							shapes[row + 1, column - 2]
						};
						/* example
						 * * * * *
						 * 3 * * *
						 * * 2 1 *
						 * * * * *
						 */
					}
				}
			}
		}

		return null;
	}

	public static List<GameObject> CheckHorizontal2(int row, int column, ShapesArray shapes)
	{
		if (column <= Constants.Columns - 2)
		{
			if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row, column + 1].GetComponent<Shape>()))
			{
				if (row >= 1)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row - 1, column + 2].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row, column + 1],
							shapes[row - 1, column + 2]
						};
						/* example
						 * * * * *
						 * 1 2 * *
						 * * * 3 *
						 * * * * *
						 */
					}
				}

				if (row <= Constants.Rows - 2)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 1, column + 2].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row, column + 1],
							shapes[row + 1, column + 2]
						};
						/* example
						 * * * * *
						 * * * 3 *
						 * 1 2 * *
						 * * * * *
						 */
					}
				}
			}
		}

		return null;
	}

	public static List<GameObject> CheckHorizontal3(int row, int column, ShapesArray shapes)
	{
		if (column <= Constants.Columns - 4)
		{
			if ((shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row, column + 1].GetComponent<Shape>())) &&
				(shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row, column + 3].GetComponent<Shape>())))
			{
				return new List<GameObject>()
				{
					shapes[row, column],
					shapes[row, column + 1],
					shapes[row, column + 3]
				};
				/* example
				 * * * * * *
				 * 1 2 * 3 *
				 * * * * * *
				 */
			}

			if ((shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row, column + 2].GetComponent<Shape>())) &&
				(shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row, column + 3].GetComponent<Shape>())))
			{
				return new List<GameObject>()
				{
					shapes[row, column],
					shapes[row, column + 2],
					shapes[row, column + 3]
				};
				/* example
				 * * * * * *
				 * 1 * 2 3 *
				 * * * * * *
				 */
			}

		}

		return null;
	}

	public static List<GameObject> CheckVertical1(int row, int column, ShapesArray shapes)
	{
		if (row >= 2)
		{
			if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row - 1, column].GetComponent<Shape>()))
			{
				if (column >= 1)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row - 2, column - 1].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row - 1, column],
							shapes[row - 2, column - 1]
						};
						/* example
						 * * * *
						 * * 1 *
						 * * 2 *
						 * 3 * *
						 * * * *
						 */
					}
				}

				if (column <= Constants.Columns - 2)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row - 2, column + 1].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row - 1, column],
							shapes[row - 2, column + 1]
						};
						/* example
						* * * *
						* 1 * *
						* 2 * *
						* * 3 *
						* * * *
						*/
					}
				}
			}
		}

		return null;
	}

	public static List<GameObject> CheckVertical2(int row, int column, ShapesArray shapes)
	{
		if (row <= Constants.Rows - 3)
		{
			if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 1, column].GetComponent<Shape>()))
			{
				if (column >= 1)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 2, column - 1].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row + 1, column],
							shapes[row + 2, column - 1]
						};
						/* example
						 * * * *
						 * 3 * *
						 * * 2 *
						 * * 1 *
						 * * * *
						 */
					}
				}

				if (column <= Constants.Columns - 2)
				{
					if (shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 2, column + 1].GetComponent<Shape>()))
					{
						return new List<GameObject>()
						{
							shapes[row, column],
							shapes[row + 1, column],
							shapes[row + 2, column + 1]
						};
						/* example
						* * * *
						* * 3 *
						* 2 * *
						* 1 * *
						* * * *
						*/
					}
				}
			}
		}

		return null;
	}

	public static List<GameObject> CheckVertical3(int row, int column, ShapesArray shapes)
	{
		if (row <= Constants.Rows - 4)
		{
			if ((shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 1, column].GetComponent<Shape>())) &&
				(shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 3, column].GetComponent<Shape>())))
			{
				return new List<GameObject>()
				{
					shapes[row, column],
					shapes[row + 1, column],
					shapes[row + 3, column]
				};
				/* example
					* * *
					* 3 *
					* * *
					* 2 *
					* 1 *
					* * *
					*/
			}

			if ((shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 2, column].GetComponent<Shape>())) &&
				(shapes[row, column].GetComponent<Shape>().IsSameType(shapes[row + 3, column].GetComponent<Shape>())))
			{
				return new List<GameObject>()
				{
					shapes[row, column],
					shapes[row + 2, column],
					shapes[row + 3, column]
				};
				/* example
				 * * *
				 * 3 *
				 * 2 *
				 * * *
				 * 1 *
				 * * *
				 */
			}
		}

		return null;
	}
}
