using System;

namespace KirkProject1
{
	public class Direction
	{
		public const int North = 0;
		public const int East = 1;
		public const int West = 2;
		public const int South = 3;
		public const int Up = 4;
		public const int Down = 5;
		public const int Southeast = 6;
		public const int Southwest = 7;
		public const int Northeast = 8;
		public const int Northwest = 9;
		public const int None = -1;

		public static int fromString(string s){
			switch (s.ToLower()) {
			case "north":
				return North;
			case "east":
				return East;
			case "south":
				return South;
			case "west":
				return West;
			case "up":
				return Up;
			case "down":
				return Down;
			case "southeast":
				return Southeast;
			case "southwest":
				return Southwest;
			case "northeast":
				return Northeast;
			case "northwest":
				return Northwest;
			default:
				return None;
			}
		}

		public static string GetString(int direction){
			switch (direction) {
			case North:
				return "north";
			case East:
				return "east";
			case South:
				return "south";
			case West:
				return "west";
			case Up:
				return "up";
			case Down:
				return "down";
			case Southeast:
				return "southeast";
			case Southwest:
				return "southwest";
			case Northeast:
				return "northeast";
			case Northwest:
				return "northwest";
			default:
				return "";
			}
		}

		public static string GetShortString(int direction){
			switch (direction) {
			case North:
				return "n";
			case East:
				return "e";
			case South:
				return "s";
			case West:
				return "w";
			case Up:
				return "u";
			case Down:
				return "d";
			case Southeast:
				return "se";
			case Southwest:
				return "sw";
			case Northeast:
				return "ne";
			case Northwest:
				return "nw";
			default:
				return "";
			}
		}
	}
}

