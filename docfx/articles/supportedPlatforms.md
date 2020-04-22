# Supported Platforms

Since the package is still in preview, support for all of Unity's build options hasn't been tested yet. This is mainly due to lack of hardware to test the builds but if you do discover a new working possible build option (or a way to achieve it) please let us know!

Here is a table breaking down all the build options in Unity and if SpeckleUnity is known to support them:

| Platform       | Supported | Untested | Not supported |
| -------------- | --------- | -------- | ------------- |
| Windows        | X   		 |          |               |
| Android        | X   		 |          |               |
| Windows IL2CPP |     		 | X        |               |
| Android IL2CPP |    		 | X        |               |
| iOS            |    		 | X        |               |
| MacOS          |    		 | X        |               |
| Linux          |    		 | X        |               |
| WebGL          |    		 |          | X             |

WebGl isn't supported because web sockets simply won't work on the platform via Unity. For web viewing purposes please check out [https://app.speckle.systems/](https://app.speckle.systems/) as an alternative.