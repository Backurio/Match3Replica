using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class ShapesManager : MonoBehaviour
{
	public Text DebugText, ScoreText;
	public GameObject ShuffleText;
	public bool ShowDebugInfo = false;

	public ShapesArray shapes;
	public Transform shapesContainer;

	public Button RestartButton;
	public Button PremadeLevelButton;
	public Transform PlayArea;

	private int score;

	public readonly Vector2 CandySize = Vector2.one * Constants.CandySize;
	public readonly Vector2 BottomLeft = -Vector2.one * Constants.CandySize * 7.0f / 2.0f;
	public readonly Vector3 CandyScale = Vector3.one * Constants.CandySize / 0.7f;

	private GameState state = GameState.None;
	private GameObject hitGo = null;

	private Vector2[] spawnPositions;
	public GameObject[] CandyPrefabs;
	public GameObject[] ExplosionPrefabs;
	public GameObject[] BonusPrefabs;
	public GameObject[] HorizontalPrefabs;
	public GameObject[] VerticalPrefabs;
	public GameObject[] BombPrefabs;
	public GameObject Ultimate;

	private IEnumerator CheckPotentialMatchesCoroutine;
	private IEnumerator AnimatePotentialMatchesCoroutine;
	private IEnumerator FindMatchesAndCollapseCoroutine;

	IEnumerable<GameObject> potentialMatches;

	public SoundManager soundManager;

	private void Awake()
	{
		DebugText.enabled = ShowDebugInfo;
		ShuffleText.SetActive(false);
	}

	private void Start()
	{
		InitializePlayArea();
		InitializePrefabs();

		InitializeCandyAndSpawnPositions();
	}

	private void InitializePlayArea()
	{
		Vector3 newScale = PlayArea.localScale;
		newScale.x = Constants.Columns * Constants.CandySize + Constants.CandySize * 0.1f;
		newScale.y = Constants.Rows * Constants.CandySize + Constants.CandySize * 0.1f;
		PlayArea.localScale = newScale;

		Vector3 newPosition = PlayArea.position;
		newPosition.y = BottomLeft.y - BottomLeft.x;
		PlayArea.position = newPosition;

	}

	private void InitializePrefabs()
	{
		// just assign the name of the prefab
		foreach (var item in CandyPrefabs)
		{
			item.GetComponent<Shape>().Type = item.name;
			item.transform.localScale = CandyScale;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in BonusPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
			item.transform.localScale = CandyScale;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in BombPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
			item.transform.localScale = CandyScale;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in HorizontalPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
			item.transform.localScale = CandyScale;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in VerticalPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().name;
			item.transform.localScale = CandyScale;
		}
	}

	public void InitializeCandyAndSpawnPositionFromPremadeLevel()
	{
		InitializeVariables();

		var premadeLevel = DebugUtilities.FillShapesArrayFromResourceData();

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

	public void InitializeCandyAndSpawnPositions()
	{
		InitializeVariables();

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

		SetupSpawnPosition();
	}

	public void ShuffleCandies()
	{
		if (shapes == null)
		{
			return;
		}

		StopAllCoroutines();

		List<GameObject> fixedCandyPool = new List<GameObject>();

		// copy shapesarray to a list of GameObjects
		for (int row = 0; row < Constants.Rows; row++)
		{
			for (int column = 0; column < Constants.Columns; column++)
			{
				fixedCandyPool.Add(shapes[row, column]);
			}
		}

		bool noArrangementFound = true;

		// run this loop as long as no valid arrangement is found
		while (noArrangementFound == true)
		{
			// copy list of GameObjects to a new list to preserve the original one
			List<GameObject> variableCandyPool = new List<GameObject>(fixedCandyPool);
			// destroy all shown candies
			DestroyAllCandy();
			shapes = new ShapesArray();
			GameObject newCandy;

			noArrangementFound = false;
			// loop through all positions and set one candy from the list
			// if no valid candy is found for a position within 10 tries, both loops are stopped and restarted
			for (int row = 0; row < Constants.Rows; row++)
			{
				for (int column = 0; column < Constants.Columns; column++)
				{
					newCandy = variableCandyPool[Random.Range(0, variableCandyPool.Count)];

					int tryCounter = 0;
					// check if two previous horizontal candies are of the same type. if true -> take a new candy
					while ((column >= 2) &&
						   (shapes[row, column - 1].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())) &&
						   (shapes[row, column - 1].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())))
					{
						newCandy = variableCandyPool[Random.Range(0, variableCandyPool.Count)];
						tryCounter++;
						if (tryCounter >= 10)
						{
							noArrangementFound = true;
							break;
						}
					}

					tryCounter = 0;
					// check if two previous vertical candies are of the same type. if true -> take a new candy
					while ((row >= 2) &&
						   (shapes[row - 1, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())) &&
						   (shapes[row - 2, column].GetComponent<Shape>().IsSameType(newCandy.GetComponent<Shape>())))
					{
						newCandy = variableCandyPool[Random.Range(0, variableCandyPool.Count)];
						tryCounter++;
						if (tryCounter >= 10)
						{
							noArrangementFound = true;
							break;
						}
					}

					if (noArrangementFound == true)
					{
						break;
					}

					// place the new candy
					InstantiateAndPlaceNewCandy(row, column, newCandy);
					// remove the new candy from the list
					variableCandyPool.Remove(newCandy);
				}

				if (noArrangementFound == true)
				{
					break;
				}
			}
		}

		state = GameState.None;

		SetupSpawnPosition();
	}

	private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy)
	{
		GameObject go = Instantiate(newCandy,
									BottomLeft + new Vector2(column * CandySize.x, row * CandySize.y),
									Quaternion.identity,
									shapesContainer) as GameObject;

		// assign the specific properties
		go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column);
		go.GetComponent<BoxCollider2D>().enabled = true;
		shapes[row, column] = go;
	}

	private void SetupSpawnPosition()
	{
		// create the spawn position for the new shapes (will pop from the ceiling)
		for (int column = 0; column < Constants.Columns; column++)
		{
			spawnPositions[column] = BottomLeft + new Vector2(column * CandySize.x, Constants.Rows * CandySize.y);
		}

		StartCheckForPotentialMatches();
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
					// if the two shapes are diagonally aligned (different row and column), just return
					if (!Utilities.AreVerticalOrHorizontalNeighbors(hitGo.GetComponent<Shape>(), hit.collider.gameObject.GetComponent<Shape>()))
					{
						state = GameState.None;
					}
					else
					{
						state = GameState.Animating;
						FixSortingLayer(hitGo, hit.collider.gameObject);
						FindMatchesAndCollapseCoroutine = FindMatchesAndCollapse(hit.collider.gameObject);
						StartCoroutine(FindMatchesAndCollapseCoroutine);
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
			sp1.sortingOrder = 11;
			sp2.sortingOrder = 10;
		}
	}

	private IEnumerator FindMatchesAndCollapse(GameObject hitGo2)
	{
		bool matchPerformed = false;

		shapes.Swap(hitGo, hitGo2);

		// move the swapped ones
		hitGo.transform.DOMove(hitGo2.transform.position, Constants.SwapAnimationDuration);
		hitGo2.transform.DOMove(hitGo.transform.position, Constants.SwapAnimationDuration);
		yield return new WaitForSeconds(Constants.SwapAnimationDuration);

		// get the matches via the helper methods
		var hitGoMatchesInfo = shapes.GetMatches(hitGo);
		var hitGo2MatchesInfo = shapes.GetMatches(hitGo2);

		var totalMatches = hitGoMatchesInfo.MatchedCandy.Union(hitGo2MatchesInfo.MatchedCandy).Distinct();

		// if user's swap didn't create at least a 3-match, undo their swap
		if (totalMatches.Count() < Constants.MinimumMatches)
		{
			hitGo.transform.DOMove(hitGo2.transform.position, Constants.SwapAnimationDuration);
			hitGo2.transform.DOMove(hitGo.transform.position, Constants.SwapAnimationDuration);
			yield return new WaitForSeconds(Constants.SwapAnimationDuration);

			shapes.UndoSwap();
		}
		else
		{
			// user performed a match, no need to show the hints anymore
			matchPerformed = true;
			StopCheckForPotentialMatches();
		}

		bool hitGoBonus = (hitGoMatchesInfo.Matches >= Constants.MinimumMatchesForBonus);
		bool hitGo2Bonus = (hitGo2MatchesInfo.Matches >= Constants.MinimumMatchesForBonus);

		// todo: add create bonus for hitGo2

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

			if (hitGoBonus == true)
			{
				CreateBonus(hitGo, hitGoMatchesInfo);
			}
			hitGoBonus = false;

			if (hitGo2Bonus == true)
			{
				CreateBonus(hitGo2, hitGo2MatchesInfo);
			}
			hitGo2Bonus = false;

			// get the columns that we had a collapse
			var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

			// the order the 2 methods below get called is important!
			// collapse the ones gone
			var collapsedCandyInfo = shapes.Collapse(columns);
			// create new ones
			var newCandyInfo = CreateNewCandyInSpecificColumns(columns);

			// wait until explosion animation has finished
			yield return new WaitForSeconds(Constants.ExplosionDuration);

			MoveAndAnimate(newCandyInfo.AlteredCandy);
			MoveAndAnimate(collapsedCandyInfo.AlteredCandy);

			// will wait for both of the above animations
			yield return new WaitForSeconds(Constants.MoveAnimationMinDuration);

			// search if there are matches with the new/collapsed items
			totalMatches = shapes.GetMatches(collapsedCandyInfo.AlteredCandy).Union(shapes.GetMatches(newCandyInfo.AlteredCandy)).Distinct();

			timesRun++;
		}

		state = GameState.None;

		if (matchPerformed == true)
		{
			StartCheckForPotentialMatches();
		}
	}

	private void CreateBonus(GameObject go, MatchesInfo matchesInfo)
	{
		Shape hitGoCache = go.GetComponent<Shape>();
		BonusType bonusType = BonusType.None;

		if ((matchesInfo.HorizontalMatches >= Constants.MinimumMatchesForUltimate) ||
			(matchesInfo.VerticalMatches >= Constants.MinimumMatchesForUltimate))
		{
			bonusType = BonusType.Ultimate;
		}
		else if ((matchesInfo.HorizontalMatches == Constants.MinimumMatchesForBonus) &&
				 (matchesInfo.HorizontalMatches > matchesInfo.VerticalMatches))
		{
			bonusType = BonusType.Vertical;
		}
		else if ((matchesInfo.VerticalMatches == Constants.MinimumMatchesForBonus) &&
			     (matchesInfo.VerticalMatches > matchesInfo.HorizontalMatches))
		{
			bonusType = BonusType.Horizontal;
		}
		else
		{
			bonusType = BonusType.Bomb;
		}

		GameObject Bonus = Instantiate(GetBonusFromType(hitGoCache.Type, bonusType),
									   BottomLeft + new Vector2(hitGoCache.Column * CandySize.x, hitGoCache.Row * CandySize.y),
									   Quaternion.identity,
									   shapesContainer) as GameObject;
		shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
		var BonusShape = Bonus.GetComponent<Shape>();
		// will have the same type as the normal candy
		BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);
		// add the proper bonus type
		BonusShape.Bonus = bonusType;
	}

	private AlteredCandyInfo CreateNewCandyInSpecificColumns(IEnumerable<int> columnsWithMissingCandy)
	{
		AlteredCandyInfo newCandyInfo = new AlteredCandyInfo();

		// find how many null values the column has
		foreach (int column in columnsWithMissingCandy)
		{
			var emptyItems = shapes.GetEmptyItemsOnColumn(column);
			int offset = 0;
			foreach (var item in emptyItems)
			{
				var go = GetRandomCandy();
				GameObject newCandy = Instantiate(go,
												  spawnPositions[column] + new Vector2(0.0f, offset * CandySize.y),
												  Quaternion.identity,
												  shapesContainer) as GameObject;

				newCandy.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);

				shapes[item.Row, item.Column] = newCandy;
				newCandyInfo.AddCandy(newCandy);

				offset++;
			}
		}

		return newCandyInfo;
	}

	private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects)
	{
		foreach (var item in movedGameObjects)
		{
			Vector2 newPosition = new Vector2(item.GetComponent<Shape>().Column * CandySize.x, item.GetComponent<Shape>().Row * CandySize.y);
			item.transform.DOMove(BottomLeft + newPosition, Constants.MoveAnimationMinDuration);
		}
	}

	private void RemoveFromScene(GameObject item)
	{
		// todo: add explosion effects
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
		// stop all coroutines. needed for Restart and Premade Level button
		StopAllCoroutines();

		score = 0;
		ShowScore();
		state = GameState.None;

		if (shapes != null)
		{
			DestroyAllCandy();
		}

		shapes = new ShapesArray();
		spawnPositions = new Vector2[Constants.Columns];
	}

	private void IncreaseScore(int amount)
	{
		score += amount;
		ShowScore();
	}

	private void ShowScore()
	{
		ScoreText.text = score.ToString();
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

	private GameObject GetBonusFromType(string type, BonusType bonusType)
	{
		string color = type.Split('_')[1].Trim();

		List<GameObject> bonusPrefabs = new List<GameObject>();

		if (bonusType == BonusType.Ultimate)
		{
			return Ultimate;
		}
		else if (bonusType == BonusType.Bomb)
		{
			bonusPrefabs.AddRange(BombPrefabs);
		}
		else if (bonusType == BonusType.Horizontal)
		{
			bonusPrefabs.AddRange(HorizontalPrefabs);
		}
		else if (bonusType == BonusType.Vertical)
		{
			bonusPrefabs.AddRange(VerticalPrefabs);
		}

		foreach (var item in bonusPrefabs)
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

				item.transform.localScale = CandyScale;
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
				for (int i = 0; i < 2; i++)
				{
					AnimatePotentialMatchesCoroutine = Utilities.AnimatePotentialMatches(potentialMatches);
					StartCoroutine(AnimatePotentialMatchesCoroutine);
					yield return new WaitForSeconds(2 * Constants.ScaleAnimationDuration);
				}
				yield return new WaitForSeconds(Constants.WaitBetweenPotentialMatchesAnimation);
			}
		}
		else
		{
			// no potential match found, shuffle the objects
			StartCoroutine("Shuffle");
		}
	}

	private IEnumerator Shuffle()
	{
		ShuffleText.SetActive(true);
		yield return new WaitForSeconds(Constants.WaitBeforeShuffling);
		ShuffleCandies();
		ShuffleText.SetActive(false);
	}
}
