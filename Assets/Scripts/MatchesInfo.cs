using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class to handle all information about occuring matches
/// </summary>
public class MatchesInfo
{
	private List<GameObject> matchedCandies;

	/// <summary>
	/// Object which caused the match to happen (e.g. user move)
	/// </summary>
	public GameObject OriginGameObject { get; set; }

	/// <summary>
	/// Information if the match will create a new bonus object
	/// </summary>
	public bool CreateBonus { get; set; }

	/// <summary>
	/// Objects have been destroyed by a bonus mechanic
	/// </summary>
	public bool DestroyedByBonus { get; set; }

	/// <summary>
	/// Matches contain bonus objects
	/// </summary>
	public BonusType BonusesContained { get; set; }

	/// <summary>
	/// Horizontal matches
	/// </summary>
	public int HorizontalMatches { get; set; }

	/// <summary>
	/// Vertical matches
	/// </summary>
	public int VerticalMatches { get; set; }

	/// <summary>
	/// Horizontal and vertical matches
	/// </summary>
	public int Matches
	{
		get
		{
			if (HorizontalMatches == 0)
			{
				return VerticalMatches;
			}
			else if (VerticalMatches == 0)
			{
				return HorizontalMatches;
			}
			else
			{
				return HorizontalMatches + VerticalMatches - 1;
			}
		}
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public MatchesInfo()
	{
		matchedCandies = new List<GameObject>();
		BonusesContained = BonusType.None;
		CreateBonus = false;
		DestroyedByBonus = false;
		OriginGameObject = null;
	}

	/// <summary>
	/// Get all matched objects
	/// </summary>
	public IEnumerable<GameObject> MatchedCandy
	{
		get
		{
			return matchedCandies.Distinct();
		}
	}

	/// <summary>
	/// Adds object to the matchedCandies list
	/// </summary>
	/// <param name="go">Object to be added to the matchedCandies list</param>
	public void AddObject(GameObject go)
	{
		if (!matchedCandies.Contains(go))
		{
			matchedCandies.Add(go);
		}
	}

	/// <summary>
	/// Adds a list of objects to the matchedCandies list
	/// </summary>
	/// <param name="gos">List of objects to be added to the matchedCandies list </param>
	public void AddObjectRange(IEnumerable<GameObject> gos)
	{
		foreach (var item in gos)
		{
			AddObject(item);
		}
	}

}
