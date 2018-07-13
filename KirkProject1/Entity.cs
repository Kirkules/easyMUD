using System;
using System.Collections.Generic;

namespace KirkProject1
{
	public interface Entity : Targetable {
		
		void ReceiveMessage(string message);
		List<Item> Inventory { get; set;}
		int CommandPermission { get; }
	}
}

