using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;
using NativeGalleryNamespace;

public class CameraController : MonoBehaviour
{
    [SerializeField] private RawImage cameraPreview; // 用于显示摄像头的UI组件
    [SerializeField] private GameObject loadingScreen; // 加载界面
    [SerializeField] private Camera uiCamera;  // UI 相机
    [SerializeField] private Camera arCamera;  // AR 相机
    [SerializeField] private float timeoutSeconds = 5f; // 摄像头启动超时时间

    private WebCamTexture webcamTexture;
    private RenderTexture renderTexture;
    private Texture2D screenshotTexture;

    private int targetWidth;
    private int targetHeight;

    void Start()
    {
        // 显示加载界面
        loadingScreen.SetActive(true);
        
        // 延迟请求权限，防止启动时阻塞
        StartCoroutine(RequestCameraPermission());
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator RequestCameraPermission()
    {
        yield return new WaitForSeconds(1f); // 延迟1秒再请求权限

        // 检查摄像头权限
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            // 等待权限响应
            float waitTime = 0;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && waitTime < timeoutSeconds)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.LogError("Camera permission denied!");
                loadingScreen.SetActive(false); // 隐藏加载界面
                yield break;
            }
            
            loadingScreen.SetActive(false);
        }

        // 权限请求完成后初始化摄像头
        StartCoroutine(InitializeCamera());
        loadingScreen.SetActive(false);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator InitializeCamera()
    {
        // 1. 检查设备是否有摄像头
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No camera detected!");
            loadingScreen.SetActive(false); // 隐藏加载界面
            yield break;
        }

        // 2. 初始化摄像头（选择第一个可用摄像头）
        string cameraName = WebCamTexture.devices[0].name;
        webcamTexture = new WebCamTexture(cameraName, 1280, 720, 30);
        cameraPreview.texture = webcamTexture;

        // 3. 启动摄像头并检测超时
        webcamTexture.Play();
        bool isCameraReady = false;
        float startTime = Time.time;

        while ((Time.time - startTime < timeoutSeconds))
        {
            // 摄像头已启动的条件：宽度/高度有效且纹理更新
            if (webcamTexture.width > 100 && webcamTexture.height > 100)
            {
                isCameraReady = true;
                loadingScreen.SetActive(false);
                break;
            }
            yield return null;
        }

        // 4. 处理结果
        if (!isCameraReady)
        {
            Debug.LogError($"Camera failed to start! Device: {cameraName}");
            webcamTexture.Stop();
            cameraPreview.texture = null;
            loadingScreen.SetActive(false); // 隐藏加载界面
            // 可以在此显示错误UI提示
        }
        else
        {
            Debug.Log($"Camera started: {webcamTexture.width}x{webcamTexture.height}");
            cameraPreview.transform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);
            loadingScreen.SetActive(false); // 隐藏加载界面
        }
    }

    // 拍照方法
    public void CaptureImage()
    {
        // 使用 AR 相机渲染画面到渲染纹理
        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        screenshotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        arCamera.targetTexture = renderTexture;
        arCamera.Render();

        // 从渲染纹理获取像素数据
        RenderTexture.active = renderTexture;
        screenshotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshotTexture.Apply();

        // 保存图像到相册
        NativeGallery.SaveImageToGallery(screenshotTexture, "ARPhoto", "screenshot.png");

        // 清理
        arCamera.targetTexture = null;
        RenderTexture.active = null;
    }

    private void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}
