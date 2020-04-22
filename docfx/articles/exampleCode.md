# Example Code

Here's some example code demonstrating some common operations you may want to do when using SpeckleUnity in your software development projects.

## Logging In

Logging in can happen by default if you set the `onStartBehaviour` property in the manager for it to do so. In the event that you don't want to log in as anyone on start, here's how you do it:

``` cs
using UnityEngine;
using SpeckleUnity;
using SpeckleCore; // needed for referencing the User object

public class Example : MonoBehaviour
{
    // hold a reference to the manager you want to login with
    public SpeckleUnityManager manager;

    // call the login method on the manager and pass in the values you want to use
    public void Login (string email, string password)
    {
        // a callback is needed to handle the result of the login
        manager.LoginAsync (email, password, HandleLoginResult); 
    }

    // your callback needs to be a void method that only takes a User.
    // called after the async operation is finished.
    private void HandleLoginResult (User resultUser)
    {
        // if the user is not null, that means the login was successful
    }

    // if you want to change users in your code, just logout and log back in
    public void Logout ()
    {
        manager.Logout ();
    }
}
```

## Getting Stream Data

Once logged in as a user, either by the default behaviour or with your own code, you may want to get an array of meta data for the streams that are available to said user so that one of them can be picked to be added to the scene. Here's how you do that:

``` cs
using UnityEngine;
using SpeckleUnity;
using SpeckleCore; // needed for referencing the Streams array

public class Example : MonoBehaviour
{
    // hold a reference to the manager. 
    // Needs to be logged in as a user before getting streams
    public SpeckleUnityManager manager;

    // call the get streams method on the manager
    public void GetStreams ()
    {
        // a callback is needed to handle the result of the download
        manager.GetAllStreamMetaDataForUserAsync (HandleStreamResultForUser); 
    }

    // your callback needs to be a void method that only takes a SpeckleStream array.
    // called after the async operation is finished.
    private void HandleStreamResultForUser (SpeckleStream[] streams)
    {
        // if the array is not null, that means the download was successful
    }
}
```

## Controlling Receivers

There's a number of things involved with managing streams that are received in Unity. To that end, some methods have been exposed that allow you to easily make these interactions without needing to worry about all of those nuances:

``` cs
using UnityEngine;
using SpeckleUnity;

public class Example : MonoBehaviour
{
    // hold a reference to the manager. Need to be logged before adding receivers
    public SpeckleUnityManager manager;

    public void ExampleMethod ()
    {
        // The arguments are the ID of the stream to receive, the transform
        // for it to spawn under and whether or not the stream should start being 
        // streamed when calling this line of code.
        manager.AddReceiverAsync ("some stream ID", null, true);

        // cleans up everything the AddReceiver method created except for the root transform
        // there are overloads for removing by stream ID or by root transform reference
        manager.RemoveReceiver (0);

        // calls the remove method on all receivers.
        manager.ClearReceivers ();
    }
}
```

## Getting SpeckleObject Data

All `SpeckleObjects` have their own standard set of values like their ID or who their owner is, but Clients like Revit or GSA asign their own unique **properties** into each object. Here is an example of how you can access any of this data:

``` cs
using UnityEngine;
using SpeckleUnity;
using SpeckleCore; // needed for referencing SpeckleObjects

public class Example : MonoBehaviour
{
    // hold a reference to the manager. Need to be logged before adding receivers
    public SpeckleUnityManager manager;

    // run a method like this after selecting the object you want via raycast or another way
    public void PrintObjectData (GameObject gameObjectKey)
    {
        // use the gameobject as an ID to get back the data associated to it
        if (manager.TryGetSpeckleObject (gameObjectKey, out SpeckleObject data))
        {
            // basic data is stored at the root of the SpeckleObject
            Debug.Log (data._id);
            Debug.Log (data.Owner);
            // the type field could give you a bit more insight on the property schema.
            Debug.Log (data.Type); 

            // The more interesting data is kept in the Properties dictionary.
            // It's a dictionary of string keys to object values. You have to cast the values
            // into the type you think they are. The schema of this dictionary is basically
            // never consistent. Revit objects are super different amongst themselves let 
            // alone the differences between different clients. A value could even be
            // another dictionary! Your best bet is to just look for the properties you
            // need rather than try to get everything.
            if (data.Properties.TryGetValue ("myPropertyKey", out object propertyValue))
            {
                Debug.Log (propertyValue.ToString ());
            }
        }
        else
        {
            Debug.LogError ("The GameObect was either null or not a SpeckleObject");
        }
    }
}
```

## Get Update Percentage

Whether if it's for a loading bar or giving some other feedback to users of what's going on in the background, here's a simple way of doing it:

``` cs
using UnityEngine;
using SpeckleUnity;
using SpeckleCore; // needed for referencing streams and users

// Add this component to a GameObject that's under a Canvas in your scene
[RequireComponent (typeof (Text))]
public class TextSetter : MonoBehaviour
{
    Text label;

    void Start ()
    {
        label = GetComponent<Text> ();
    }

    // Write a public void method that takes only a SpeckleUnityUpdate object.
    // Assign this to the SpeckleUnityManager's onUpdateProgress field as a dynamic callback (not static).
    // When a stream is updated this method will automatically be called.
    // You can use the data coming from it to make things like a loading bar for each stream.
    // The update data has more fields like the id of the stream the update came from and so on.
    public void SetText (SpeckleUnityUpdate data)
    {
        label.text = string.Format ("Loading...: {0}%", Mathf.Floor (data.updateProgress * 100));
    }
}
```