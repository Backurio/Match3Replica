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
	public bool DebuggingMode = false;

	public ShapesArray shapes;
	public Transform shapesContainer;

	public Button RestartButton;
	public Button PremadeLevelButton;
	public Transform PlayArea;

	public Button ToggleGraphyButton;
	public GameObject Graphy;

	private int score;
	private int nextCandy;

	public readonly Vector2 CandySize = Vector2.one * Constants.CandySize;
	public readonly Vector2 BottomLeft = -Vector2.one * Constants.CandySize * 7.0f / 2.0f;
	public readonly Vector3 CandyScale = Vector3.one * Constants.CandySize / 0.7f;

	private GameState state = GameState.None;
	private GameObject hitGo = null;

	private Vector2[] spawnPositions;
	public GameObject[] CandyPrefabs;
	public GameObject[] ExplosionPrefabs;
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
		DebugText.enabled = DebuggingMode;
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
			item.GetComponent<Shape>().Type = item.name.Split('_')[1].Trim();
			item.GetComponent<Shape>().Bonus = BonusType.None;
			item.transform.localScale = CandyScale;
		}


		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in BombPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().GetComponent<Shape>().Type;
			item.GetComponent<Shape>().Bonus = BonusType.Bomb;
			item.transform.localScale = CandyScale;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in HorizontalPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().GetComponent<Shape>().Type;
			item.GetComponent<Shape>().Bonus = BonusType.Horizontal;
			item.transform.localScale = CandyScale;
		}

		// assign the name of the respective "normal" candy as the type of the bonus
		foreach (var item in VerticalPrefabs)
		{
			// todo: understand this line o.O
			item.GetComponent<Shape>().Type = CandyPrefabs.Where(x => x.GetComponent<Shape>().Type.Contains(item.name.Split('_')[1].Trim())).Single().GetComponent<Shape>().Type;
			item.GetComponent<Shape>().Bonus = BonusType.Vertical;
			item.transform.localScale = CandyScale;
		}

		// ultimate
		Ultimate.GetComponent<Shape>().Type = Ultimate.name;
		Ultimate.GetComponent<Shape>().Bonus = BonusType.Ultimate;
		Ultimate.transform.localScale = CandyScale;
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
				InstantiateAndPlaceNewCandy(row, column, newCandy, BonusType.None);
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

				InstantiateAndPlaceNewCandy(row, column, newCandy, BonusType.None);
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
					InstantiateAndPlaceNewCandy(row, column, newCandy, newCandy.GetComponent<Shape>().Bonus);
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

	private void InstantiateAndPlaceNewCandy(int row, int column, GameObject newCandy, BonusType bonusType = BonusType.None)
	{
		GameObject go = Instantiate(newCandy,
									BottomLeft + new Vector2(column * CandySize.x, row * CandySize.y),
									Quaternion.identity,
									shapesContainer) as GameObject;

		//go.GetComponent<Shape>().Bonus = bonusType;
		if (bonusType == BonusType.Ultimate)
		{
			go.GetComponent<Shape>().Type = "Ultimate";
		}
		// assign the specific properties
		go.GetComponent<Shape>().Assign(newCandy.GetComponent<Shape>().Type, row, column, bonusType);
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
		if (DebuggingMode)
		{
			//DebugText.text = DebugUtilities.GetArrayContents(shapes);
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
			// debugging only functionality
			if (DebuggingMode == true)
			{
				// user has clicked with the right mousebutton
				if (Input.GetMouseButtonDown(1) == true)
				{
					// stop check for potential matches coroutine to prevent an error when changing a potential match object
					StopCheckForPotentialMatches();

					// get the hit position
					var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
					// if hit occured
					if (hit.collider != null)
					{
						hitGo = hit.collider.gameObject;

						// find the next candy according to the candy prefabs
						for (int i = 0; i < CandyPrefabs.Length; i++)
						{
							if (CandyPrefabs[i].GetComponent<Shape>().Type == hitGo.GetComponent<Shape>().Type)
							{
								nextCandy = i + 1;
								if (nextCandy >= CandyPrefabs.Length)
								{
									nextCandy = 0;
								}
								break;
							}
						}
						// create a new candy according the nextCandy from the prefab candies at the same position
						GameObject newCandy = CandyPrefabs[nextCandy];

						ReplaceCandy(hit.collider.gameObject, newCandy);
					}

					// restart the check for potential matches coroutine
					StartCheckForPotentialMatches();
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

		BonusType hitGoBonus = hitGo.GetComponent<Shape>().Bonus;
		BonusType hitGo2Bonus = hitGo2.GetComponent<Shape>().Bonus;

		// check if one of the two objects was combined with an ultimate
		if ((hitGo2Bonus == BonusType.Ultimate) && (hitGoBonus != BonusType.Ultimate) && (hitGoBonus != BonusType.None))
		{
			DuplicateCandy(hitGo);
		}

		if ((hitGoBonus == BonusType.Ultimate) && (hitGo2Bonus != BonusType.Ultimate) && (hitGo2Bonus != BonusType.None))
		{
			DuplicateCandy(hitGo2);
		}

		List<MatchesInfo> matchesInfos = new List<MatchesInfo>();

		// get the matches via the helper methods
		matchesInfos.Add(shapes.GetMatches(hitGo, hitGo2));
		matchesInfos.Add(shapes.GetMatches(hitGo2, hitGo));

		var totalMatches = matchesInfos[0].MatchedCandy.Union(matchesInfos[1].MatchedCandy).Distinct();

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

		// decide if a bonus shall be added to one of the 2 user matches
		foreach (var item in matchesInfos)
		{
			if (item.Matches >= Constants.MinimumMatchesForBonus)
			{
				item.CreateBonus = true;
			}
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

			// check if there are any bonus objects to create
			foreach (var item in matchesInfos)
			{
				if (item.CreateBonus == true)
				{
					CreateBonus(item.OriginGameObject, item);
					item.CreateBonus = false;
				}
			}

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

			// search if there are matches with the collapsed objects and then new objects
			matchesInfos = new List<MatchesInfo>();
			matchesInfos.AddRange(shapes.GetMatchesInfos(collapsedCandyInfo.AlteredCandy));
			matchesInfos.AddRange(shapes.GetMatchesInfos(newCandyInfo.AlteredCandy));

			// boolean array to not create boni for the same match more than onces
			bool[,] bonusCreated = new bool[Constants.Rows, Constants.Columns];
			for (int i = 0; i < Constants.Rows; i++)
			{
				for (int j = 0; j < Constants.Columns; j++)
				{
					bonusCreated[i, j] = false;
				}
			}

			// loop throw all found matches
			List<GameObject> matches = new List<GameObject>();
			foreach (var item in matchesInfos)
			{
				// check if bonus should be created
				if (item.Matches >= Constants.MinimumMatchesForBonus)
				{
					bool bonus = true;
					foreach (var go in item.MatchedCandy)
					{
						if (bonusCreated[go.GetComponent<Shape>().Row, go.GetComponent<Shape>().Column] == true)
						{
							bonus = false;
						}
					}

					if (bonus == true)
					{
						item.CreateBonus = true;

						foreach (var go in item.MatchedCandy)
						{
							bonusCreated[go.GetComponent<Shape>().Row, go.GetComponent<Shape>().Column] = true;
						}
					}
				}
				// save each object to save it then to totalMatches
				matches.AddRange(item.MatchedCandy);
			}

			totalMatches = matches;

			timesRun++;
		}

		state = GameState.None;

		if (matchPerformed == true)
		{
			StartCheckForPotentialMatches();
		}
	}

	private void DuplicateCandy(GameObject go)
	{
		BonusType goBonus = go.GetComponent<Shape>().Bonus;
		string goType = go.GetComponent<Shape>().Type;
		List<GameObject> newCandy = new List<GameObject>();

		// if the candy is horizontal or vertical, add both to the list
		if ((goBonus == BonusType.Horizontal) || (goBonus == BonusType.Vertical))
		{
			newCandy.Add(GetPrefabFromTypeAndBonus(goType, BonusType.Horizontal));
			newCandy.Add(GetPrefabFromTypeAndBonus(goType, BonusType.Vertical));
		}
		else
		{
			newCandy.Add(GetPrefabFromTypeAndBonus(goType, goBonus));
		}

		foreach (var item in shapes.GetAllShapes().MatchedCandy)
		{
			if (item.GetComponent<Shape>().Type == goType)
			{
				// only replace item if the BonusType is None to not downgrade the object
				if (item.GetComponent<Shape>().Bonus == BonusType.None)
				{
					ReplaceCandy(item, newCandy[Random.Range(0, newCandy.Count)]);
				}
			}
		}
	}

	private void ReplaceCandy(GameObject oldCandy, GameObject newCandy)
	{
		int row = oldCandy.GetComponent<Shape>().Row;
		int column = oldCandy.GetComponent<Shape>().Column;
		Destroy(shapes[row, column]);
		InstantiateAndPlaceNewCandy(row, column, newCandy, newCandy.GetComponent<Shape>().Bonus);
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

		InstantiateAndPlaceNewCandy(hitGoCache.Row, hitGoCache.Column, GetPrefabFromTypeAndBonus(hitGoCache.Type, bonusType), bonusType);
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
		nextCandy = 0;
		ShowScore();
		state = GameState.None;
		Graphy.SetActive(false);

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

	private GameObject GetPrefabFromTypeAndBonus(string type, BonusType bonusType)
	{
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
		else if (bonusType == BonusType.None)
		{
			bonusPrefabs.AddRange(CandyPrefabs);
		}

		foreach (var item in bonusPrefabs)
		{
			if (item.GetComponent<Shape>().Type.Contains(type))
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

	public void ToggleGraphy()
	{
		Graphy.SetActive(!Graphy.activeSelf);
	}
}
