using UnityEngine;

public class ModelControlManager : MonoBehaviour
{
    private GameObject currentModel;   // 当前显示的模型
    public float rotationSpeed = 100f;  // 旋转速度（可以根据需求调整）
    private bool isRotating = false;
    private string rotateAxis = "";

    void Start()
    {
        // 从模型选择管理器获取当前模型
        currentModel = GetComponent<ModelSelectionManager>().GetCurrentModel();
    }

    void Update()
    {
        // 持续旋转（如果旋转标志为 true）
        if (isRotating && currentModel != null)
        {
            RotateModel(rotateAxis);
        }
    }

    // 旋转模型的函数（基于世界坐标旋转）
    public void RotateModel(string axis)
    {
        if (currentModel)
        {
            // 根据世界坐标来旋转
            switch (axis)
            {
                case "X":
                    currentModel.transform.Rotate(Vector3.right * (rotationSpeed * Time.deltaTime), Space.World);
                    break;
                case "-X":
                    currentModel.transform.Rotate(Vector3.left * (rotationSpeed * Time.deltaTime), Space.World);
                    break;
                case "Y":
                    currentModel.transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime), Space.World);
                    break;
                case "-Y":
                    currentModel.transform.Rotate(Vector3.down * (rotationSpeed * Time.deltaTime), Space.World);
                    break;
            }
        }
    }

    // 按下旋转按钮时开始旋转
    public void OnRotateButtonDown(string axis)
    {
        rotateAxis = axis;
        isRotating = true;  // 开始旋转
    }

    // 松开旋转按钮时停止旋转
    public void OnRotateButtonUp()
    {
        isRotating = false;  // 停止旋转
    }

    // 控制当前模型的显示/隐藏
    public void ToggleModel()
    {
        if (currentModel != null)
        {
            currentModel.SetActive(!currentModel.activeSelf);  // 切换模型的显示与隐藏
        }
    }

    // 重置模型的旋转
    public void ResetRotation()
    {
        if (currentModel != null)
        {
            currentModel.transform.rotation = Quaternion.identity;  // 恢复初始旋转
        }
    }
}
