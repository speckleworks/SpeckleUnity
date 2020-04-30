# Supported Platforms

Since the package is still in preview, support for all of Unity's build options hasn't been tested yet. This is mainly due to lack of hardware to test the builds but if you do discover a new working possible build option (or a way to achieve it) please let us know!

Here is a table breaking down all the build options in Unity and if SpeckleUnity is known to support them. This list is inclusive of both .NET Framework 4.x and .NET Standard 2.0:

| Platform       | Supported | Untested | Not supported |
| -------------- | --------- | -------- | ------------- |
| Windows        | X   		 |          |               |
| Android        | X   		 |          |               |
| Windows IL2CPP | X   		 |          |               |
| Android IL2CPP | X  		 |          |               |
| iOS            |    		 | X        |               |
| MacOS          |    		 | X        |               |
| Linux          |    		 | X        |               |
| WebGL          |    		 |          | X             |

WebGl isn't supported because web sockets simply won't work on the platform via Unity. For web viewing purposes please check out [https://app.speckle.systems/](https://app.speckle.systems/) as an alternative.

# iOS and IL2CPP

In order for Unity to successfully build to iOS (untested) and any other target that uses an IL2CPP scripting backend, a `link.xml` file needs to be added to the root of your project's Assets folder with the following content to prevent the build process from stripping any of Speckle's code:

``` xml
<linker>
  <!--Preserve these entire assemblies-->
  <assembly fullname="SpeckleUnity" preserve="all"/>
  <assembly fullname="SpeckleCore" preserve="all"/>
  <assembly fullname="SpeckleCoreGeometry" preserve="all"/>
  <assembly fullname="SpeckleCoreGeometryClasses" preserve="all"/>
  <assembly fullname="SpeckleElements" preserve="all"/>
  <assembly fullname="SpeckleElementsClasses" preserve="all"/>
</linker>
```

If you have additional Speckle Kits or other bits of code that you want to mark as preserved during the build process, just add those to this file before building.