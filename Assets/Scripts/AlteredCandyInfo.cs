using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class to save the information about objects which were altered (=collapse) due to matches
/// </summary>
public class AlteredCandyInfo
{
	private List<GameObject> NewCandy { get; set; }

	public AlteredCandyInfo()
	{
		NewCandy = new List<GameObject>();
	}

	public IEnumerable<GameObject> AlteredCandy
	{
		get
		{
			return NewCandy.Distinct();
		}
	}

	/// <summary>
	/// Add Object to the NewCandy list if it is not already added
	/// </summary>
	/// <param name="go">Object to be added to the NewCandy list</param>
	public void AddCandy(GameObject go)
	{
		if (!NewCandy.Contains(go))
		{
			NewCandy.Add(go);
		}
	}
}
