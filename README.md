# Red and Black Editor
A level editor for Perfect Dark on the Nintendo 64.

This uses the [decompilation](https://github.com/n64decomp/perfect_dark) by Ryan Dwyer (without it, none of this would be possible.)

## How to use
This project is still very much in-progress. You can load setup files from the decomp, and look around levels and edit setup values. Editing and importing the background/portal/collision shapes is not yet supported.

When you start up, you will be prompted to select a decomp project. Select the root folder of your decomp project. Then, click File > Import > Import Background, Import Collision & Import Setup and Pads (in that order).

Hold right click and use WASD to fly around the level. You can click to select objects, their attributes will be displayed on the right, in the object inspector. In the bottom left you can switch between pad/prop, room, portal, collision and lighting mode.

Right click in the level to add, delete, copy and paste objects.

Go to Editors > Setup settings to view settings for most of the setup, like pads, objectives, intro settings, etc.

Known issues:
- The position, rotation and size of some objects (most notable glass and autoguns) are not 100% correct; I still need help figuring that out
- The level models are mostly accurate, but some things like transparency and mirrored/clamped textures are not working yet
- Most things in the script editor (AI lists/action blocks) should work, though most actions are still unnamed and some aren't included

## Unity plugins used
- [Flexible Color Picker](https://assetstore.unity.com/packages/tools/gui/flexible-color-picker-150497)
- [Free Outline](https://assetstore.unity.com/packages/vfx/shaders/free-outline-326925)
- [Runtime File Browser](https://assetstore.unity.com/packages/tools/gui/runtime-file-browser-113006)

## Resources
Everything that still needs to be done is listed [here](https://trello.com/b/YSko025P/pd-editor).

