using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchesInfo
{
	private List<GameObject> matchedCandies;

	public BonusType BonusesContained { get; set; }

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
