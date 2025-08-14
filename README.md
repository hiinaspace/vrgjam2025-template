# VRG Game Jam 2025 Template

This template provides everything needed to create a multiplayer VR minigame for the [/vrg/ Game Jam 2025](https://jam.vrg.party). The template includes the MUGI framework for multiplayer game logic and a submission helper tool for packaging your game.

For complete documentation, troubleshooting, and jam details, visit [jam.vrg.party](https://jam.vrg.party).

## Tutorial

In this tutorial, you'll make a simple game where all players click a capsule to make their score go up. The game will be fully usable as a prefab in any VRChat world, including as a demo world you can upload to your own VRChat account.

### Prerequisites

This tutorial assumes you have [VCC (VRChat Creator Companion)](https://vcc.docs.vrchat.com/) and Unity setup already, and basic familiarity with both. If you don't, do VRChat's own [Creating Your First World](https://creators.vrchat.com/worlds/creating-your-first-world) tutorial first.

### Step 1. Setup

#### Download Template

[Download this template as a ZIP](https://github.com/hiinaspace/vrgjam2025-template/archive/refs/heads/main.zip) from GitHub and extract it somewhere on your computer.

#### Add VPM Repository

Before opening in VCC, you need to add the VRG Game Jam package repository:

1. Open your web browser and go to https://hiinaspace.github.io/vrgjam2025-vpm-repo/
2. Click the "Add to VCC" button
3. This adds the MUGI framework and submission helper packages to your VCC

#### Open Project

1. Open VCC (VRChat Creator Companion)
2. Add the extracted template folder as a project
3. In VCC, click the 'Manage Project' Button.
4. Observe that the `Mugi (Mini Udon Game Interface)` and `/vrg/ Game Jam 2025 Submission Helper` are in the list under `Manage Packages`. (You might get a prompt to download these if they're not there.)
5. Open the project in Unity using the button in VCC.
6. Wait warmly for the Domain to Complete

Note: if you look in the Console of Unity and see a bunch of errors, don't despair. The errors are usually spurious. Press the "Clear" button in the upper-left of the console to hopefully make 'em go away.

### Step 2: Rename Your Package

1. in Unity's browser, navigate to `Packages/!/vrg/ Template Package'`.
2. Right click the folder and "Show in Explorer"
3. In Explorer, notice that the folder is called `com.example.jamgame` instead.
4. Right-click the folder and rename it to `com.example.awesomegame`. This way we don't conflict with other packages using the same template.
5. Open the `package.json` file inside this folder.
6. Update the package metadata:
   ```json
   {
       "name": "com.example.awesomegame",
       "displayName": "Awesome VR Game",
       "description": "A thrilling capsule-clicking experience",
       "author": {
           "name": "Your Name",
           "email": "your.email@example.com"
       }
   }
   ```
7. Back in Unity's Project browser, notice that your package now shows up as `Packages/Awesome VR Game`. The `displayName` field from the metadata is what unity displays instad, while Explorer shows the `com.exmaple.awesomegame`.

### Step 3: Create a new MugiGame-based  Prefab

1. In the Project window, navigate to `Packages/Awesome VR Game/Runtime`
2. Right Click in the window and click `Create -> Mugi -> Mugi Game Variant`.
3. Save the new prefab as `MyAwesomeGame.prefab`.
2. Double-click `MyAwesomeGame.prefab` to open it in prefab editing mode
3. This prefab contains the MugiGame framework components at the root.

### Step 4: Add a Capsule

1. In the prefab hierarchy, right-click and create `3D Object > Capsule`
2. Name it "ScoreButton"
3. Position it at the center of the scene (0, 0, 0)

### Step 5: Create the CapsuleClicker Script

Now we'll add some behavior.

1. In the Project window, navigate to `Packages/Awesome VR Game/Runtime`
2. Right-click and create `Create > U# Script`
3. Name it `CapsuleClicker`
4. Open the script. The default script looks like:

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CapsuleClicker : UdonSharpBehaviour
{
    void Start()
    {
        
    }
}
```

#### Alternate Creation Method

When you do `Create > U# Script`, if you get an error saying "Couldn't create Asset", try this:

1. In the Project window, navigate to `Assets/` (not the Package)
2. Right-click and create `Create > U# Script`
3. Name it `CapsuleClicker`.
4. Move both the new `CapsuleClicker.cs` and `CapsuleClicker.asset` into your `Packages/Awesome VR Game/Runtime` directory.

I dunno why this occurs sometimes. 

### Step 6: Implementing the script

First, let's add a namespace to the script, so it won't conflict with other packages:

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Com.Example.AwesomeGame // <--- New!
{
    public class CapsuleClicker : UdonSharpBehaviour
    {
        void Start()
        {
            
        }
    }
}
```

Then we'll add a reference to the MugiGame behavior itself:

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Space.Hiina.Mugi; // <-- New

namespace Com.Example.AwesomeGame
{
    public class CapsuleClicker : UdonSharpBehaviour
    {
        public MugiGame mugiGame; // <-- New

        void Start()
        {
            
        }
    }
}
```

Then finally we'll implement the Interact method, which gets called when you click on a thing in Udon:

```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Com.Example.AwesomeGame
{
    public class CapsuleClicker : UdonSharpBehaviour
    {
        public MugiGame mugiGame;

        void Start()
        {
            
        }

        // New method:
        public override void Interact()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (mugiGame != null && localPlayer != null)
            {
                if (mugiGame.gameState == MugiGame.STATE_RUNNING && 
                    mugiGame.IsPlayerInGame(localPlayer))
                {
                    mugiGame.IncrementScore(localPlayer.playerId, 1);
                }
            }
        }
    }
}
```

This script makes it so when you click the capsule and you're in the game, your score goes up. Don't worry too much about the details for now.

### Step 7: Attach the Script and Reference

1. Select the "ScoreButton" capsule in the prefab hierarchy
2. Click "Add Component" and add your `CapsuleClicker` script
3. In the script component, drag the root `MyAwesomeGame` object into the "Mugi Game" field.

### Step 8: Configure Game Settings

1. Select the root `MyAwesomeGame` object in the prefab hierarchy
2. In the `MugiGame` component, configure:
   - **Game Time Limit**: 30 seconds (default 300)
   - **Min Players**: 1 (default 2)

We change these so when we're testing, we don't have to figure out multiplayer or wait 5 minutes for the game to end.

### Step 9: Add your prefab to the Test Scene

1. Navigate to `Packages/My Awesome Game/Samples`
2. Open the `GameTestWorld.unity` scene.
3. Navigate to the `Packages/My Awesome Game/Runtime` in the browser.
4. Drag your `MyAwesomeGame` prefab into the scene.

You should see the scoreboard and your capsule visible now. Move it to (0,0,0) if it's not.

### Step 10: Test Your Game in Play Mode

3. Enter Play Mode in Unity
4. Your capsule should be visible in the scene
5. Join the game using the lobby UI.
6. Start the game using the lobby UI.
7. Click the capsule to increment your score
8. Once the game ends, see your final score on the scoreboard.

### Step 11: Submit Your Game

1. Open the VRG Game Jam Submission Helper: `Menu > vrg Game Jam 2025 > Submission Helper`
2. Your package should appear automatically in the dropdown
3. Click "Refresh Validation" to check for issues. There may be some. You can ignore those for now (WIP)
4. Accept the submission agreement.
5. Click "Export as ZIP" to create your submission file.

### Step 12: Upload and Test in VRChat (Optional)

1. Open the VRChat SDK window from VRChat SDK -> Show Control Panel.
2. Name the world "Awesome game test"
3. Click "Capture in Scene" to make a thumbnail image.
4. Click the "Build & Publish" button.
5. Test the world with your friends (and/or alt accounts)

You now have a working game, a demo world, and a file other people can use to add your game to their world!

## Next Steps

Visit [jam.vrg.party](https://jam.vrg.party) for complete documentation, troubleshooting guides, and community support.