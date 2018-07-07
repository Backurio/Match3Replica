using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlteredCandyInfo
{
	private List<GameObject> NewCandy { get; set; }
	public int MaxDistance { get; set; }

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

	public void AddCandy(GameObject go)
	{
		if (!NewCandy.Contains(go))
		{
			NewCandy.Add(go);
		}
	}
}
