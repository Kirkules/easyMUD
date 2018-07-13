using System;
using System.Collections.Generic;

namespace KirkProject1
{
	public interface Targetable
	{
		string RoomLook { get; set; }
		string LookAt { get; set; }
		int ZoneRoomNumber { get; set; }
		string Name { get; set; }
		Zone Zone { get; set; }
		List<string> Shortcuts { get; }

		Room Location { get; set;}
	}
}

