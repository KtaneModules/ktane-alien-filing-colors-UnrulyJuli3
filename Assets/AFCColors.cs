using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFCColors
{
	public enum Color
	{
		White,
		Red,
		Orange,
		Yellow,
		Green,
		Blue,
		Violet,
		Magenta
	}

	public static readonly Dictionary<Color, UnityEngine.Color> ColorMap = new Dictionary<Color, UnityEngine.Color>
	{
		{ Color.White, UnityEngine.Color.white },
		{ Color.Red, UnityEngine.Color.red },
		{ Color.Orange, new UnityEngine.Color(1f, 0.647f, 0f) },
		{ Color.Yellow, UnityEngine.Color.yellow },
		{ Color.Green, UnityEngine.Color.green },
		{ Color.Blue, UnityEngine.Color.blue },
		{ Color.Violet, new UnityEngine.Color(0.576470588f, 0.105882353f, 0.894117647f) },
		{ Color.Magenta, UnityEngine.Color.magenta }
	};
}