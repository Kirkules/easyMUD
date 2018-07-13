using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace KirkProject1
{
	public class Player : Entity
	{
		protected Connection mConnection;
		public Connection Connection { 
			get { return mConnection; }
		}

		private int mCommandPermission;
		public int CommandPermission { get { return mCommandPermission; } set{ mCommandPermission = value; }}

		// Entity interface properties
		public string Name { get; set; }
		public Zone Zone { get; set; }
		public int ZoneRoomNumber { get; set; }
		public string RoomLook { get { return Name + " is here."; } set { } }
		public string LookAt { get { return "It's " + Name + "."; } set { } }
		private List<string> mShortcuts;
		public List<string> Shortcuts { get { return mShortcuts;} }
		public Room Location { get { return Zone.Rooms [ZoneRoomNumber]; } set { ZoneRoomNumber = value.ZoneRoomNumber; }}

		private int mMaxCarriedWeight;
		public int MaxCarriedWeight { get { return mMaxCarriedWeight; } }

		public List<Item> Inventory { get; set;}

		private string Prompt { get; set; }


		public Player (TcpClient client, Zone zone, string name = "Nobody", int zoneRoom = 0)
		{
			Name = name;
			mShortcuts = new List<string> ();
			mShortcuts.Add (Name.ToLower());
			mMaxCarriedWeight = 200;
			ZoneRoomNumber = zoneRoom;
			Zone = zone;
			Inventory = new List<Item> ();
			Prompt = @">> ";
			mConnection = new Connection (client);
		}

		public void ReceiveMessage(string message){
			Connection.SendMessage ("\n" + message + "\n" + Prompt);
		}

		public string GetCommandString(){
			// if the player sent a command, get it and return it
			if (Connection.Client.Connected) {
				byte[] data = Connection.PopReadBuffer ();
				if (data != null) {
					string command = Encoding.ASCII.GetString (data, 0, data.Length);
					Console.WriteLine ("Got a command (" + command + ") from Player " + Name + ".");
					return command;
				}
			}
			return "";
		}


	}
}

