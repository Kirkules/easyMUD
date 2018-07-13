using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace KirkProject1
{
	public class Mob : Entity
	{

		// Entity interface variables
		public string Name { get; set;}
		private string mRoomLook;
		public string RoomLook { get { return mRoomLook; } set { } }
		private List<string> mShortcuts;
		public List<string> Shortcuts { get { return mShortcuts;} }
		private string mLookAt;
		public string LookAt { get { return mLookAt; } set { } }
		public List<Timer> ActionTimers;
		public List<Tuple<string, int>> ActionTimes;

		public List<Stopwatch> ActionTimer { get; set; }

		public Zone Zone { get; set; }
		public int ZoneRoomNumber { get; set; }
		public Room Location { get { return Zone.Rooms [ZoneRoomNumber]; } set { ZoneRoomNumber = value.ZoneRoomNumber; }}

		public int CommandPermission { get { return 0; } }

		public List<Item> Inventory { get; set; }

		public Mob(){
			DefaultMob ();
		}

		public Mob (string filename)
		{
			// Mob file format:
			// --------------------------------------------------
			// Mob name
			// Mob's one-line short room-look description
			// # of lines of mob's "look at" description
			// (mob's look-at description)
			// "N" number of timed actions for the mob
			// N lines of "D M" where D is the number of seconds delay between doing things, M is the command to the MUD to do
			// "S" number of shortcut ways of referring to mob
			// S lines of "s" where s is a string that the mob may be referred to
			try {
				int lineIndex = 0;
				string[] fileContents = System.IO.File.ReadAllLines(filename);

				// get the mob name
				Name = fileContents[lineIndex];
				lineIndex++;

				// get the mob one-line description
				mRoomLook = fileContents[lineIndex];
				lineIndex++;

				// get number of lines of look-at description
				int numLookAtLines = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get look-at description
				mLookAt = String.Join("\n", new ArraySegment<string>(fileContents, lineIndex, numLookAtLines));
				lineIndex += numLookAtLines;

				// get number of mob's timed actions
				int numTimedActions = int.Parse(fileContents[lineIndex]);
				lineIndex++;


				// get mob's timed actions
				ActionTimes = new List<Tuple<string, int>>();
				foreach (string line in (new ArraySegment<string>(fileContents, lineIndex, numTimedActions))){
					// TODO: finish this and then read rest of file
					string[] words = line.Split(' ');
					int time = Math.Max(2, int.Parse(words[0]))*1000; // milliseconds, so multiply by 1000
					string command = string.Join(" ", new ArraySegment<string>(words, 1, words.Length-1));

					Timer actionTimer = new Timer(time);
					actionTimer.Elapsed += (sender, e) => Server.Instance().MobCommand(this, command);
					actionTimer.AutoReset = true;
					actionTimer.Enabled = true;


					lineIndex++;
				}

				int numShortcuts = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				mShortcuts = new List<string>();
				mShortcuts.Add(Name.ToLower());
				for (int i = 0; i < numShortcuts; i++){
					mShortcuts.Add(fileContents[lineIndex + i].ToLower());
				}
				lineIndex += numShortcuts;

				// try to detect misformatted files (might not raise exceptions...)
				if (lineIndex != fileContents.Length){
					Console.WriteLine("Mobs/" + filename + " may not be formatted correctly.");
				}
					

			} catch (Exception e) {
				Console.WriteLine (e);
				Console.WriteLine ("Couldn't read " + filename + ". Using default mob.");
				DefaultMob ();
			}
		}

		public void ReceiveMessage(string s){
			// do nothing (most mobs shouldn't respond to receiving a message)
		}

		private void DefaultMob(){
			Name = "A MOB";
			mRoomLook = "There is something moving around here.";
			mLookAt = "It seems alive, but you can't tell much else.";

		}
	}
}

