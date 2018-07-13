using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KirkProject1
{
	public class Commands
	{
		private Dictionary<string, int> RequiredPermissionLevels;
		// Permission levels: 
		// 0 = anyone/any mob can do it
		// 1 = only players can do it
		// 2 = only admins can do it

		public delegate void CommandFunction(Entity user, string[] parameters);
		private Dictionary<string, CommandFunction> mCommandFunctions;



		public Commands(string permissionsFilename){
			// command permissions file structure:
			// --------------------------------------------------------
			// multiple lines of "C L", where C is the single-word command name and L is the permission level
			RequiredPermissionLevels = new Dictionary<string, int>();
			mCommandFunctions = new Dictionary<string, CommandFunction> ();
			try{
				string[] fileContents = System.IO.File.ReadAllLines(permissionsFilename);

				// fill commands permission levels with... their permission levels
				for (int i = 0; i < fileContents.Length; i++){
					string[] commandAndPermission = fileContents[i].Split(' ');
					RequiredPermissionLevels.Add(commandAndPermission[0], int.Parse(commandAndPermission[1]));
				}


				mCommandFunctions.Add("quit", QuitCommand);
				mCommandFunctions.Add("emote", EmoteCommand);
				mCommandFunctions.Add("look", LookCommand);

				mCommandFunctions.Add("east", MoveDirectionCommand("east"));
				mCommandFunctions.Add("e", MoveDirectionCommand("east"));
				mCommandFunctions.Add("north", MoveDirectionCommand("north"));
				mCommandFunctions.Add("n", MoveDirectionCommand("north"));
				mCommandFunctions.Add("south", MoveDirectionCommand("south"));
				mCommandFunctions.Add("s", MoveDirectionCommand("south"));
				mCommandFunctions.Add("west", MoveDirectionCommand("west"));
				mCommandFunctions.Add("w", MoveDirectionCommand("west"));
				mCommandFunctions.Add("up", MoveDirectionCommand("up"));
				mCommandFunctions.Add("u", MoveDirectionCommand("up"));
				mCommandFunctions.Add("down", MoveDirectionCommand("down"));
				mCommandFunctions.Add("d", MoveDirectionCommand("down"));
				mCommandFunctions.Add("northeast", MoveDirectionCommand("northeast"));
				mCommandFunctions.Add("ne", MoveDirectionCommand("northeast"));
				mCommandFunctions.Add("southeast", MoveDirectionCommand("southeast"));
				mCommandFunctions.Add("se", MoveDirectionCommand("southeast"));
				mCommandFunctions.Add("northwest", MoveDirectionCommand("northwest"));
				mCommandFunctions.Add("nw", MoveDirectionCommand("northwest"));
				mCommandFunctions.Add("southwest", MoveDirectionCommand("southwest"));
				mCommandFunctions.Add("sw", MoveDirectionCommand("southwest"));

				mCommandFunctions.Add("uptime", UptimeCommand);
				mCommandFunctions.Add("time", UptimeCommand);
				mCommandFunctions.Add("say", SayCommand);
				mCommandFunctions.Add("wander", WanderCommand);
				mCommandFunctions.Add("exits", ExitsCommand);

				// default permission is 2 (must be admin to do it)
				foreach (string command in mCommandFunctions.Keys){
					if (!RequiredPermissionLevels.ContainsKey(command)){
						RequiredPermissionLevels.Add(command, 2);
					}
				}
				// for each command with a permission level that hasn't been implemented, fill with not-implemented


			} catch (Exception e) {
				Console.WriteLine (e);
				DefaultCommands ();
			}
		}

		public void HandleCommand(Entity user, string commandString){
			if (commandString.Equals("")) {
				return;
			}
			// break command up into usable parts
			string[] words = commandString.Split(' ');
			string commandWord = words [0];

			// find the closest available command to what the user typed as the command word
			List<string> possibleCommands = new List<string>();
			// add all commands the user has the right permission level to access that are also implemented
			foreach (string possibleCommand in RequiredPermissionLevels.Keys) {
				if (RequiredPermissionLevels[possibleCommand] <= user.CommandPermission &&
					mCommandFunctions.ContainsKey (possibleCommand)) {
					possibleCommands.Add (possibleCommand);
				}
			}
			string actualCommand = GetBestMatch (possibleCommands, commandWord);
			string[] parameters = new string[words.Length - 1]; 
			Array.Copy (words, 1, parameters, 0, parameters.Length);

			if (actualCommand != null) {
				mCommandFunctions [actualCommand](user, parameters);
			} else {
				user.ReceiveMessage ("Do what, again?\n");
			}
		}

		public void ExitsCommand(Entity user, string[] parameters){
			string message = "[ ";
			foreach (int direction in user.Location.Exits.Keys.ToArray()) {
				message += Direction.GetShortString (direction) + " ";
			}
			message += "]";
			user.ReceiveMessage (message);
		}

		public void SayCommand(Entity user, string[] parameters){
			string message = string.Join (" ", parameters);
			foreach (Entity entity in user.Location.Entities) {
				if (entity.Equals (user)) {
					entity.ReceiveMessage ("You say, '" + message + "'");
				} else {
					entity.ReceiveMessage (user.Name + " says, '" + message + "'");
				}
			}
		}

		public void UptimeCommand(Entity user, string[] parameters){
			long elapsedMilliseconds = (long)Server.Instance ().MUDTimer.ElapsedMilliseconds;
			long elapsedGameHours = elapsedMilliseconds / (20*1000);
			long elapsedGameMinutes = (elapsedMilliseconds / (333)) % 60;
			user.ReceiveMessage ("The MUD has been up for " + Server.Instance ().DaysPassed.ToString () + " days, " +
			elapsedGameHours.ToString () + " hours, and " + elapsedGameMinutes.ToString () + " minutes.\n");

		}

		public void QuitCommand(Entity user, string[] parameters){
			if (user is Player) {
				// players can quit for reals
				Player player = (Player)user;
				player.Connection.Disconnect ();
				player.Location.Entities.Remove (player);
				Server.Instance ().RemovePlayer ((Player)user);
			} else if (user is Mob) {
				// mobs emote "just gives up" when they try the "quit" command.
				EmoteCommand(user, "just gives up.".Split(' '));
			}
			
		}

		public CommandFunction MoveDirectionCommand(string direction){
			return MoveDirectionCommand (Direction.fromString (direction));
		}

		public void WanderCommand(Entity user, string[] parameters){
			int direction = user.Location.Exits.Keys.ToArray()[new Random ().Next (user.Location.Exits.Keys.Count)];
			user.ReceiveMessage("You wander off to the " + Direction.GetString(direction));
			(MoveDirectionCommand (direction)) (user, parameters);
		}

		public CommandFunction MoveDirectionCommand(int direction){
			if (0 <= direction && direction <= 9) {
				return delegate(Entity user, string[] parameters) {
					// don't actually need parameters for move-in-direction commands
					if (user.Location.Exits.ContainsKey (direction)) {
						Room destination = user.Zone.Rooms [user.Location.Exits [direction]];
						Room justLeft = user.Location;
						destination.Broadcast (user.Name + " has arrived.\n");
						user.Location.Entities.Remove(user);
						user.Location = destination;
						justLeft.Broadcast(user.Name + " leaves to the " + Direction.GetString(direction) + ".\n");
						user.Location.Entities.Add(user);
						LookCommand(user, new string[] {});
					} else {
						user.ReceiveMessage("You can't go that direction...\n");
					}
				};
			} else {
				throw new Exception(direction.ToString() + " is not an allowed direction for a move command.\n");
			}

		}

		public void EmoteCommand(Entity user, string[] parameters){
			// anybody can do an emote!
			string emoteMessage = String.Join(" ", parameters);
			user.Location.Broadcast (user.Name + " " + emoteMessage);
		}

		public void LookCommand(Entity user, string[] parameters){
			// Mobs don't get to look at stuff
			if (user is Mob) {
				return;
			}

			// if there are no parameters, user is just looking in the room
			if (parameters.Length == 0) {
				RoomLook (user, parameters);
			} else if (user is Player){
				// user is trying to look at something. If they said "at", see what they want to look at
				Console.WriteLine(parameters.Length);
				string targetString;
				if (parameters.Length > 1) {
					if (parameters [1].Equals("at")) {
						targetString = parameters [2].ToLower();
					} else {
						targetString = parameters [1].ToLower();
					}
				} else {
					targetString = parameters [0].ToLower();
				}
				Room userRoom = user.Zone.Rooms [user.ZoneRoomNumber];

				// find all possible target matches and a measure of how well they match
				// (measure is just the shortcut that matches; these will be ordered alphabetically)
				List<Targetable> possibleTargets = new List<Targetable>();
				possibleTargets.AddRange (userRoom.Entities);
				possibleTargets.AddRange (userRoom.Items);
				possibleTargets.AddRange (((Player)user).Inventory);

				Targetable bestTarget = GetBestTarget (possibleTargets, targetString);
				if (bestTarget != null) {
					user.ReceiveMessage (bestTarget.LookAt + "\n");
				} else {
					user.ReceiveMessage ("That doesn't seem to be here...\n");
				}
			}
		}

		public Targetable GetBestTarget(List<Targetable> targets, string targetString){
			// find all possible target matches and a measure of how well they match
			// (measure is just the shortcut that matches; these will be ordered alphabetically)
			List<Tuple<Targetable, string>> matches = new List<Tuple<Targetable, string>>();

			foreach (Targetable target in targets) {
				foreach (string shortcut in target.Shortcuts) {
					if (shortcut.StartsWith (targetString, StringComparison.CurrentCulture)) {
						matches.Add(new Tuple<Targetable, string>(target, shortcut));
						break;
					}
				}
			}

			if (matches.Count > 0) {
				// sort by the shortcuts and take the alphabetically first one
				matches.Sort ((Tuple<Targetable, string> tup1, Tuple<Targetable, string> tup2) => tup1.Item2.CompareTo (tup2.Item2));
				return matches [0].Item1;
			}

			return null;

		}

		public string GetBestMatch(List<string> possibleMatches, string targetString){
			List<string> matches = new List<string> ();
			foreach (string possibleMatch in possibleMatches) {
				if (possibleMatch.StartsWith (targetString, StringComparison.CurrentCulture)) {
					matches.Add (possibleMatch);
				}
			}

			if (matches.Count > 0) {
				// sort by the shortcuts and take the alphabetically first one
				matches.Sort ((string s1, string s2) => s1.CompareTo (s2));
				return matches [0];
			}
			return null;

		}

		public void RoomLook(Entity user, string[] parameters){
			// TODO: make this use colors
			if (user is Player) {
				// build up look string
				string message = "";
				Room userRoom = user.Zone.Rooms [user.ZoneRoomNumber];
				message += "  " + userRoom.Name + "\n";
				message += userRoom.Look + "\n";
				foreach (Item item in userRoom.Items) {
					message += "  " + item.RoomLook + "\n";
				}
				foreach (Entity entity in userRoom.Entities) {
					if (!entity.Equals (user)) {
						message += " " + entity.RoomLook + "\n";
					}
				}
				user.ReceiveMessage (message);
			} else if (user is Mob) {
				// don't need to do anything!
			}
		}

		private void DefaultCommands(){
			// If command permission loading effs up, the only thing you're allowed to do is quit.
			RequiredPermissionLevels.Add ("quit", 0);
		}
	}
}

/* Commands to make: *command has been made, otherwise not
access     activate   afk        aggressive 
aid        aim        alert      alias      allowheal  ambush     analyze    
answer     appraise   approach   aqualish   areas      arm        ask        
assault    assay      assist     auctalk    auction    autoassist autocast   
autocreditsautoexit   autofire   autoloot   autoprof   autospend  autosplit  
averagecheckbackstab   balance    bandage    barrage    barricade  bash       
beguile    berserk    bet        bid        bite       blacksun   blast      
blaze      blend      blitz      blockade   blow       bmeditate  board      
bodyguard  bombard    bonus      boost      bothan     brainwash  breakaway  
breakcamp  breakdown  brew       bribe      brief      bug        build      
burstfire  buy        calm       camouflage camp       capture    carve      
cash       cattack    charge     chat       cheat      check      claim      
clan       clancom    clawdite   clear      climb      cloak      close      
cls        collect    color      combine    comheal    comlink    commands   
compact    concentratecondition  consider   construct  consume    cook       
cordon     cover      coverup    create     credits    crew       cure      
cut        dance      date       deactivate deal       decoy      defend     
defensive  delete     depart     deposit    destruct   detain     diagnose   
disarm     disband    disembark  disguise   disintegratedismount   display    
dissect    divert     dock       donate     down       drag       drain      
draw       drawfire   drink      drive      drop       duel       dump       
east       eat        ecm        eject      ejectround *emote      empire     
empty      engage     enhance    enlist     enslave    enter      entrench   
equipment  euthanize  evacuate   evade      evaluate   event      ewok       
examine    exercise   exits      experience exude      faction    fadvance   
fakename   falleen    farm       feed       field      fill       find       
fire       fixme      flak       flee       flip       flurry     fly        
focus      follow     forge      free       frenzy     friends    fundamentals
fwd        fwithdrawalgain       gamble     gamorrean  gcharge    geonosian  
get        gift       give       glance     gore       grab       grapple    
grats      gretreat   grind      group      growl      gsay       gtell      
guard      gungan     hack       hail       hailfire   hand       harvest    
harvest    headbutt   heal       help       hire       hit        hj         
hold       holler     hotjump    hover      hulldown   huttese    ic         
idea       ignore     imbue      impale     impnet     info       infra      
inject     inspect    install    install    intensify  interface  into       
inventitem inventory  invert     invis      ionize     ithorian   jam        
jawa       jedi       jedinet    jerk       join       jump       junk       
kb         kick       kill       know       land       launch     legend     
levels     light      link       list       listen     loadround  locate     
lock       lognav     *look       loop       lowblow    mail       make       
manufacturemark       masound    mat        materials  maul       mblacksun_power
mbuzzdroid mdamage    mdestroy   mdisarm    mdoor      mecho      mechoaround
medevac    meditate   mend       mexp       mfaction   mfeed_news mflip      
mfnet      mforce     mforce_balancemforget    mgold      mgoto      mhunt      
mind       mjunk      mkill      mlandfightermload      moncalamarimonitor    
morale     motd       mount      move       mprotect   mpurge     mquest     
mrebellion mremember  mrestore   msend      mspynet    mstun      mtechno_status
mteleport  mtransform multiplay  murder     nab_me     nav        neutrality 
news       noauction  noclancom  nocomlink  noghri     nograts    noic       
noimp      nojedi     nolegend   nopolnet   noquestion norebel    norepeat   
normal     north      noshout    nospam     nosummon   nosun      notech     
notell     observe    offensive  offer      ofire      open       order      
out        outpost    overlap    overrun    pack       pages      palm       
partscheck patch      pcomlink   peek       pick       pilot      pinpoint   
pirate     pkill      place      play       plevels    poison     polnet     
pop        post       pour       power      pp         practice   press      
private    process    promote    prompt     protect    protective pskills    
pulse      pummel     punch      purchase   push       put        qlog       
qsay       quaff      ques       quest      question   *quit       race       
rage       raisefunds rally      ram        rambike    rampage    random     
read       rearm      rebel      rebnet     rebuild    recall     reconstruct
recover    recover    recycle    redeem     redirect   reequip    refill     
refocus    reinforce  release    reload     removal    remove     rent       
repair     repair     replenish  reply      report     reroute    rescue     
rest       restock    restring   resupply   retire     retreat    return     
ride       riot       rodian     rotate     roundhouse rp         rules      
safemode   salvage    salvo      sample     sanitize   sarlacc    save       
say        scale      scan       scavenge   score      scout      sdisplay   
search     selkath    sell       sensor sweepserve      service    setsize    
setup      sew        shadow     shift      shock      shopstat   shout      
showcolor  shuttle    sideswipe  sinventitemsip        sit        slam       
slash      slavelist  slay       sleep      slice      slip       smelt      
smuggler   snapshot   snare      sneak      sniff      snipe      socials    
south      speech     speed      spinkick   split      spot       spreadfire 
sprompt    spy        squirt     sremove    stab       stalk      stand      
standard   stationary status     steady     steal      stomp      stop       
strike     stun       suck       suicide    sullustan  sunload    sunmail    
sunnet     suppress   surgical   survey     swallow    swarm      sweep      
swimpy     tactics    tail       take       talk       tame       target     
taste      taxi       technet    techno     tell       teraskasi  terminal   
test       third      throw      thrust     time       tinker     tips       
title      todo       toggle     trace      tracer     track      tractor    
trade      train      trandoshan transform  transfusiontranslist  transport  
trap       treat      trick      trip       tripwire   tshot      tumble     
tune       turn       twilek     typo       ungroup    uninstall  unload     
unlock     up         update     upgrade    upme       uptime     use        
utilize    value      vdisplay   veermok    version    view       visible    
vision     vlevel     vote       vprompt    vskills    wake       walk       
wampa      ward       wash       watch      wd         weaponremovewear       
weather    west       where      whip       whirlwind  whisper    who        
whoami     wield      wimpy      wingman    withdraw   withdrawal wizlist    
wookiee    woundexp   woundheal  wounds     write      yank       yell   
 * */
