using Photon.Voice.Unity;
using UnityEngine;

public class VoiceToggleController : MonoBehaviour
{
    private Recorder recorder;

    void Start()
    {
        recorder = GetComponent<Recorder>();
        if (recorder == null)
        {
            Debug.LogError("Recorder not found on GameObject.");
        }
    }

    void Update()
    {
        // Push-to-talk for crew (Tab key)
        if (Input.GetKeyDown(KeyCode.Tab))
            recorder.TransmitEnabled = true;
        if (Input.GetKeyUp(KeyCode.Tab))
            recorder.TransmitEnabled = false;

        // Push-to-talk for MCC (BackQuote key)
        if (Input.GetKeyDown(KeyCode.BackQuote))
            recorder.TransmitEnabled = true;
        if (Input.GetKeyUp(KeyCode.BackQuote))
            recorder.TransmitEnabled = false;
    }
}
