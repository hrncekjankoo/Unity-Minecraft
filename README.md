# Unity-Minecraft
Optimizations possible: 
Do not use built in Cubes in Unity, they have lots of option which are not needed and it could be really exensive if we will be using thousands of them. Implement rather own Voxels and Chunks system.- IMPLEMENTED

Draw only higher level of terrain, user still cannot ee what is under the ground. And redraw lower level only if user is digging. - IMPLEMENTED (but with small bug, sometimes it in not drawing texture on lower levels)

Do not use built in Mesh Collider and build own system for moving and checking for near Voxels. - IMPLEMENTED 
