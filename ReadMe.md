# Tweach

## What is it?

Tweach is a hierarchy viewer and inspector for viewing and making changes to components
and scripts variables at runtime. The UI is optimized for touch controls, making it a
great companion when developing and tweaking mobile games. No more waiting for a build,
realizing you left an Int at 4 that's supposed to be a 5, and promptly rebuilding the
whole thing. Just open up Tweach and change it on the fly.

## How do I install it?
Download the unitypackage in the root folder and import into your project by clicking
Assets > Import Package > Custom Package... 

## How do I open Tweach at runtime?
Just instantiate the prefab Tweach.prefab in the main folder into your scene and the
scripts will do the rest. Dont forget to pause your game. The script TweachActivator
in the base tweach folder contains an example.

## I keep getting recursion errors!
This mostly seems to happen when mapping private properties. If you absolutely need to
do this, try using the Tweach-attribute on the members you need to map.

## I keep getting angry error messages!
I've noticed these popping up, especially when Tweach tries to access meshes that aren't
read enabled. The sollution is once again in the settings. Try disabling mapping of
private properties.

## How do I change the settings?
The settings are standard bools on the TweachMain-script in the root of the Tweach prefab.
Either change them before hand or get the component and set them from the activation script.

## What does Tweach mean?
Touch Tweak ^^
