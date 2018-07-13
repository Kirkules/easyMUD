using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace KirkProject1
{
	public class Room
	{
		public Zone Zone;
		public int ZoneRoomNumber; // unique identifier from the Zone's perspective

		private string mLook;
		public string Look { get { return mLook; } }
		private string mName;
		public string Name { get { return mName; } }

		public Dictionary<int, int> Exits;

		public List<Entity> Entities;
		public List<Item> Items;

		public Room(){
			DefaultRoom ();
		}

		public Room (string filename)
		{
			// Room file format:
			// --------------------------------------------------
			// Room name
			// # lines of room description to follow
			// (room description, possibly multiple lines)

			// get stuff from the given file
			try{
				Entities = new List<Entity>();
				Items = new List<Item> ();

				Exits = new Dictionary<int, int>(); // determined by the zone, not the room type

				int lineIndex = 0;
				string[] fileContents = System.IO.File.ReadAllLines(filename);
				// get the Room name
				mName = fileContents[lineIndex];
				lineIndex++;

				// get the Number of lines of Room description (for when a player "looks" in the room)
				int numLookLines = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// read room description into a single string
				mLook = String.Join("\n", new ArraySegment<string>(fileContents, lineIndex, numLookLines));
				lineIndex += numLookLines;

				// detect misformatted files (might not raise exceptions...)
				if (lineIndex != fileContents.Length){
					Console.WriteLine("Rooms/" + filename + " may not be formatted correctly.");
				}

			} catch (Exception e) {
				Console.WriteLine (e);
				DefaultRoom ();
			}
		}



		public Room(string name, string look){
			Entities = new List<Entity> ();
			Items = new List<Item> ();
			Exits = new Dictionary<int, int>(); // determined by the zone, not the room type
			mName = name;
			mLook = look;
		}

		public Room GetExit(string direction){
			return GetExit (Direction.fromString (direction));
		}

		public Room GetExit(int direction){
			if (Exits.ContainsKey(direction)){
				return Zone.Rooms[Exits[direction]];
			}
			return null;
		}
		

		// broadcast a message to all players in the room
		public void Broadcast(string message) {
			Entity[] whoHears = Entities.ToArray ();
			foreach (Entity entity in whoHears) {
				entity.ReceiveMessage (message);
			}
		}

		private void DefaultRoom(){
			mName = "Nowhere";
			mLook = "There's a whole lot of nothing here.";

			Exits = new Dictionary<int, int> ();
			Entities = new List<Entity> ();
			Items = new List<Item> ();
		}

	}
}

