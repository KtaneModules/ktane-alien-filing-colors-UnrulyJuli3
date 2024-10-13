using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AFCButton : MonoBehaviour
{
	public KMSelectable Selectable;
	public Renderer Renderer;

	public TextMesh[] SplitColorblindTexts;
	public TextMesh CenterColorblindText;

	private static readonly Color unselectedColor = Color.white;
	private static readonly Color selectedColor = new Color(0.196078431f, 1f, 1f / 3f);

	internal bool selected;

	private void Start()
	{
		SetSelected(false);
	}

	public void SetSelected(bool isSelected)
	{
		selected = isSelected;
		Renderer.materials[1].color = selected ? selectedColor : unselectedColor;
	}

	private string GetColorLetter(AFCColors.Color color)
	{
		return color.ToString().First().ToString();
	}

	public List<AFCColors.Color> CurrentColors;

	public void SetColors(List<AFCColors.Color> colors, bool colorblindMode)
	{
		foreach (TextMesh text in SplitColorblindTexts) text.text = "";
		CenterColorblindText.text = "";
		switch (colors.Count)
		{
			case 1:
				if (colorblindMode) CenterColorblindText.text = GetColorLetter(colors[0]);
				SetMaterialColor(0, colors[0]);
				SetMaterialColor(2, colors[0]);
				break;
			case 2:
				if (colorblindMode)
				{
					SplitColorblindTexts[0].text = GetColorLetter(colors[0]);
					SplitColorblindTexts[1].text = GetColorLetter(colors[1]);
				}
				SetMaterialColor(0, colors[0]);
				SetMaterialColor(2, colors[1]);
				break;
		}
		CurrentColors = colors;
	}

	private void SetMaterialColor(int materialIndex, AFCColors.Color color)
	{
		Renderer.materials[materialIndex].color = AFCColors.ColorMap[color];
	}
}
