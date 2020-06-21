Having read comments and questions, we thought it would be fun to talk about the things we feel like we would change, and the things we're happy with.

Obviously much of the code could simply be cleaner had we known exactly what everything was going to do from the beginning, or had time to do a major refactor (which wasn't ever a big priority).

### Animation code
We would have liked all Animation related code to be its own system that was more data driven. It's the way it is because we never implemented more than a simple frame-by-frame sprite component. We completely agree that having `if frame == whatever` inside the player class is ugly.

### States that shouldn't be states
There are a large number of states that simply shouldn't exist within the player. Everything regarding the `Intro` states and the `Dummy` state should be an entirely different entity used for cutscenes that get swapped with the real player during gameplay.

This also goes for the `ChaserState`. This could likely be abstracted into a Component and removed entirely from the player class.

### One big file vs. A bunch of files
We wouldn't have moved states into their own classes. To us, due to how much interaction there is between states and the nuance in how the player moves, this would turn into a giant messy web of references between classes. If we were to make a tactics game then yes - a more modular system makes sense.

One reason we like having one big file with some huge methods is because we like to keep the code sequential for maintainability. If the player behavior was split across several files needlessly, or methods like Update() were split up into many smaller methods needlessly, this will often just make it harder to parse the order of operations. In a platformer like Celeste, the player behavior code needs to be very tightly ordered and tuned, and this style of code was a conscious choice to fit the project and team.

### How do you Unit Test this class?
We don't. We wrote unit tests for various other parts of the game (ex. making sure the dialog files across languages all match, trigger the correct events, and that their font files have all the appropriate characters). Writing unit tests for the player in an action game with highly nuanced movement that is constantly being refined feels pointless, and would cause more trouble than they're worth. Unit Tests are great tools, but like every tool they have advantages and disadvantages. Unit tests could be useful for making sure input is still triggering, collision functionality behaves as expected, and so on - but none of that should exist in the Player class.

### Is there an Entity / Component structure?
We do use a Scene->Entity->Component system, which may not entirely be clear from the player class alone. She inherits "Actor" which inherits "Entity". Actor has generic code for movement and collisions. Anything that was re-used was put into a component (player sprite, player hair, state machine, mirror reflections, and so on). Things that the player only ever did were left in the player.

### Why no array in that `ChaserState` struct at the bottom?
The reason that "ChaserState" struct has a switch statement instead of an array is to save on creating garbage. There's no way to have an array in a struct in C# with a predefined size, (ex. `int[4] fourValues;`) so every struct would be creating a new array instance. In the end this probably didn't matter, but we were trying to save on creating garbage during levels as we weren't sure how the GC would perform cross-platform.

### Isn't XNA Deprecated?
Yes, it is! We use XNA because we're comfortable in it, like C#, it's very stable on Windows, and is easy to make cross platforms with open source ports such as FNA and MonoGame. If you're playing on macOS or Linux you're on FNA, and on consoles you're on MonoGame. Will we use it for future projects? Maybe, maybe not.

### Do you have any tutorials on how the basic physics of Celeste work?
Not right now, but Matt wrote this overview of the TowerFall physics a few years back: https://medium.com/@MattThorson/celeste-and-towerfall-physics-d24bd2ae0fc5

Celeste's physics system is very similar to TowerFall's.

### How big was the programming team?
We had 2 programmers working on Celeste (Noel and Matt).

