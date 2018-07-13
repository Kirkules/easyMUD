using System;
using System.Collections.Generic;

namespace KirkProject1
{
	public class Item : Targetable
	{
		public Zone Zone { get; set; }
		public int ZoneRoomNumber { get; set; }
		private string mName;
		public string Name { get { return mName; } set { }}

		private int mWeight;
		public int Weight { get { return mWeight; } }

		private string mRoomLook;
		public string RoomLook { get { return mRoomLook; } set { } }

		private string mLookAt;
		public string LookAt { get { return mLookAt; } set { }}

		// implement targetable
		private List<string> mShortcuts;
		public List<string> Shortcuts { get { return mShortcuts;} }
		public Room Location { get { return Zone.Rooms [ZoneRoomNumber]; } set { ZoneRoomNumber = value.ZoneRoomNumber; }}

		public Item (string filename)
		{
			// Item file format:
			// --------------------------------------------------
			// Item name
			// Item's one-line short room-look description (if it's just on the floor)
			// Item's weight
			// # of lines of Item's "look at" description
			// (Item's look-at or examine description)
			// "S" number of shortcut lines
			// S lines of "s" where s is a single word shortcut
			try {
				int lineIndex = 0;
				string[] fileContents = System.IO.File.ReadAllLines(filename);

				// get the item's name
				mName = fileContents[lineIndex];
				lineIndex++;

				// get the item's look-description (when you "look" in the room containing it)
				mRoomLook = fileContents[lineIndex];
				lineIndex++;

				// get the item's weight
				mWeight = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get # of lines of look-at description
				int numLookLines = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get look-at description
				mLookAt = String.Join("\n", new ArraySegment<string>(fileContents, lineIndex, numLookLines));
				lineIndex += numLookLines;

				// get targeting shortcuts
				mShortcuts = new List<string>();
				int numShortcuts = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				for (int i = 0; i < numShortcuts; i++){
					mShortcuts.Add(fileContents[lineIndex+i].ToLower());
				}
				lineIndex += numShortcuts;


				// try to detect misformatted files (might not raise exceptions...)
				if (lineIndex != fileContents.Length){
					Console.WriteLine("Items/" + filename + " may not be formatted correctly.");
				}
			} catch (Exception e) {
				Console.WriteLine (e);
				DefaultItem ();
			}

		}

		private void DefaultItem(){

		}
	}
}

