using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchesInfo
{
	private List<GameObject> matchedCandies;

	public BonusType BonusesContained { get; set; }
	public int HorizontalMatches { get; set; }
	public int VerticalMatches { get; set; }
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

	public MatchesInfo()
	{
		matchedCandies = new List<GameObject>();
		BonusesContained = BonusType.None;
	}

	public IEnumerable<GameObject> MatchedCandy
	{
		get
		{
			return matchedCandies.Distinct();
		}
	}

	public void AddObject(GameObject go)
	{
		if (!matchedCandies.Contains(go))
		{
			matchedCandies.Add(go);
		}
	}

	public void AddObjectRange(IEnumerable<GameObject> gos)
	{
		foreach (var item in gos)
		{
			AddObject(item);
		}
	}

}
