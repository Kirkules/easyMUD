using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace KirkProject1
{
	public class Server
	{
		private TcpListener mListener;

		private List<Player> mPlayers;

		private ConcurrentQueue<TcpClient> mClientQueue;

		private Task acceptingTask;
		private string TopDirectory;

		public ConcurrentQueue<Task> mudEventQueue;

		public int DaysPassed;

		private Stopwatch mTimer;
		public Stopwatch MUDTimer { get { return mTimer; } }


		private int TcpBacklogQueueSize { get; set; }

		private Commands Commands { get; set; }

		// Only one server!
		private static Server mInstance = null;
		public static Server Instance(int port = 1337) {
			if (mInstance == null) {
				mInstance = new Server (port);
			}
			return mInstance;
		}

		public List<Zone> Zones;

		public TcpListener Listener 
		{
			get { return mListener; }
		}

		public int Port { get; set; }
		public bool Shutdown { get; set; }

		public Server (int port = 1337, int maxPlayers = 10)
		{
			// pass in any, so that we'll listen on all local IP addresses 
			mListener = new TcpListener (System.Net.IPAddress.Any, port);
			mClientQueue = new ConcurrentQueue<TcpClient> ();
			mPlayers = new List<Player>();
			mTimer = new Stopwatch ();
			DaysPassed = 0;
			mudEventQueue = new ConcurrentQueue<Task> ();

			

			// Get the top level directory
			if (Directory.Exists(Directory.GetCurrentDirectory() + "/Zones")) {
				TopDirectory = Directory.GetCurrentDirectory();
			} else {
				TopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/KirkMUD";
			}

			// load up command stuff
			Commands = new Commands (TopDirectory + "/CommandPermissions.txt");
			// load up the zones
			Zones = new List<Zone> ();
			LoadZones (TopDirectory);
			Task.Factory.StartNew (ProcessEvents);
		}

		private async void ProcessEvents(){
			Task NextEvent;
			while (!Shutdown) {
				// try to get input from players
				foreach (Player player in mPlayers)
				{
					string CommandString = player.GetCommandString ().Trim();

					// only make a command handling event if there's a command to process
					if (CommandString != "" && CommandString != "\n") {
						mudEventQueue.Enqueue (new Task (
							() => {
								Commands.HandleCommand (player, CommandString);
							}
						));
					}
				}

				if (mudEventQueue.TryDequeue (out NextEvent)) {
					NextEvent.Start ();
					await NextEvent;
				}
			}
		}


		public void Listen()
		{
			// start listening
			Listener.Start (TcpBacklogQueueSize);
			HandleListen ();
		}

		public void Run()
		{
			Listen ();
			MUDTimer.Start ();
			// Let there be time.
			Task.Factory.StartNew (DayCycle);
			// Let there be players.
			// Let there be player input.
			while (!Shutdown)
			{
				if (mClientQueue.Count > 0)
				{
					Console.WriteLine ("A challenger has arrived!");
					TcpClient client;
					while (mClientQueue.TryDequeue (out client) != true)
						;

					// now we can create the player. There had better be a zone!
					Task.Factory.StartNew( () => IntroducePlayer(client));
				}
				// being done in ProcessEvents: ProcessPlayerInput ();
			}
		}
			
		public void MobCommand(Mob mob, string command){
			mudEventQueue.Enqueue (new Task (
				() => {
					Commands.HandleCommand (mob, command);
				}
			));
		}

		public void DayCycle(){
			int hourLength = 1000 * 20; // 1 game hour = 20 seconds
			int dayLength = hourLength * 24;
			int sunrise = 6*hourLength; // 6am = sunrise
			bool sunriseHappened = false;
			int sunset = 50; // 6pm = sunset
			bool sunsetHappened = false;
			while (!Shutdown) {
				Task.Delay(500).Wait(); // wait half a second, then do checks
				if (Math.Abs(MUDTimer.ElapsedMilliseconds - sunrise) < 50 && !sunriseHappened) {
					foreach (Player player in mPlayers) {
						if (player != null) {
							mudEventQueue.Enqueue (new Task (
								() => {
									player.ReceiveMessage (player.Zone.Sunrise);
								}
							));
						}
					}
					sunriseHappened = true;
				}
				if (Math.Abs(MUDTimer.ElapsedMilliseconds - sunset) < 50 && !sunsetHappened) {
					foreach (Player player in mPlayers) {
						if (player != null) {
							mudEventQueue.Enqueue (new Task (
								() => {
									player.ReceiveMessage (player.Zone.Sunset);
								}
							));
						}
					}
					sunsetHappened = true;
				}
				if (MUDTimer.ElapsedMilliseconds > dayLength) {
					MUDTimer.Reset ();
					DaysPassed++;
					sunsetHappened = false;
					sunriseHappened = false;
				}
			}
		}


		public void IntroducePlayer(TcpClient client){
			Player player = new Player (client, Zones[0], "Nobody" + (mPlayers.Count+1).ToString());
			player.ReceiveMessage ("Hello! Welcome to Kirk's Project 1 MUD!\n\n What is your name? ");
			player.CommandPermission = -1; // don't allow other commands before introducing themselves
			string userInput = "";
			string tentativeName = "";
			bool userAccepted = false;
			while (userInput == "" && !userAccepted) {
				userInput = player.GetCommandString ();

				if (userInput != "" && tentativeName == "") {
					tentativeName = userInput;
					userInput = "";
					player.ReceiveMessage ("Are you sure you want to go by " + tentativeName + "? (yes/no) ");
				} else if (userInput != "" && tentativeName != "") {
					if ("yes".StartsWith (userInput.ToLower ())) {
						player.Name = tentativeName;
						player.ReceiveMessage ("\nTeleporting to the spaceport...\n\n");
						player.CommandPermission = 1;
						player.Shortcuts.Add (tentativeName.ToLower ());
						mudEventQueue.Enqueue (new Task(
							() => {
								player.Location.Entities.Add (player);
								mPlayers.Add(player);
							}
						));
						userAccepted = true;
					} else {
						tentativeName = "";
						userInput = "";
						player.ReceiveMessage ("\nWhat is your name? ");
					}
				}
			}
		}

		public void RemovePlayer(Player player){
			mudEventQueue.Enqueue (new Task (
				() => {
					mPlayers.Remove(player);
					Console.WriteLine ("Removed player " + player.Name + ".");
				}
			));
		}



		public void HandleListen()
		{
			acceptingTask = Task.Factory.StartNew (
				() => { 
					while (true) {
						TcpClient client = Listener.AcceptTcpClient();
						mClientQueue.Enqueue(client);
					}
				}
			);
		}

		public void LoadZones(string topLevelDirectory){
			try{
				// Look for Zone files. Requires "Zones" folder in "~/KirkMUD/", or in folder containing program
				string ZonesDirectory = "";
				ZonesDirectory = Directory.GetCurrentDirectory() + "/Zones";

				if (Directory.Exists(ZonesDirectory)) {
					// Each (correctly formatted) zone has a folder with a top level file of the same name + ".txt"
					foreach (string ZoneDirectory in Directory.GetDirectories(ZonesDirectory)){
						// open the zone's main file
						string ZoneFile = ZoneDirectory + "/ZoneInfo.txt";
						Zones.Add(new Zone(ZoneFile, ZoneDirectory));
					}
				} else {
					Zones.Add(new Zone());
				}


			} catch (Exception e){
				Console.WriteLine (e);
				Zones.Add (new Zone ());
			}
		}
	}

}

