# SpeckleUnity

In this repository you will find the source code, assets and project settings of the SpeckleUnity package for Unity app development (Unity 2019.3 or newer recommended) along with a DocFX project containing documentation assets and articles.

## NOTICE

* [Documentation can be found here](https://speckleworks.github.io/SpeckleUnity/).
* This is a very rough proof-of-concept project, not intended for actual use.
* Only tested for Windows (so far).
* Does not implement runtime login or stream selection.
* Only displays mesh, point, and polyline data types. Breps and Curves are displayed with display values.
* Currently only implements sending a transform as a Speckle Point, or sending numbers
* Does not use the Speckle Kit workflow. Any kit DLLs must be added manually to the Unity Project. 

## How To Install

See instructions in [this](https://speckleworks.github.io/SpeckleUnity/articles/howToInstall.html) part of the documentation.

---

## Roadmap

> Roadmap is subject to change. Last reviewed 14th of March 2020.

| Version | Defining Feature                  						  				    |
| ------- | --------------------------------------------------------------------------- |
| ~0.1~   | ~First prototype release~   								 			    |
| ~0.2~   | ~Restructure and release as UPM package~								    |
| ~0.3~   | ~Custom materials assigned via inspector~								    |
| ~0.4~   | ~Spawn geometry in transform heirarchy based on layer data~		    	    |
| 0.5     | User login API and stream selection UI prefabs		       				    |
| 0.6     | Local caching of Speckle streams			                                |
| 0.7     | Implement SpeckleUnitySender API    			                            |
