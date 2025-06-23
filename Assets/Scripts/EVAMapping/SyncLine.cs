using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class SyncLine : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public RectTransform rectTransform;
    public Image image;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;

        if (data.Length >= 7)
        {
            Vector2 start = (Vector2)data[0];
            Vector2 end = (Vector2)data[1];
            Color color = new Color((float)data[2], (float)data[3], (float)data[4]);
            string routeType = (string)data[5];
            float distance = (float)data[6];

            // Defer setup until MapImage is available
            StartCoroutine(WaitForMapAndSetup(start, end, color));

            // UI manager and scroll entry creation
            RouteDrawingManager mgr = FindFirstObjectByType<RouteDrawingManager>();
            if (mgr != null)
            {
                GameObject scrollEntry = mgr.AddLineToScrollView(routeType, start, end, distance);

                PhotonView pv = GetComponent<PhotonView>();
                if (pv != null)
                {
                    mgr.RegisterLine(pv.ViewID, gameObject, scrollEntry, routeType, start, end, distance);
                }

                DeleteOnClick del = GetComponent<DeleteOnClick>();
                if (del != null)
                {
                    del.manager = mgr;
                }
            }
        }
    }

    private IEnumerator WaitForMapAndSetup(Vector2 start, Vector2 end, Color color)
    {
        while (GameObject.Find("MapImage") == null)
        {
            yield return null;
        }

        Transform mapImage = GameObject.Find("MapImage").transform;
        transform.SetParent(mapImage, false);

        Vector2 dir = (end - start).normalized;
        float dist = Vector2.Distance(start, end);

        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = start;
        rectTransform.sizeDelta = new Vector2(dist, 4f);
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        image.color = color;
    }
}
