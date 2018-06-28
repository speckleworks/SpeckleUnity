# SpeckleUnity
Proof-of-Concept integration of speckle and unity

## Disclaimer

This is a rough proof-of-concept project, not intended for actual use

Developed for Unity 2018.1.4f1

### To use:
1. Add SpeckleManager to scene
2. Set StreamID on SpeckleManager in inspector. 
3. Set server and authtoken directly in the UnityReciever script

Does not implement runtime login or stream selection.

Does not implement a sender of any kind.

Only reads mesh, point, and polyline data types.

### Notes on Hololens dev:

Hololens uses Universal Windows Platforms. Things that run in the editor while testing may not work when deploying to the Hololens. This project includes a SpeckleCoreUWP.dll, set to target WSAPlayer only while SpeckleCore.dll excludes WSAPlayer

Additionally, websocket-sharp does not work on UWP, so websocket-sharp.dll also excludes WSAPlayer. SpeckleCoreUWP.dll includes an alternative websocket implementation through Windows MessageWebSockets
