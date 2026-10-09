using UnityEngine;
using UnityEngine.UI;

public class WebcamManager : MonoBehaviour
{
    public RawImage webcamDisplay;

    private WebCamTexture webcamTexture;

    void Start()
    {
        StartWebcam();
    }

    void StartWebcam()
    {
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No webcam detected!");
            return;
        }

        string cameraName = WebCamTexture.devices[0].name;

        webcamTexture = new WebCamTexture(
            cameraName,
            1280,
            720,
            30
        );

        webcamDisplay.texture = webcamTexture;

        webcamTexture.Play();
    }

    void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}