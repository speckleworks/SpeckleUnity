# SpeckleUnity
Proof-of-Concept integration of speckle and unity

## Disclaimer
Updated July 2019

This is a very rough proof-of-concept project, not intended for actual use

Developed for Unity 2019.1.xx for Windows (not yet tested for mobile or other platforms)

### To use:
1. Drag UnitySpeckle folder into a new project
2. Set authtoken directly in the SpeckleUnityClient.cs script
3. Add SpeckleManager from Prefabs folder to scene
4. Set Server URL on SpeckleManager in inspector. 
5. Add Sender and/or Receiver prefabs to the scene
6. For any receiver prefabs, enter the Stream Id
7. Add a specific SendComponent to game objects that have data you want to send (see the example scene)



### Notes 

Does not implement runtime login or stream selection.

Only displays mesh, point, and polyline data types. Breps and Curves are displayed with display values.

Currently only implements sending a transform as a Speckle Point, or sending numbers

Does not use the Speckle Kit workflow. Any kit DLLs must be added manually to the Unity Project. 