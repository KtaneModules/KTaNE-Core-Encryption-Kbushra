using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using Rnd = UnityEngine.Random;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable[] buttons;
	public KMSelectable[] arrows;
	public TextMesh[] Displays;
	public GameObject indicator;
	static int ModuleIdCounter = 1;
	int ModuleId;

	private int randNum;
	private int randNum2;
	private string randText;
	private string[] randTextStore = new string[4];
	private double[] textOrder = new double[4];
	private double[] numShifts = new double[4];
	private int[] intShifts = new int[4];
	private int[] mazeCoords = new int[5];
	private int[] currentPos = new int[2];

	private int output;

	public Renderer displayRend;
	public Material orange;
	public Material black;

	private int[,] maze1 = new int[8,8]
	{
		{0, 0, 0, 0, 0, 0, 0, 0},
		{0, 1, 0, 1, 0, 1, 1, 1},
		{0, 1, 0, 1, 0, 0, 0, 1},
		{0, 1, 0, 1, 1, 1, 0, 1},
		{0, 1, 0, 1, 0, 0, 0, 1},
		{0, 1, 0, 1, 1, 1, 0, 1},
		{0, 1, 0, 1, 0, 0, 0, 1},
		{0, 1, 0, 0, 0, 1, 0, 1}
	};

	private int[,] maze2 = new int[8,8]
	{
		{0, 0, 0, 0, 0, 0, 1, 0},
		{0, 1, 1, 1, 1, 0, 1, 0},
		{0, 1, 0, 0, 0, 0, 1, 0},
		{0, 1, 1, 1, 1, 0, 1, 0},
		{0, 0, 0, 1, 0, 0, 0, 0},
		{0, 1, 0, 1, 1, 1, 1, 0},
		{0, 1, 0, 0, 0, 0, 1, 0},
		{0, 1, 0, 1, 1, 0, 0, 0}
	};


	bool _isSolved = false;
	bool incorrect = false;
	bool maze1chosen = false;
	bool solvedMaze = false;
	bool lightsOn = false;
	bool scroll = false;
	bool scroll2 = false;
	bool flashingOn = false;

	void Awake()
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in buttons)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { buttonPress(pressedButton); return false; };
		}
		foreach (KMSelectable arrow in arrows)
		{
			KMSelectable pressedArrow = arrow;
			arrow.OnInteract += delegate () { arrowPress(pressedArrow); return false; };
		}
		module.OnActivate = delegate() {lightsOn = true;};
	}

	void FixedUpdate ()
	{
		if (lightsOn == true && scroll == false) 
		{
			StartCoroutine (ScreenScroll ());
			scroll = true;
		}
	}

	IEnumerator Peek()
	{
		Displays [2].text = (currentPos [0] + 1).ToString () + "," + (currentPos [1] + 1).ToString ();
		yield return new WaitForSeconds(3f);
		Displays [2].text = "??";
		yield break;
	}

	IEnumerator ScreenScroll()
	{
		if (maze1chosen == true && maze1 [mazeCoords [2], mazeCoords [3]] == 6) 
		{
			maze1 [mazeCoords [2], mazeCoords [3]] = maze1 [mazeCoords [2], mazeCoords [3]] - 5;
		}
		else if (maze1chosen == true) 
		{
			maze1 [mazeCoords [2], mazeCoords [3]] = 0;
		}
		else if (maze1chosen == false && maze2 [mazeCoords [2], mazeCoords [3]] == 6) 
		{
			maze2 [mazeCoords [2], mazeCoords [3]] = maze2 [mazeCoords [2], mazeCoords [3]] - 5;
		}
		else if (maze1chosen == false) 
		{
			maze2 [mazeCoords [2], mazeCoords [3]] = 0;
		}

		maze1chosen = false;

		for(int i = 0; i < 4; i++)
		{
			randNum = Rnd.Range (11111, 44444);
			randText = randNum.ToString ();
			randTextStore [i] = randText;
			Displays [1].text = randText;
		}

		for (int i = 0; i < 5; i++) 
		{
			mazeCoords [i] = (randTextStore [0].ToCharArray () [i] + randTextStore [1].ToCharArray () [i] + randTextStore [2].ToCharArray () [i] + randTextStore [3].ToCharArray () [i]) % 8;
		}

		if (mazeCoords [4] % 2 == 0)
		{
			maze1chosen = true;
			maze1 [mazeCoords [2], mazeCoords [3]] = maze1 [mazeCoords [2], mazeCoords [3]] + 5;
			if (maze1 [mazeCoords [0], mazeCoords [1]] == 1 || maze1 [mazeCoords [0], mazeCoords [1]] == 5 || maze1 [mazeCoords [2], mazeCoords [3]] == 6) 
			{
				StartCoroutine (ScreenScroll ());
				yield break;
			}
		} 
		else 
		{
			maze2 [mazeCoords [2], mazeCoords [3]] = maze2 [mazeCoords [2], mazeCoords [3]] + 5;
			if (maze2 [mazeCoords [0], mazeCoords [1]] == 1 || maze2 [mazeCoords [0], mazeCoords [1]] == 5 || maze2 [mazeCoords [2], mazeCoords [3]] == 6)
			{
				StartCoroutine (ScreenScroll ());
				yield break;
			}
		}

		Debug.LogFormat("[Core Encryption #{0}] The starting position of the maze is at {1},{2} and the goal is at {3}, {4} and the number determining the maze is {5}", ModuleId, mazeCoords[0] + 1, mazeCoords[1] + 1, mazeCoords[2] + 1, mazeCoords[3] + 1, mazeCoords[4]);

		currentPos [0] = mazeCoords [0];
		currentPos [1] = mazeCoords [1];

		while(scroll2 == false && flashingOn == false)
		{
			for(int j = 0; j < 4; j++)
			{
				yield return new WaitForSeconds (0.4f);
				Displays [1].text = randTextStore [j];
				if (flashingOn == true) 
				{
					break;
				}
			}
		}
		if (flashingOn == true) 
		{
			displayRend = indicator.GetComponent<Renderer>();
			displayRend.enabled = true;
			Displays [2].gameObject.SetActive (false);
			randNum2 = Rnd.Range (0, 3);

			Calculate ();

			while (solvedMaze == true && _isSolved == false)
			{
				for(int j = 0; j < 4; j++)
				{
					yield return new WaitForSeconds (0.4f);
					Displays [1].text = randTextStore [j];
					if (Displays [1].text == randTextStore [randNum2])
					{
						displayRend.sharedMaterial = orange;
					} 
					else 
					{
						displayRend.sharedMaterial = black;
					}
				}
			}
		}
	}

	void Calculate()
	{
		textOrder[0] = double.Parse(randTextStore [randNum2]);
		textOrder[1] = double.Parse(randTextStore [(randNum2 + 1) % 4]);
		textOrder[2] = double.Parse(randTextStore [(randNum2 + 2) % 4]);
		textOrder[3] = double.Parse(randTextStore [(randNum2 + 3) % 4]);

		numShifts [0] = ((textOrder [0] * textOrder [0]) + (char.GetNumericValue(textOrder [0].ToString().ToCharArray() [0]) * char.GetNumericValue(textOrder [0].ToString().ToCharArray() [1]) * char.GetNumericValue(textOrder [0].ToString().ToCharArray() [2]) * char.GetNumericValue(textOrder [0].ToString().ToCharArray() [3]) * char.GetNumericValue(textOrder [0].ToString().ToCharArray() [4]))) % 88889;
		numShifts [1] = (char.GetNumericValue(textOrder [0].ToString().ToCharArray () [0]) + char.GetNumericValue(textOrder [1].ToString().ToCharArray() [1]) - char.GetNumericValue(textOrder [2].ToString().ToCharArray() [2]) + char.GetNumericValue(textOrder [3].ToString().ToCharArray() [3]) + 10) % 88889;
		numShifts [2] = ((textOrder [2] * textOrder [0]) / (textOrder [1] * textOrder [3]) + 9999) % 88889;
		numShifts [3] = ((char.GetNumericValue(textOrder [2].ToString ().ToCharArray () [2]) * textOrder [3]) / (textOrder [1] - char.GetNumericValue(textOrder [0].ToString ().ToCharArray () [2])) + 11111) % 88889;

		intShifts [0] = (int)Mathf.Floor ((float)numShifts [0]);
		intShifts [1] = (int)Mathf.Floor ((float)numShifts [1]);
		intShifts [2] = (int)Mathf.Floor ((float)numShifts [2]);
		intShifts [3] = (int)Mathf.Floor ((float)numShifts [3]);

		for (int i = 0; i < 4; i++)
		{
			if (intShifts[i] < 10000) 
			{
				intShifts[i] = intShifts[i] + 10000;
			}
		}

		output = ((int.Parse (randTextStore [0]) + intShifts [0]) - (int.Parse (randTextStore [1]) + intShifts [1]) + (int.Parse (randTextStore [2]) + intShifts [2]) - (int.Parse (randTextStore [3]) + intShifts [3]) + 1000000) % 44445;
		char[] outputchar = output.ToString ().ToCharArray ();

		for(int i = 0; i < output.ToString().ToCharArray().Length; i++)
		{
			if (output.ToString ().ToCharArray () [i] > 4) 
			{
				outputchar [i] = (Convert.ToInt32 (outputchar [i].ToString ()) % 5).ToString().ToCharArray()[0];
				string outputstring = new string (outputchar);
				output = int.Parse (outputstring);
			}
		}
		Debug.LogFormat("[Core Encryption #{0}] The 4 shifts are {1}, {2}, {3} and {4}, with {5} being the answer", ModuleId, intShifts[0], intShifts[1], intShifts[2], intShifts[3], output);
	}

	void buttonPress(KMSelectable pressedButton)
	{
		if(solvedMaze == true)
		{
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			pressedButton.AddInteractionPunch (1f);
			int buttonPosition = Array.IndexOf(buttons, pressedButton);

			switch (buttonPosition) 
			{
			case 0:
				if(Displays [0].text.ToCharArray ().Length > 1 || Displays [0].text.ToCharArray ().Length < 5)
				{
					Displays[0].text = (int.Parse (Displays [0].text) * 10).ToString ();
				}
				break;
			case 1:
				if (Displays [0].text.ToCharArray ().Length == 1 && Displays [0].text.ToCharArray () [0] == 0.ToString ().ToCharArray() [0]) 
				{
					Displays[0].text = ((int.Parse (Displays [0].text) + 1) * 10).ToString ();
					Displays [0].text = "01";
				}
				else if(Displays [0].text.ToCharArray ().Length < 5)
				{
					Displays[0].text = ((int.Parse (Displays [0].text) * 10) + 1).ToString ();
				}
				break;
			case 2:
				if (Displays [0].text.ToCharArray ().Length == 1 && Displays [0].text.ToCharArray () [0] == 0.ToString ().ToCharArray() [0]) 
				{
					Displays[0].text = ((int.Parse (Displays [0].text) + 1) * 10).ToString ();
					Displays [0].text = "02";
				}
				else if(Displays [0].text.ToCharArray ().Length < 5)
				{
					Displays[0].text = ((int.Parse (Displays [0].text) * 10) + 2).ToString ();
				}
				break;
			case 3:
				if (Displays [0].text.ToCharArray ().Length == 1 && Displays [0].text.ToCharArray () [0] == 0.ToString ().ToCharArray() [0]) 
				{
					Displays[0].text = ((int.Parse (Displays [0].text) + 1) * 10).ToString ();
					Displays [0].text = "03";
				}
				else if(Displays [0].text.ToCharArray ().Length < 5)
				{
					Displays[0].text = ((int.Parse (Displays [0].text) * 10) + 3).ToString ();
				}
				break;
			case 4:
				if (Displays [0].text.ToCharArray ().Length == 1 && Displays [0].text.ToCharArray () [0] == 0.ToString ().ToCharArray() [0]) 
				{
					Displays[0].text = ((int.Parse (Displays [0].text) + 1) * 10).ToString ();
					Displays [0].text = "04";
				}
				else if(Displays [0].text.ToCharArray ().Length < 5)
				{
					Displays[0].text = ((int.Parse (Displays [0].text) * 10) + 4).ToString ();
				}
				break;
			case 5:
				if (Displays [0].text.ToCharArray ().Length == 1 && Displays [0].text.ToCharArray () [0] != 0.ToString ().ToCharArray() [0]) 
				{
					Displays[0].text = 0.ToString ();
				}
				else
				{
					Displays[0].text = ((int) Math.Floor((double) (int.Parse (Displays [0].text) / 10))).ToString ();
				}
				break;
			case 6:
				if (int.Parse (Displays [0].text) == output)
				{
					scroll2 = true;
					StartCoroutine (ScreenScroll2 ());
					Log ("Harder flashes commenced");
				}
				else
				{
					incorrect = true;
				}
				break;
			}
			if (incorrect) 
			{
				module.HandleStrike ();
				Log ("Striked!");
				incorrect = false;
			}
		}
		else
		{
			incorrect = true;
		}
		if (incorrect) 
		{
			module.HandleStrike ();
			Log ("Striked!");
			incorrect = false;
		}

	}

	void arrowPress(KMSelectable pressedArrow)
	{
		if (solvedMaze == false) 
		{
			audio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, transform);
			pressedArrow.AddInteractionPunch (0.2f);
			int buttonPosition = Array.IndexOf (arrows, pressedArrow);

			switch (buttonPosition)
			{
			case 0:
				if (maze1chosen == true)
				{
					if (currentPos [0] - 1 < 0)
					{
						incorrect = true;
					} 
					else if (maze1 [currentPos [0] - 1, currentPos [1]] == 1) 
					{
						incorrect = true;
					}
					else 
					{
						currentPos [0] = currentPos [0] - 1;
					}
					if (maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords[2],mazeCoords[3]] && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				} 
				else 
				{
					if (currentPos [0] - 1 < 0)
					{
						incorrect = true;
					}
					else if (maze2 [currentPos [0] - 1, currentPos [1]] == 1)
					{
						incorrect = true;
					}
					else 
					{
						currentPos [0] = currentPos [0] - 1;
					}
					if (maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords[2],mazeCoords[3]] && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				}
				break;
			case 1:
				if (maze1chosen == true) 
				{
					if (currentPos [1] + 1 > 7)
					{
						incorrect = true;
					} 
					else if (maze1 [currentPos [0], currentPos [1] + 1] == 1)
					{
						incorrect = true;
					}
					else 
					{
						currentPos [1] = currentPos [1] + 1;
					}
					if (maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords[2],mazeCoords[3]] && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				} 
				else 
				{
					if (currentPos [1] + 1 > 7) 
					{
						incorrect = true;
					}
					else if (maze2 [currentPos [0], currentPos [1] + 1] == 1) 
					{
						incorrect = true;
					}
					else
					{
						currentPos [1] = currentPos [1] + 1;
					}
					if (maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords[2],mazeCoords[3]] && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				}
				break;
			case 2:
				if (maze1chosen == true) 
				{
					if (currentPos [0] + 1 > 7) 
					{
						incorrect = true;
					} 
					else if (maze1 [currentPos [0] + 1, currentPos [1]] == 1) 
					{
						incorrect = true;
					}
					else 
					{
						currentPos [0] = currentPos [0] + 1;
					}
					if (maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords[2],mazeCoords[3]] && scroll2 == true && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				} 
				else 
				{
					if (currentPos [0] + 1 > 7) 
					{
						incorrect = true;
					}
					else if (maze2 [currentPos [0] + 1, currentPos [1]] == 1) 
					{
						incorrect = true;
					} 
					else 
					{
						currentPos [0] = currentPos [0] + 1;
					}
					if (maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords[2],mazeCoords[3]] && scroll2 == true && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				}
				break;
			case 3:
				if (maze1chosen == true)
				{
					if (currentPos [1] - 1 < 0) 
					{
						incorrect = true;
					} 
					else if (maze1 [currentPos [0], currentPos [1] - 1] == 1) 
					{
						incorrect = true;
					}
					else 
					{
						currentPos [1] = currentPos [1] - 1;
					}
					if (maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze1 [currentPos [0], currentPos [1]] == maze1 [mazeCoords[2],mazeCoords[3]] && scroll2 == true && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				}
				else
				{
					if (currentPos [1] - 1 < 0)
					{
						incorrect = true;
					} 
					else if (maze2 [currentPos [0], currentPos [1] - 1] == 1)
					{
						incorrect = true;
					}
					else 
					{
						currentPos [1] = currentPos [1] - 1;
					}
					if (maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords [2], mazeCoords [3]] && scroll2 == false) 
					{
						solvedMaze = true;
						flashingOn = true;
						Log ("Flashings commenced");
					}
					else if(maze2 [currentPos [0], currentPos [1]] == maze2 [mazeCoords[2],mazeCoords[3]] && scroll2 == true && scroll2 == true)
					{
						module.HandlePass ();
						Log ("Module solved!");
						_isSolved = true;
					}
				}
				break;
			}
			Debug.LogFormat ("[Core Encryption #{0}] Your current position is now {1},{2}", ModuleId, currentPos [0] + 1, currentPos [1] + 1);
		}
		else
		{
			incorrect = true;
		}
		if (incorrect)
		{
			module.HandleStrike ();
			Log ("Striked!");
			StartCoroutine (Peek ());
			incorrect = false;
		}
	}

	IEnumerator ScreenScroll2()
	{
		char[] chara = textOrder [0].ToString ().ToCharArray ();
		scroll2 = true;
		if (maze1chosen == true && maze1 [mazeCoords [2], mazeCoords [3]] == 6) 
		{
			maze1 [mazeCoords [2], mazeCoords [3]] = maze1 [mazeCoords [2], mazeCoords [3]] - 5;
		}
		else if (maze1chosen == true) 
		{
			maze1 [mazeCoords [2], mazeCoords [3]] = 0;
		}
		else if (maze1chosen == false && maze2 [mazeCoords [2], mazeCoords [3]] == 6) 
		{
			maze2 [mazeCoords [2], mazeCoords [3]] = maze2 [mazeCoords [2], mazeCoords [3]] - 5;
		}
		else if (maze1chosen == false) 
		{
			maze2 [mazeCoords [2], mazeCoords [3]] = 0;
		}
		solvedMaze = false;
		maze1chosen = false;
		for(int i = 0; i < 4; i++)
		{
			randNum = Rnd.Range (11111, 44444);
			randText = randNum.ToString ();
			randTextStore [i] = randText;
		}

		for (int i = 0; i < 5; i++) 
		{
			mazeCoords [i] = (randTextStore [0].ToCharArray () [i] + randTextStore [1].ToCharArray () [i] + randTextStore [2].ToCharArray () [i] + randTextStore [3].ToCharArray () [i]) % 8;
		}
			
		mazeCoords [0] = (mazeCoords[0] + Convert.ToInt32 (chara[0].ToString ())) % 8;
		mazeCoords [1] = (mazeCoords[1] + Convert.ToInt32 (chara[4].ToString ())) % 8;


		if (mazeCoords [4] % 2 == 0)
		{
			maze1chosen = true;
			mazeCoords [2] = (mazeCoords [2] + info.GetPortCount ()) % 8;
			mazeCoords [3] = (mazeCoords [3] + info.GetBatteryCount ()) % 8;
			maze1 [mazeCoords [2], mazeCoords [3]] = maze1 [mazeCoords [2], mazeCoords [3]] + 5;
			if (maze1 [mazeCoords [0], mazeCoords [1]] == 1 || maze1 [mazeCoords [0], mazeCoords [1]] == 5 || maze1 [mazeCoords [2], mazeCoords [3]] == 6) 
			{
				StartCoroutine (ScreenScroll2 ());
				yield break;
			}
		} 
		else 
		{
			mazeCoords [2] = (mazeCoords [2] + info.GetPortPlateCount ()) % 8;
			mazeCoords [3] = (mazeCoords [3] + info.GetBatteryHolderCount ()) % 8;
			maze2 [mazeCoords [2], mazeCoords [3]] = maze2 [mazeCoords [2], mazeCoords [3]] + 5;
			if (maze2 [mazeCoords [0], mazeCoords [1]] == 1 || maze2 [mazeCoords [0], mazeCoords [1]] == 5 || maze2 [mazeCoords [2], mazeCoords [3]] == 6)
			{
				StartCoroutine (ScreenScroll2 ());
				yield break;
			}
		}

		Debug.LogFormat("[Core Encryption #{0}] The starting position of the maze is at {1},{2} and the goal is at {3}, {4} and the number determining the maze is {5}", ModuleId, mazeCoords[0] + 1, mazeCoords[1] + 1, mazeCoords[2] + 1, mazeCoords[3] + 1, mazeCoords[4]);

		currentPos [0] = mazeCoords [0];
		currentPos [1] = mazeCoords [1];

		while(_isSolved == false)
		{
			for(int j = 0; j < 4; j++)
			{
				yield return new WaitForSeconds (0.2f);
				Displays [1].text = randTextStore [j];
			}
		}
		Displays [1].text = "";
	}

	void Log(string message)
	{
		Debug.LogFormat("[Core Encryption #{0}] {1}", ModuleId, message);
	}

	#pragma warning disable 414
	private string TwitchHelpMessage = "!{0} pressarrow # [Presses the specified arrow (1 = up and goes clockwise)], !{0} pressbutton # [Presses the specified button (goes by the number it shows, delete is 5, submit is 6)].";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		int num;
		string[] parameters = command.Split(' ');
		parameters [0] = parameters [0].ToLower ();

		if (parameters[0] == "pressarrow") 
		{
			if (parameters.Length == 1) 
			{
				yield return "sendtochaterror Not enough parameters!";
				yield break;
			}
			else if(parameters.Length > 2)
			{
				yield return "sendtochaterror Too many parameters!";
				yield break;
			}
			else if(int.TryParse(parameters[1], out num) == false)
			{
				yield return "sendtochaterror Second parameter is not a number!";
				yield break;
			}
			else if(int.Parse(parameters[1]) < 1 || int.Parse(parameters[1]) > 4)
			{
				yield return "sendtochaterror Second parameter is not in range!";
				yield break;
			}
			else
			{
				yield return null;
				arrows [int.Parse (parameters [1]) - 1].OnInteract();
				yield break;
			}
		}
		else if (parameters[0] == "pressbutton") 
		{
			if (parameters.Length == 1) 
			{
				yield return "sendtochaterror Not enough parameters!";
				yield break;
			}
			else if(parameters.Length > 2)
			{
				yield return "sendtochaterror Too many parameters!";
				yield break;
			}
			else if(int.TryParse(parameters[1], out num) == false)
			{
				yield return "sendtochaterror Second parameter is not a number!";
				yield break;
			}
			else if(int.Parse(parameters[1]) < 0 || int.Parse(parameters[1]) > 6)
			{
				yield return "sendtochaterror Second parameter is not in range!";
				yield break;
			}
			else
			{
				yield return null;
				buttons [int.Parse (parameters [1])].OnInteract();
				yield break;
			}
		}
		else
		{
			yield return "sendtochaterror That command does not exist!";
			yield break;
		}
	}
}

