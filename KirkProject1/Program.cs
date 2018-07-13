using System;

namespace KirkProject1
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try
			{
				Console.WriteLine ("Creating the server...");

				Server server = Server.Instance(args.Length > 0 ? int.Parse(args[0]) : 1337);

				Console.WriteLine ("Accepting connections ... ");

				// this will block
				server.Run ();
			}
			catch (Exception err)
			{
				Console.WriteLine ("Error in server: " + err);
			}
		}
	}
}
