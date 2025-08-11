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
3. Open the project in Unity.
4. Wait warmly for the Domain to Complete

### Step 2: Rename Your Package

1. In the Unity Project window, navigate to `Packages/com.example.jamgame`
2. Right-click the folder and rename it to `com.example.awesomegame`
3. Open the `package.json` file inside this folder
4. Update the package metadata:
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

In a real game you'll use real, unique info, but this is fine for the tutorial.

### Step 3: Open the Template Prefab

1. In the Project window, navigate to `Packages/com.example.awesomegame/Runtime`
2. Double-click `TemplateGame.prefab` to open it in prefab editing mode
3. This prefab contains the MugiGame framework components at the root

### Step 4: Add a Capsule

1. In the prefab hierarchy, right-click and create `3D Object > Capsule`
2. Name it "ScoreButton"
3. Position it at the center of the scene (0, 0, 0)

### Step 5: Create the CapsuleClicker Script
1. In the Project window, navigate to `Packages/com.example.awesomegame/Runtime`
2. Right-click and create `Create > U# Script`
3. Name it `CapsuleClicker`
4. Open the script and replace its contents with:

```csharp
using Space.Hiina.Mugi;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Com.Example.AwesomeGame
{
    public class CapsuleClicker : UdonSharpBehaviour
    {
        public MugiGame mugiGame;

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

### Step 6: Attach the Script

1. Select the "ScoreButton" capsule in the prefab hierarchy
2. Click "Add Component" and add your `CapsuleClicker` script
3. In the script component, drag the root `TemplateGame` object into the "Mugi Game" field

### Step 7: Configure Game Settings

1. Select the root `TemplateGame` object in the prefab hierarchy
2. In the `MugiGame` component, configure:
   - **Game Time Limit**: 30 (seconds)
   - **Min Players**: 1
   - **Max Players**: 8
   - **Use Teams**: false (unchecked)

### Step 8: Test Your Game in Play Mode

1. Navigate to `Packages/com.example.awesomegame/Samples`
2. Open the `GameTestWorld.unity` scene
3. Enter Play Mode in Unity
4. Your capsule should be visible in the scene
5. Join the game using the lobby UI.
6. Start the game using the lobby UI.
7. Click the capsule to increment your score
8. Once the game ends, see your final score on the scoreboard.

### Step 9: Upload and Test in VRChat

1. Open the VRChat SDK window from VRChat SDK -> Show Control Panel.
2. Name the world "Awesome game test"
3. Click "Capture in Scene" to make a thumbnail image.
4. Click the "Build & Publish" button.
5. Test the world with your friends (or alt accounts)

### Step 10: Submit Your Game

1. Open the VRG Game Jam Submission Helper: `Menu > /vrg Game Jam 2025 > Submission Helper`
2. Your package should appear automatically in the dropdown
3. Click "Refresh Validation" to check for issues
4. Accept the submission agreement
5. Click "Export as ZIP" to create your submission file.

You now have a working game, a demo world, and a file other people can use to add your game to their world!

## Support

Visit [jam.vrg.party](https://jam.vrg.party) for complete documentation, troubleshooting guides, and community support.