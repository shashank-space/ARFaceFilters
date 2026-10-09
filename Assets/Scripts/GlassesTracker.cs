using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;

public class GlassesTracker : MonoBehaviour
{
    [Header("Glasses")]
    public RectTransform glasses;

    [Header("Screen")]
    public RectTransform screen;

    [Header("Tracking Settings")]
    public float sizeMultiplier = 2.5f;

    private Vector2 leftEye;
    private Vector2 rightEye;

    private bool faceDetected = false;

    private readonly object landmarkLock = new object();

    void OnEnable()
    {
        FaceLandmarkerRunner.OnFaceLandmarkResult += OnFaceLandmarkResult;
    }

    void OnDisable()
    {
        FaceLandmarkerRunner.OnFaceLandmarkResult -= OnFaceLandmarkResult;
    }

    // This is called by MediaPipe's background thread
    private void OnFaceLandmarkResult(FaceLandmarkerResult result)
    {
        if (result.faceLandmarks == null || result.faceLandmarks.Count == 0)
        {
            lock (landmarkLock)
            {
                faceDetected = false;
            }

            return;
        }

        var landmarks = result.faceLandmarks[0].landmarks;

        // Eye corner landmarks
        // Left eye: 33 and 133
        // Right eye: 263 and 362

        Vector2 newLeftEye = new Vector2(
            (landmarks[33].x + landmarks[133].x) / 2f,
            (landmarks[33].y + landmarks[133].y) / 2f
        );

        Vector2 newRightEye = new Vector2(
            (landmarks[263].x + landmarks[362].x) / 2f,
            (landmarks[263].y + landmarks[362].y) / 2f
        );

        lock (landmarkLock)
        {
            leftEye = newLeftEye;
            rightEye = newRightEye;
            faceDetected = true;
        }
    }

    void Update()
    {
        Vector2 currentLeftEye;
        Vector2 currentRightEye;
        bool detected;

        lock (landmarkLock)
        {
            currentLeftEye = leftEye;
            currentRightEye = rightEye;
            detected = faceDetected;
        }

        if (!detected)
        {

            return;
        }

        glasses.gameObject.SetActive(true);

        // Calculate center between the eyes
        Vector2 eyeCenter = (currentLeftEye + currentRightEye) / 2f;

        // Convert MediaPipe normalized coordinates to UI coordinates
        float x = (eyeCenter.x - 0.5f) * screen.rect.width;
        float y = (0.5f - eyeCenter.y) * screen.rect.height;

        glasses.localPosition = new Vector3(x, y, 0);

        // Calculate distance between eyes
        float eyeDistance = Vector2.Distance(
            currentLeftEye,
            currentRightEye
        );

        // Convert normalized distance to screen size
        float glassesWidth = eyeDistance * screen.rect.width * sizeMultiplier;

        float aspectRatio = glasses.rect.height / glasses.rect.width;

        float glassesHeight = glassesWidth * aspectRatio;

        glasses.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            glassesWidth
        );

        glasses.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            glassesHeight
        );

        // Calculate head tilt
        float angle = Mathf.Atan2(
            currentRightEye.y - currentLeftEye.y,
            currentRightEye.x - currentLeftEye.x
        ) * Mathf.Rad2Deg;

        glasses.localRotation = Quaternion.Euler(
            0,
            0,
            -angle
        );
    }
}