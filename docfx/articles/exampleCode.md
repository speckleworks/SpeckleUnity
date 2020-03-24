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
        manager.Login (email, password, HandleLoginResult); 
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
        manager.GetAllStreamMetaDataForUser (HandleStreamResultForUser); 
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
        manager.AddReceiver ("some stream ID", null, true);

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

## Putting It All Together

Here's an example of all the above usecases being used together in some theoretical Unity app that allows its users to provide an email and password, login as themself, get the data of the streams they have access to, pick a stream to start receiving and print the speckle data associated to a given object after its selected:

``` cs
using UnityEngine;
using SpeckleUnity;
using SpeckleCore; // needed for referencing streams and users

public class Example : MonoBehaviour
{
    public SpeckleUnityManager manager;

    private User loggedInUser; // you can display user properties in the UI
    private SpeckleStream[] userStreams; // you can display stream options in the UI

    public void Login (string email, string password)
    {
        manager.Login (email, password, HandleLoginResult);
    }

    private void HandleLoginResult (User resultUser)
    {
        if (resultUser != null) 
        {
            loggedInUser = resultUser; // cache the user for later use
            manager.GetAllStreamsForUser (HandleStreamResultForUser);
        }
        else
        {
            Debug.LogError ("Could not log in to the server");
        }
    }

    private void HandleStreamResultForUser (SpeckleStream[] streams)
    {
        if (streams != null)
        {
            userStreams = streams; // cache the array for later use
        }
        else
        {
            Debug.LogError ("Could not get streams for user");
        }
    }

    // This method could be called from a dropdown UI or button or whatever else
    public void SelectStream (int index)
    {
        manager.AddReceiver (userStreams[index].StreamId, null, true);
    }

    // run a method like this after selecting the object you want via raycast or another way
    public void PrintObjectData (GameObject gameObjectKey)
    {
        // use the gameobject as an ID to get back the data associated to it
        if (manager.TryGetSpeckleObject (gameObjectKey, out SpeckleObject data))
        {
            // basic data is stored at the root of the SpeckleObject
            Debug.Log (data._id);
            Debug.Log (data.Owner);
            Debug.Log (data.Type); 
            
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