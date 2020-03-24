using SpeckleUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class RaycastCheck : MonoBehaviour
{
    public GameobjectReference gameobjectReference;
    public SpeckleUnityManager manager;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIElement())
            {
                Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(rayCast, out hit))
                {
                    gameobjectReference.reference = hit.transform.gameObject;
                    GetCurrentGameobjectsData();
                }
            }
        }
    }

    public void GetCurrentGameobjectsData()
    {
        if (gameobjectReference.reference != null)
            manager.TryGetSpeckleObject (gameobjectReference.reference, out SpeckleCore.SpeckleObject data);
    }

    public static bool IsPointerOverUIElement()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
