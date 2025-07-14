using UnityEngine;
using Photon.Pun;

public class EVAObjectSync : MonoBehaviourPun, IPunObservable
{
    private RectTransform rectTransform;
    private bool parented = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!parented)
        {
            Transform panel = GameObject.Find("MappingPanel")?.transform;
            if (panel != null)
            {
                rectTransform.SetParent(panel, false);
                parented = true;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rectTransform.anchoredPosition);
            stream.SendNext(rectTransform.localEulerAngles.z);
        }
        else
        {
            if (!photonView.IsMine)
            {
                rectTransform.anchoredPosition = (Vector2)stream.ReceiveNext();

                float zRotation = (float)stream.ReceiveNext();
                Vector3 angles = rectTransform.localEulerAngles;
                rectTransform.localEulerAngles = new Vector3(angles.x, angles.y, zRotation);
            }
        }
    }
}
