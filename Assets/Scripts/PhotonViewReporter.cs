using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PhotonViewReporter : MonoBehaviour
{
    [Tooltip("Delay in seconds after scene load before reporting PhotonViews.")]
    public float delayInSeconds = 0.2f;

    void Start()
    {
        StartCoroutine(DelayedReport());
    }

    IEnumerator DelayedReport()
    {
        yield return new WaitForSeconds(delayInSeconds);

        Debug.Log("---- Full PhotonView Report (Including Inactive Objects) ----");

        PhotonView[] photonViews = Resources.FindObjectsOfTypeAll<PhotonView>();

        if (photonViews.Length == 0)
        {
            Debug.Log("No PhotonView components found.");
        }

        foreach (PhotonView view in photonViews)
        {
            string status = view.gameObject.activeInHierarchy ? "Active" : "Inactive";
            Debug.Log($"GameObject: '{view.gameObject.name}' | ViewID: {view.ViewID} | Status: {status}");
        }

        Debug.Log("---- End of Report ----");
    }
}



