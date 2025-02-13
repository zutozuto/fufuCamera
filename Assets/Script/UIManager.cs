using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PhotoCapture photoCapture;

    // 为按钮绑定点击事件
    public void OnCaptureButtonPressed()
    {
        photoCapture.CapturePhoto();
    }
}