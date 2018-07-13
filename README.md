# An easily expandable telnet MUD server
*Kirk A. Boyer*

This is a project I made in a Networking for Games class at the University of Denver, taught by Chris GauthierDickey in 2016.

It's a MUD (Multi-User Domain [game]) server. I designed it so that the content (game world, characters, their behavior, etc.) would be easily expandable without adding more code. The (text) data files containing this information are parsed and converted into structures the server can use.

When running in debug mode from Monodevelop, everything should work fine.

The MUD server will search for a Zones folder with info about the rooms in the current directory containing the process executable. 

In debug mode, this is just /bin/Debug, so I put a copy of the Zones folder there.
There is a copy of the Zones folder in the project folder if you compile the project instead; just move both Zones/ and the file CommandPermissions.txt into the folder containing the compiled binary.

The MUD still boots up if it can't find any zones, but it's bare-bones and doesn't have any MOBs or any interesting stuff to do if it can't find the Zones folder and the command permissions file.
