using UnityEngine;
using Photon.Voice.Unity;
using Photon.Pun;
using Photon.Voice.PUN;

public class VoiceController : MonoBehaviour
{
    private static VoiceController instance;

    private Recorder recorder;
    private const byte GROUP_EVA_IVA = 1;
    private const byte GROUP_MCC_LLM = 2;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        recorder = Object.FindFirstObjectByType<Recorder>();

        if (recorder == null)
        {
            Debug.LogError("Recorder component not found.");
            return;
        }

        Photon.Voice.PUN.PunVoiceClient.Instance.Client.LoadBalancingPeer.OpChangeGroups(null, new byte[] { GROUP_EVA_IVA, GROUP_MCC_LLM });

        recorder.TransmitEnabled = false;
        recorder.InterestGroup = 0;

        Debug.Log("Connected to Photon: " + PhotonNetwork.IsConnected);
        Debug.Log("Joined Room: " + PhotonNetwork.InRoom);


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            StartTransmitting(GROUP_EVA_IVA);
        if (Input.GetKeyUp(KeyCode.Tab))
            StopTransmitting();

        if (Input.GetKeyDown(KeyCode.BackQuote))
            StartTransmitting(GROUP_MCC_LLM);
        if (Input.GetKeyUp(KeyCode.BackQuote))
            StopTransmitting();
    }

    void StartTransmitting(byte group)
    {
        recorder.InterestGroup = group;
        recorder.TransmitEnabled = true;
    }

    void StopTransmitting()
    {
        recorder.TransmitEnabled = false;
        recorder.InterestGroup = 0;
    }
}
