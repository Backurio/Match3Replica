using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class ShapesManager : MonoBehaviour
{
	public Text DebugText, ScoreText;
	public bool ShowDebugInfo = false;

	public ShapesArray shapes;

	private int score;

	public readonly Vector2 BottomRight = new Vector2(-2.73f, -4.27f);
	public readonly Vector2 CandySize = new Vector2(0.7f, 0.7f);

	private GameState state = GameState.None;
	private GameObject hitGo = null;
	private Vector2[] spawnPositions;
	public GameObject[] CandyPrefabs;
	public GameObject[] ExplosionPrefabs;
	public GameObject[] BonusPrefabs;

	private IEnumerator CheckPotentialMatchesCoroutine;
	private IEnumerator AnimatePotentialMatchesCoroutine;

	IEnumerable<GameObject> potentialMatches;

	public SoundManager soundManager;

	private void Awake()
	{
		DebugText.enabled = ShowDebugInfo;
	}

	private void Start()
	{

	}

	private void InitializeTypesOnPrefabShapesAndBonuses()
	{
		// just assign the name of the prefab
		foreach (var item in CandyPrefabs)
		{
			item.GetComponent<Shape>().Type = item.name;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in BonusPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
		}
	}

	public void InitializeCandyAndSpawnPositionFromPremadeLevel()
	{
		InitializeVariables();

		var premadeLevel = DebugUtilities.FillShapesArrayFromResourceData();

		if (shapes != null)
		{
			DestroyAllCandy();
		}

		shapes = new ShapesArray();
		spawnPositions = new Vector2[Constants.Columns];

		for (int row = 0; row < Constants.Rows; row++)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				GameObject newCandy = null;
				newCandy = GetSpecificCandyOrBonusForPremadeLevel(premadeLevel[row, column]);
				InstantiateAndPlaceNewCandy(row, column, newCandy);
			}
		}

		SetupSpawnPosition();
	}

	private void InitializeCandyAndSpanwPositions()
	{
		InitializeVariables();

		if (shapes != null)
		{
			DestroyAllCandy();
		}

		shapes = new ShapesArray();
		spawnPositions = new Vector2[Constants.Columns];

		for (int row = 0; row < Constants.Rows; row++)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				GameObject newCandy = GetRandomCandy();

				// check if two previous horizontal candies are of the same type
				while ((column >= 2) &&
					   (shapes[row, column - 1].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())) &&
					   (shapes[row, column - 1].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())))
				{
					newCandy = GetRandomCandy();
				}

				// check if two previous vertical candies are of the same type
				while ((row >= 2) &&
					   (shapes[row - 1, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())) &&
					   (shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())))
				{
					newCandy = GetRandomCandy();
				}

				InstantiateAndPlaceNewCandy(row, column, newCandy);
			}
		}
	}

	private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
	{
		GameObject go = Instantiate(newCandy,
									BottomRight + new Vector2(column * CandySize.x, row * CandySize.y),
									Quaternion.identity) as GameObject;

		// assign the specific properties
		go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
		shapes[row, column] = go;
	}

	private void SetupSpawnPosition()
	{
		// create the spawn position for the new shapes (will pop from the ceiling)
		for (int column = 0; column < Constants.Columns; column++)
		{
			spawnPositions[column] = BottomRight + new Vector2(column * CandySize.x, Constants.Rows * CandySize.y);
		}
	}

	private void Update()
	{
		if (ShowDebugInfo)
		{
			DebugText.text = DebugUtilities.GetArrayContents(shapes);
		}

		if (state == GameState.None)
		{
			// user has clicked or touched
			if (Input.GetMouseButtonDown(0) == true)
			{
				// get the hit position
				var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
				// if hit occured
				if (hit.collider != null)
				{
					hitGo = hit.collider.gameObject;
					state = GameState.SelectionStarted;
				}
			}
		}
		else if (state == GameState.SelectionStarted)
		{
			// user dragged
			if (Input.GetMouseButton(0) == true)
			{
				var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
				// if hit occured
				if ((hit.collider != null) && (hitGo != hit.collider.gameObject))
				{
					// user did a hit, no need to show the hints
					StopCheckForPotentialMatches();

					// if the two shapes are diagonally aligned (different row and column), just return
					if (!Utilities.AreVerticalOrHorizontalNeighbors(hitGo.GetComponent<Shape>(), hit.collider.gameObject.GetComponent<Shape>()))
					{
						state = GameState.None;
					}
					else
					{
						state = GameState.Animating;
						FixSortingLayer(hitGo, hit.collider.gameObject);
						// todo: code here, use hit.collider.gameObject
					}

				}
			}
		}
	}

	private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
	{
		SpriteRenderer sp1 = hitGo.GetComponent<SpriteRenderer>();
		SpriteRenderer sp2 = hitGo2.GetComponent<SpriteRenderer>();

		if (sp1.sortingOrder >= sp2.sortingOrder)
		{
			sp1.sortingOrder = 1;
			sp2.sortingOrder = 0;
		}
	}

	private IEnumerator FindMatchesAndCollapse(GameObject hitGo2)
	{
		shapes.Swap(hitGo, hitGo2);

		// move the swapped ones
		hitGo.transform.DOMove(hitGo2.transform.position, Constants.AnimationDuration);
		hitGo2.transform.DOMove(hitGo.transform.position, Constants.AnimationDuration);
		yield return new WaitForSeconds(Constants.AnimationDuration);

		// get the matches via the helper methods
		var hitGoMatchesInfo = shapes.GetMatches(hitGo);
		var hitGo2MatchesInfo = shapes.GetMatches(hitGo2);

		var totalMatches = hitGoMatchesInfo.MatchedCandy.Union(hitGo2MatchesInfo.MatchedCandy).Distinct();

		// if user's swap didn't create at least a 3-match, undo their swap
		if (totalMatches.Count() < Constants.MinimumMatches)
		{
			hitGo.transform.DOMove(hitGo2.transform.position, Constants.AnimationDuration);
			hitGo2.transform.DOMove(hitGo.transform.position, Constants.AnimationDuration);
			yield return new WaitForSeconds(Constants.AnimationDuration);

			shapes.UndoSwap();
		}

		// if more than 3 matches and no bonus is contained in the line, we will award a new bonus
		bool addBonus = ((totalMatches.Count() >= Constants.MinimumMatchesForBonus) &&
						 (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGoMatchesInfo.BonusesContained)) &&
						 (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2MatchesInfo.BonusesContained)));

		Shape hitGoCache = null;
		if (addBonus == true)
		{
			// get the game object that was of the same type
			var sameTypeGo = hitGoMatchesInfo.MatchedCandy.Count() > 0 ? hitGo : hitGo2;
			hitGoCache = sameTypeGo.GetComponent<Shape>();
		}

		int timesRun = 1;
		while (totalMatches.Count() >= Constants.MinimumMatches)
		{
			// increase score
			IncreaseScore((totalMatches.Count() - 2) * Constants.Match3Score);

			if (timesRun >= 2)
			{
				IncreaseScore(Constants.SubsequentMatchScore);
			}

			soundManager.PlayCrinkle();

			foreach (var item in totalMatches)
			{
				shapes.Remove(item);
				RemoveFromScene(item);
			}

			// check and instantiate bonus if needed
			if (addBonus == true)
			{
				CreateBonus(hitGoCache);
			}

			addBonus = false;

			// get the columns that we had a collapse
			var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

			// the order the 2 methods below get called is important!
			// collapse the ones gone
			var collapsedCandyInfo = shapes.Collapse(columns);
			// create new ones
			var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

			int maxDistance = Mathf.Max(collapsedCandyInfo.MaxDistance, newCandyInfo.MaxDistance);

			MoveAndAnimate(newCandyInfo.AlteredCandy, maxDistance);
			MoveAndAnimate(collapsedCandyInfo.AlteredCandy, maxDistance);

			// will wait for both of the above animations
			yield return new WaitForSeconds(Constants.MoveAnimationMinDuration * maxDistance);

			// search if there are matches with the new/collapsed items
			totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();

			timesRun++;
		}

		state = GameState.None;
		StartCheckForPotentialMatches();
	}

	private void CreateBonus(Shape hitGoCache)
	{
		GameObject Bonus = Instantiate(GetBonusFromType(hitGoCache.Type),
									   BottomRight + new Vector2(hitGoCache.Column * CandySize.x, hitGoCache.Row * CandySize.y),
									   Quaternion.identity) as GameObject;
		shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
		var BonusShape = Bonus.GetComponent<Shape>();
		// will have the same type as the normal candy
		BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
		// add the proper bonus type
		BonusShape.Bonus |= BonusType.DestroyWholeRowColumn;
	}

	private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
	{
		AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

		// find how many null values the column has
		foreach (int column in columnsWithMissingCandy)
		{
			var emptyItems = shapes.GetEmptyItemsOnColumn(column);
			foreach (var item in emptyItems)
			{
				var go = GetRandomCandy();
				GameObject newCandy = Instantiate(go, spawnPositions[column], Quaternion.identity) as GameObject;

				newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

				if (Constants.Rows - item.Row > newCandyInfo.MaxDistance)
				{
					newCandyInfo.MaxDistance = Constants.Rows - item.Row;
				}

				shapes[item.Row, item.Column] = newCandy;
				newCandyInfo.AddCandy(newCandy);
			}
		}

		return newCandyInfo;
	}

	private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects, int distance)
	{
		foreach (var item in movedGameObjects)
		{
			item.transform.DOMove(BottomRight + new Vector2(item.GetComponent<Shape>().Column * CandySize.x, item.GetComponent<Shape>().Row * CandySize.y),
								  Constants.MoveAnimationMinDuration + distance);
		}
	}

	private void RemoveFromScene(GameObject item)
	{
		GameObject explosion = GetRandomExplosion();
		var newExplosion = Instantiate(explosion, item.transform.position, Quaternion.identity) as GameObject;
		Destroy(newExplosion, Constants.ExplosionDuration);
		Destroy(item);
	}

	private GameObject GetSpecificCandyOrBonusForPremadeLevel(string info)
	{
		var tokens = info.Split('_');

		if (tokens.Count() == 1)
		{
			foreach (var item in CandyPrefabs)
			{
				if (item.GetComponent<Shape>().Type.Contains(tokens[0].Trim()))
				{
					return item;
				}
			}
		}
		else if ((tokens.Count() == 2) && (tokens[1].Trim() == "B"))
		{
			foreach (var item in BonusPrefabs)
			{
				if (item.name.Contains(tokens[0].Trim()))
				{
					return item;
				}
			}
		}

		throw new System.Exception("Wrong type, check your premade level");
	}

	private GameObject GetRandomCandy()
	{
		return CandyPrefabs[Random.Range(0, CandyPrefabs.Length)];
	}

	private void InitializeVariables()
	{
		score = 0;
		ShowScore();
	}

	private void IncreaseScore(int amount)
	{
		score += amount;
		ShowScore();
	}

	private void ShowScore()
	{
		ScoreText.text = "Score: " + score.ToString();
	}

	private void DestroyAllCandy()
	{
		for (int row = 0; row < Constants.Rows; row++)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				Destroy(shapes[row, column]);
			}
		}
	}

	private GameObject GetRandomExplosion()
	{
		return ExplosionPrefabs[Random.Range(0, ExplosionPrefabs.Length)];
	}

	private GameObject GetBonusFromType(string type)
	{
		string color = type.Split('_')[1].Trim();
		foreach (var item in BonusPrefabs)
		{
			if (item.GetComponent<Shape>().Type.Contains(color))
			{
				return item;
			}
		}
		throw new System.Exception("wrong type");
	}

	private void StartCheckForPotentialMatches()
	{
		StopCheckForPotentialMatches();

		// get a reference to stop it later
		CheckPotentialMatchesCoroutine = CheckpotentialMatches();
		StartCoroutine(CheckPotentialMatchesCoroutine);
	}

	private void StopCheckForPotentialMatches()
	{
		if (AnimatePotentialMatchesCoroutine != null)
		{
			StopCoroutine(AnimatePotentialMatchesCoroutine);
		}

		if (CheckPotentialMatchesCoroutine != null)
		{
			StopCoroutine(CheckPotentialMatchesCoroutine);
		}

		ResetOpacityOnPotentialMatches();
	}

	private void ResetOpacityOnPotentialMatches()
	{
		if (potentialMatches != null)
		{
			foreach (var item in potentialMatches)
			{
				if (item == null)
				{
					break;
				}

				Color c = item.GetComponent<SpriteRenderer>().color;
				c.a = 1.0f;
				item.GetComponent<SpriteRenderer>().color = c;
			}
		}
	}

	private IEnumerator CheckpotentialMatches()
	{
		yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
		potentialMatches = Utilities.GetPotentialMatches(shapes);
		if (potentialMatches != null)
		{
			while (true)
			{
				AnimatePotentialMatchesCoroutine = Utilities.AnimatePotentialMatches(potentialMatches);
				StartCoroutine(AnimatePotentialMatchesCoroutine);
				yield return new WaitForSeconds(Constants.WaitBeforePotentialMatchesCheck);
			}
		}
	}
}
