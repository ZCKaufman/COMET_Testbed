using UnityEngine;
using Photon.Pun;

public class POISync : MonoBehaviourPun, IPunObservable
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        Transform parent = GameObject.Find("EVAMapPanel")?.transform;
        if (parent != null)
        {
            rectTransform.SetParent(parent, worldPositionStays: false);
        }
        else
        {
            Debug.LogError("EVAMapPanel not found for POI parenting.");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rectTransform.anchoredPosition);
        }
        else
        {
            if (!photonView.IsMine)
            {
                rectTransform.anchoredPosition = (Vector2)stream.ReceiveNext();
            }
        }
    }
}
