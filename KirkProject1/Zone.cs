using System;
using System.Collections.Generic;
using System.Linq;

namespace KirkProject1
{
	public class Zone
	{
		private string mName;
		public string Name { get { return mName; } }

		public Room[] Rooms { get; set; }
		public List<Mob> Mobs { get; set; }

		public string Sunrise { get; set; }

		public string Sunset { get; set; }

		public Zone(){
			DefaultZone ();
		}

		public Zone (string filename, string zoneDirectory)
		{
			// Zone file format:
			// --------------------------------------------------
			// Zone name (also the folder name containing its rooms)

			// "N" number of rooms in the zone
			// N lines of "R F" where R is the zone room number and F is the corresponding filenames

			// "E" number of lines describing exits from each room
			// E lines of "R1 D R2", room number R1 has exit in direction D (full word, "east" or "west" etc) to room R2

			// "M" number of lines describing which rooms have which mobs
			// M lines of "A B C" where A is a zone room number, C is a mob filename, and B is # of copies of that mob in the room

			// "K" number of lines describing which rooms have which items on the floor
			// K lines of "A B C" where A is a zone room number, C is an item filename, and B is # of copies of the item

			// sunrise message (single line)
			// sunset message (single line)

			try {
				string[] fileContents = System.IO.File.ReadAllLines(filename);

				// get room filenames
				int lineIndex = 0;
				// get zone name
				mName = fileContents[lineIndex];
				lineIndex++;

				// get the number of rooms in the zone
				int numRooms = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get rooms
				Rooms = new Room[numRooms];
				// read all the room files
				for (int i = 0; i < numRooms; i++){
					string[] numAndName = fileContents[lineIndex + i].Split(' '); // room index, room filename
					Rooms[i] = new Room(zoneDirectory + "/Rooms/" + numAndName[1]);
					Rooms[i].ZoneRoomNumber = int.Parse(numAndName[0]); // just the index in Rooms, really
					Rooms[i].Zone = this;
				}
				lineIndex += numRooms;

				// get the number of lines describing exits
				int numExitLines = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get the room exits info.
				string[] exitInfo;
				for (int i = 0; i < numExitLines; i++){
					exitInfo = fileContents[lineIndex + i].Split(' '); // room 1, direction, room2
					// add the exit to the corresponding room
					Rooms[int.Parse(exitInfo[0])].Exits.Add(Direction.fromString(exitInfo[1]), int.Parse(exitInfo[2]));
				}
				lineIndex += numExitLines;

				// get number of lines describing Mobs
				Mobs = new List<Mob>();
				int numMobLines = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get mobs for each room
				string[] mobInfo;
				for (int i = 0; i < numMobLines; i++){
					mobInfo = fileContents[lineIndex + i].Split(' '); // room #, mob filename, number of copies of the mob
					for (int j = 0; j < int.Parse(mobInfo[2]); j++){
						Mob theMob = new Mob(zoneDirectory + "/Mobs/" + mobInfo[1]);
						int zoneRoomNumber = int.Parse(mobInfo[0]);
						theMob.Zone = this;
						theMob.Location = Rooms[zoneRoomNumber];
						Rooms[zoneRoomNumber].Entities.Add(theMob);
					}
				}
				lineIndex += numMobLines;

				// get number of lines describing Items
				int numItemLines = int.Parse(fileContents[lineIndex]);
				lineIndex++;

				// get Items for each room
				string[] itemInfo;
				for (int i = 0; i < numItemLines; i++){
					itemInfo = fileContents[lineIndex + i].Split(' '); // room #, item filename, # copies of item
					for (int j = 0; j < int.Parse(itemInfo[2]); j++){
						Rooms[int.Parse(itemInfo[0])].Items.Add(new Item(zoneDirectory + "/Items/" + itemInfo[1]));
					}
				}
				lineIndex += numItemLines;

				// get sunrise/sunset info
				Sunrise = fileContents[lineIndex];
				lineIndex++;
				Sunset = fileContents[lineIndex];
				lineIndex++;

				// detect misformatted files (might not raise exceptions...)
				if (lineIndex != fileContents.Length){
					Console.WriteLine("Rooms/" + filename + " may not be formatted correctly.");
				}
			} catch (Exception e){
				Console.WriteLine (e);
				Console.WriteLine (e.Source);
				DefaultZone ();
			}
		}

		// default 1-room zone if making a zone from files effs up
		private void DefaultZone(){
			Rooms = new Room[1];
			string newName = "Somewhere in the vast void of space";
			string newLook = "There isn't much here. Everywhere you look, you \n" +
				"see a dense carpet of tiny points of light. You have a strange feeling\n" +
				"that there should really be something here, but of course there isn't.\n";
			Rooms [0] = new Room (name: newName, look: newLook);
			Rooms [0].ZoneRoomNumber = 0;
		}

	}
}
