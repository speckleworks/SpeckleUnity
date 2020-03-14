# SpeckleUnity

In this repository you will find the source code, assets and project settings of the SpeckleUnity package for Unity app development (Unity 2019.3 or newer recommended).

Developed in Unity 2019.3.x for Windows (not yet supported for mobile or other platforms)

## NOTICE

* This is a very rough proof-of-concept project, not intended for actual use.
* Only tested for Windows (so far).
* Does not implement runtime login or stream selection.
* Only displays mesh, point, and polyline data types. Breps and Curves are displayed with display values.
* Currently only implements sending a transform as a Speckle Point, or sending numbers
* Does not use the Speckle Kit workflow. Any kit DLLs must be added manually to the Unity Project. 

## How To Install

In order to install SpeckleUnity in your project, add the following to the top of your manifest.json file right before the dependencies object. Next, save the file and restart the editor. You should then find the package in the package manager UI which can be installed like any other Unity package. You can even view a list of all previous versions of the framework that are available in our registry.

``` js
"scopedRegistries": [
    {
        "name": "Open Source Packages",
        "url": "http://35.227.114.200:8080",
        "scopes": [
            "com.open"
        ]
    }
],
```

The manifest.json file can be found under:

"YOUR-PROJECT-FOLDER" > Packages > manifest.json

This needs to be done once for each Unity project you intend to use SpeckleUnity with because new Unity projects are always created with the default manifest.json file for that editor version.

If you can't see the latest version of SpeckleUnity in the package manager UI, it's likely because it supports only a later version of Unity than you are currently using.

---

## Roadmap

> Roadmap is subject to change. Last reviewed 9th of March 2020.

| Version | Defining Feature                  						  				    |
| ------- | --------------------------------------------------------------------------- |
| ~0.1~   | ~First prototype release~   								 			    |
| ~0.2~   | ~Restructure and release as UPM package~								    |
| ~0.3~   | ~Custom materials assigned via inspector~								    |
| ~0.4~   | ~Spawn geometry in transform heirarchy based on layer data~		    	    |
| 0.5     | User login API and stream selection UI prefabs		       				    |
| 0.6     | Local caching of Speckle streams			                                |
| 0.7     | Implement SpeckleUnitySender API    			                            |
