using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class PhotoCapture : MonoBehaviour
{
    public RawImage displayImage;
    private string savedImagePath = "";

    // 拍照并保存图片
    public void CapturePhoto()
    {
        // 先生成一个相机截图
        StartCoroutine(TakeScreenshot());
    }

    // 拍照并保存到设备相册
    private IEnumerator TakeScreenshot()
    {
        // 等待一帧，确保图像渲染完成
        yield return new WaitForEndOfFrame();

        // 获取截图的纹理
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // 将截图保存为 PNG 格式
        byte[] bytes = screenshot.EncodeToPNG();

        // 生成保存路径
        string path = Path.Combine(Application.persistentDataPath, "screenshot.png");
        File.WriteAllBytes(path, bytes);

        // 释放截图纹理内存
        Destroy(screenshot);

        // 使用 NativeGallery 保存到相册
        if (NativeGallery.IsMediaPickerBusy())
        {
            Debug.LogWarning("Media picker is busy. Please try again later.");
            yield break;
        }

        NativeGallery.SaveImageToGallery(path, "MyApp", "screenshot.png");

        // 显示提示或反馈
        Debug.Log("Screenshot saved to gallery!");

        // 更新 UI（如果需要）
        displayImage.texture = screenshot;
    }
}