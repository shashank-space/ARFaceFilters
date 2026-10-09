using UnityEngine;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;

public class FaceGlasses3D : MonoBehaviour
{
    [Header("3D Glasses")]
    public Transform glasses;

    [Header("Tracking Screen")]
    public RectTransform screen;

    [Header("Settings")]
    public float scaleMultiplier = 2.5f;

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

    private void OnFaceLandmarkResult(FaceLandmarkerResult result)
    {
        if (result.faceLandmarks == null ||
            result.faceLandmarks.Count == 0)
        {
            lock (landmarkLock)
            {
                faceDetected = false;
            }

            return;
        }

        var landmarks = result.faceLandmarks[0].landmarks;

        // Store ONLY the normalized landmark coordinates here.
        // Do not access Unity RectTransform from this callback.

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

        if (!detected || glasses == null || screen == null)
        {
            if (glasses != null)
                glasses.gameObject.SetActive(false);

            return;
        }

        glasses.gameObject.SetActive(true);

        // Now we are on Unity's main thread,
        // so accessing screen.rect is safe.

        float width = screen.rect.width;
        float height = screen.rect.height;

        Vector3 leftPosition = new Vector3(
            (currentLeftEye.x - 0.5f) * width,
            (0.5f - currentLeftEye.y) * height,
            0f
        );

        Vector3 rightPosition = new Vector3(
            (currentRightEye.x - 0.5f) * width,
            (0.5f - currentRightEye.y) * height,
            0f
        );

        // Center between eyes
        Vector3 center =
            (leftPosition + rightPosition) / 2f;

        // Distance between eyes
        float eyeDistance =
            Vector3.Distance(leftPosition, rightPosition);

        // Move glasses
        glasses.localPosition = center;

        // Rotate with face
        Vector2 direction =
            new Vector2(
                rightPosition.x - leftPosition.x,
                rightPosition.y - leftPosition.y
            );

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        glasses.localRotation =
            Quaternion.Euler(0f, 0f, -angle);

        // Scale with face
        float scale =
            eyeDistance * scaleMultiplier;

        glasses.localScale =
            Vector3.one * scale;
    }
}