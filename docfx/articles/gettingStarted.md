# Getting Started

This article describes how to connect to your first stream with SpeckleUnity

## Step 0: Satisfy Dependencies

Once SpeckleUnity is installed in your Unity project, there is just **one more dependency** you need to satisfy. Currently, SpeckleCore still makes some assumptions about "Speckle" being installed on your Windows computer even though the libraries are also present in the package. So for the time being, any use of SpeckleUnity, either in editor or in a Windows build, needs to have the following installed on the same computer in order to run.

![Figure1](~/images/gettingStarted1.png)

With Speckle now installed, you should be able to enter play mode and run builds without concern.

We plan to remove this dependency in a future update.

## Step 1: Create a SpeckleUnityManager

In your scene's hierarchy view, right click and select "SpeckleUnityManager":

![Figure2](~/images/gettingStarted2.png)

This will add a new GameObject to your scene which only has the `SpeckleUnityManager` component attached to it:

![Figure3](~/images/gettingStarted3.png)

A single instance of a manager is designed to connect to a single speckle server and authenticate as a single user for all interactions it makes. This means that multiple managers will allow you to connect to multiple servers or connect as multiple users at the same time.

## Step 2: Configure your SpeckleUnityManager

In the inspector of our manager object, there are a number of fields exposed to us but for this quick start guide we will be focussing only on the following fields:

* Server Url
* Auth Token
* Mesh Material
* Receivers

Your server URL is the root URL to the Speckle server you wish to connect to, followed by "/api/". By default, this field is set to the public hestia server that is available to all users. If you require to connect to a different server, like a privately hosted company server, just change this default value to the URL of your new server.

Your authentication token can be found after logging in to your server, viewing your profile and clicking "Show API Token". Paste this value into the Auth Token field of the inspector to authenticate this manager as yourself.

The Mesh Material field allows you to assign what material all meshe objects in your received streams are rendered with. By default this field is left null because your Unity project may be using the built in renderer, the Universal Render Pipeline or the HD Render Pipeline, all of which require their own materials that are incompatible with one another. So you can either assign an existing material from your assets folder, or simply create a new material and assign that in. Leaving this field empty will cause all objects to be rendered with a bright magenta color.

Finally, the receivers field expands to reveal a list of receivers. By default it doesn't have any, so set the length of your receivers list to be 1 as shown below:

![Figure4](~/images/gettingStarted4.png)

This will add a new receiver instance which you can further configure with the ID of the stream it needs to receive as well as an **optional** field of type `Transform` called Stream Root. This Stream Root field specifies what object in the scene the stream objects will spawn under. If no value is set, a new object will be created by default. For now, just paste in the ID of your stream and then enter play mode.

## Step 3: Receive a Stream

If you enter play mode now, assuming you configured your manager correctly, you should be able to see your stream appear after a moment, rendered with the material you assigned!