using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AFCColor = AFCColors.Color;
using KModkit;

/*
 * This is a complete re-code of Alien Filing Colors.
 */

public class AFCScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo BombInfo;
	public KMBombModule Module;
	public KMColorblindMode ColorblindMode;
	public AFCButton[] Buttons;

	private int moduleId;
	private static int moduleIdCounter;

	private bool isSolved;

	private class AFCName
	{
		public string Name;
		public List<AFCColor> Colors;
		public AFCName(string name, params AFCColor[] colors)
		{
			Name = name;
			Colors = colors.ToList();
		}
	}

	private static readonly AFCName[] nameData = new[]
	{
		new AFCName("Xilly", AFCColor.White),
		new AFCName("Xyxxy", AFCColor.Red),
		new AFCName("Xyzz", AFCColor.Orange),
		new AFCName("Xyzzy", AFCColor.Yellow),
		new AFCName("Xzzyxyz", AFCColor.Green),
		new AFCName("Zazzblat", AFCColor.Blue),
		new AFCName("Zilla", AFCColor.Magenta),
		new AFCName("Zizz", AFCColor.White, AFCColor.Red),
		new AFCName("Zizza", AFCColor.White, AFCColor.Orange),
		new AFCName("Zizzy", AFCColor.White, AFCColor.Yellow),
		new AFCName("Zxylly", AFCColor.White, AFCColor.Green),
		new AFCName("Zy", AFCColor.White, AFCColor.Blue),
		new AFCName("Zyll", AFCColor.White, AFCColor.Violet),
		new AFCName("Zylly", AFCColor.White, AFCColor.Magenta),
		new AFCName("Zyz", AFCColor.Red, AFCColor.Orange),
		new AFCName("Zyzz", AFCColor.Red, AFCColor.Yellow),
		new AFCName("Zyzza", AFCColor.Red, AFCColor.Green),
		new AFCName("Zyzzblat", AFCColor.Red, AFCColor.Blue),
		new AFCName("Zyzzblit", AFCColor.Red, AFCColor.Violet),
		new AFCName("Zyzzy", AFCColor.Red, AFCColor.Magenta),
		new AFCName("Zyzzzz", AFCColor.Orange, AFCColor.Yellow),
		new AFCName("Zyzzzzzz", AFCColor.Orange, AFCColor.Green),
		new AFCName("Zz", AFCColor.Orange, AFCColor.Blue),
		new AFCName("Zzz", AFCColor.Orange, AFCColor.Violet),
		new AFCName("Zzzz", AFCColor.Orange, AFCColor.Magenta),
		new AFCName("Zzzzz", AFCColor.Yellow, AFCColor.Green),
		new AFCName("Zzzzzz", AFCColor.Yellow, AFCColor.Blue),
		new AFCName("Zzzzzzz", AFCColor.Yellow, AFCColor.Violet),
		new AFCName("Zzzzzzzz", AFCColor.Yellow, AFCColor.Magenta)
	};

	private readonly List<string> moduleNames = new List<string>();
	private List<string> shuffledNames;

	private bool colorblindModeActive;

	private void Start()
	{
		moduleId = ++moduleIdCounter;

		if (ColorblindMode.ColorblindModeActive) colorblindModeActive = true;

		Init();
	}

	private void Init()
	{
		currentSequenceIndex = 0;
		moduleNames.Clear();

		List<string> namePool = nameData.Select(n => n.Name).ToList();
		for (int i = 0; i < 8; i++)
		{
			int index = Random.Range(0, namePool.Count);
			moduleNames.Add(namePool[index]);
			namePool.RemoveAt(index);
		}

		DoNameSorting();
		ScrambleButtons();
	}

	private AFCName GetName(string name)
	{
		return nameData.First(n => n.Name == name);
	}

	private void MoveName(string name, int position, bool condition)
	{
		int index = moduleNames.IndexOf(name);
		if (index > -1 && condition)
		{
			moduleNames.RemoveAt(index);
			moduleNames.Insert(position, name);
		}
	}

	private void DoNameSorting()
	{
		moduleNames.Sort();

		string serial = BombInfo.GetSerialNumber();
		if (serial.Last() % 2 != 0) moduleNames.Reverse();

		MoveName("Xilly", 7, BombInfo.GetPortCount() >= 3);
		MoveName("Zylly", 0, (serial[2] + serial[5]) % 3 == 0);
		MoveName("Zzzzzz", 0, BombInfo.GetBatteryCount(Battery.D) > BombInfo.GetBatteryCount(Battery.AA));
		bool litOverUnlit = BombInfo.GetOnIndicators().Count() > BombInfo.GetOffIndicators().Count();
		MoveName("Zilla", 0, litOverUnlit);
		MoveName("Zizza", 7, litOverUnlit);

		Log("Correct name order: {0}", moduleNames.Join(", "));
	}

	private void ScrambleButtons()
	{
		shuffledNames = new List<string>(moduleNames).Shuffle();
		Log("Order of buttons: {0}", shuffledNames.Join(", "));
		for (int i = 0; i < shuffledNames.Count; i++) AssignButton(i);
	}

	private void AssignButton(int i)
	{
		AFCButton button = Buttons[i];
		button.SetColors(GetName(shuffledNames[i]).Colors, colorblindModeActive);
		button.Selectable.OnInteract = delegate
		{
			button.Selectable.AddInteractionPunch();
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
			SelectButton(button, shuffledNames[i]);
			return false;
		};
	}

	private int currentSequenceIndex;

	private void SelectButton(AFCButton button, string name)
	{
		if (isSolved || button.selected) return;

		string expectedName = moduleNames[currentSequenceIndex];
		Log("Pressed {0}, expected {1}", name, expectedName);

		if (name == expectedName)
		{
			button.SetSelected(true);

			if (currentSequenceIndex == 7)
			{
				Log("Module solved.");
				Module.HandlePass();
				isSolved = true;
				return;
			}

			currentSequenceIndex++;
		}
		else
		{
			Log("Strike! Resetting module.");
			Module.HandleStrike();
			foreach (AFCButton button1 in Buttons) button1.SetSelected(false);
			Init();
		}
	}

	private void Log(string format, params object[] args)
	{
		Debug.LogFormat("[Alien Filing Colors #{0}] {1}", moduleId, string.Format(format, args));
	}



	private string TwitchHelpMessage = "!{0} 1,2,3 | !{0} 123 | !{0} 1 2,3|4-5 | Numbers are in reading order | !{0} colorblind";

	private IEnumerator ProcessTwitchCommand(string command)
	{
		switch (command.ToLowerInvariant().Split(new[] { " " }, System.StringSplitOptions.None).First())
		{
			case "colorblind":
			case "colourblind":
			case "cb":
				yield return null;
				if (!colorblindModeActive)
				{
					colorblindModeActive = true;
					foreach (AFCButton button in Buttons) button.SetColors(button.CurrentColors, true);
				}
				yield break;
		}

		foreach (char buttonChar in command.Where(char.IsDigit))
		{
			int buttonNum;
			if (int.TryParse(buttonChar.ToString(), out buttonNum))
			{
				if (buttonNum < 1 || buttonNum > 8) yield return string.Format("sendtochaterror Button number \"{0}\" is invalid.", buttonNum);
				else
				{
					Buttons[buttonNum - 1].Selectable.OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}

	private IEnumerator TwitchHandleForcedSolve()
	{
		while (!isSolved)
		{
			Buttons[shuffledNames.IndexOf(moduleNames[currentSequenceIndex])].Selectable.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
	}
}