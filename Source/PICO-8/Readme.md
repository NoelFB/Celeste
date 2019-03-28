This is Celeste Classic written in C# that runs in the new full game. 
 - The actual C# port is located in `Classic.cs` and tries to be as close to 1-1 with the original LUA code as it can.
 - The `Emulator.cs` runs the game inside of a Celeste Scene. It references various Celeste & XNA APIs.
 - The Emulator references some art assets which exist in the Graphics folder. Note their paths wont match since in the full game these are stored elsewhere.
 - The PICO-8 Name and Logo is owned by Lexaloffle Games LLP

You can play Celeste Classic and view its LUA code here:
https://www.lexaloffle.com/bbs/?tid=2145