using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class to handle the scaling of the canvas based on the screen resolution
/// - for screens with a bigger width than the normal 16:9 the scaling is done for height
/// - for screens with a bigger height than the normal 16:9 the scaling is done for the width
/// </summary>
public class CanvasManager : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		float matchWidthOrHeight;

		// check the screen ratio
		if ((Screen.height / Screen.width) <= 1.78) // screen ratio <= 16:9 -> scale with screen height
		{
			matchWidthOrHeight = 1.0f;
		}
		else // screen ratio > 16:9 -> scale with screen width
		{
			matchWidthOrHeight = 0.0f;
		}

		// set canvas scale type according to settings (depends on screen ratio - smartphone vs tablet)
		gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = matchWidthOrHeight;
	}
}
