# How To Install

Since SpeckleUnity is a package installed via the Unity Package Manager (UPM), there are a few different ways to go about installing it into your Unity project.

## Scoped Registry (Recommended)

Add the following to the top of your manifest.json file right before the dependencies object. Next, save the file and restart the editor. You should then find the package in the package manager UI which can be installed like any other Unity package. You can even view a list of all previous versions of the package that are available in our registry.

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

**Note:** In some versions of Unity there is a bug with the package manager in how it communicates with scoped registries. If you don't see any new packages appear, you must also update your list of dependencies to include SpeckleUnity. For example...

``` js
"dependencies": {
    "com.open.speckleunity": "0.6.3-preview",
    "com.unity.ads": "2.0.8"
 }
```
Keep in mind that you will most likely have a larger list of dependencies than what is depicted in the example above. The line beginning with "com.unity.ads" merely serves as a placeholder to show that the necessary entry of "com.open.speckleunity" is inserted into the dependencies list.

## OpenUMP CLI

SpeckleUnity is also available on OpenUPM, a community driven "virtual" package registry. The reason why this method isn't recomended is because it's the least beginner friendly even though there's nothing wrong with doing it this way. To learn more about installing via OpenUPM, see their official guide [here](https://openupm.com/docs/getting-started.html).