using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class UIModelManager : MonoBehaviour
{
    // UI 面板管理
    public PanelSwitcher panelSwitcher; // 引用负责管理 UI 面板的脚本
    public GameObject[] uiElements; // 用于存放 UI 元素（按钮、面板等）

    // 模型管理
    public GameObject[] modelPrefabs; // 3D 模型预制件
    private GameObject currentModel; // 当前显示的模型
    private Transform modelParent; // Model 节点作为父物体

    // AR Raycast 管理器
    private ARRaycastManager arRaycastManager; // AR Raycast 管理器
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // 存储 Raycast 命中的信息

    // 旋转控制
    public float rotationSpeed = 100f; // 模型旋转的速度
    private bool isRotationActive = false; // 是否正在旋转
    private string currentRotationAxis = ""; // 当前旋转轴

    // 触摸控制
    private Vector2 previousTouchPos; // 记录前一次触摸的位置
    private bool isDragging = false; // 是否正在拖动
    private float previousTouchDistance = 0f; // 记录前一次双指之间的距离（用于缩放）

    // XROrigin 的引用
    public Transform xrOrigin; // XROrigin，一般在场景中的 XR Origin 物体上
    private Camera arMainCamera; // AR 摄像机

    // 存储模型初始缩放
    private Vector3 initialScale;

    // 记录是否开始缩放
    private bool isScaling = false;

    void Start()
    {
        // 初始化 AR 摄像机
        arMainCamera = xrOrigin.GetComponentInChildren<Camera>();
        if (arMainCamera == null)
        {
            Debug.LogError("未找到 AR 摄像机。请确保 XR Origin 中包含 AR Camera。");
        }

        // 设置 Model 节点作为父物体
        modelParent = GameObject.Find("Model")?.transform;
        if (modelParent == null)
        {
            Debug.LogError("未找到 Model 物体，请检查场景中是否存在名为 'Model' 的 GameObject。");
        }

        // 获取 AR Raycast 管理器
        arRaycastManager = xrOrigin.GetComponent<ARRaycastManager>();
        if (arRaycastManager == null)
        {
            Debug.LogError("未找到 ARRaycastManager 组件。请确保该脚本挂载在合适的 GameObject 上。");
        }
    }

    void Update()
    {
        if (!currentModel)
            return;

        // 处理旋转
        HandleRotation();
        // 处理触摸交互
        HandleTouchInteraction();
    }

    // 切换模型
    public void SwitchModel(int modelIndex)
    {
        if (currentModel != null)
            Destroy(currentModel);

        if (modelIndex >= 0 && modelIndex < modelPrefabs.Length)
        {
            currentModel = Instantiate(modelPrefabs[modelIndex], modelParent.position, modelParent.rotation);
            currentModel.transform.SetParent(modelParent, false);
            initialScale = currentModel.transform.localScale;
        }
    }

    // 处理旋转
    private void HandleRotation()
    {
        if (isRotationActive && !string.IsNullOrEmpty(currentRotationAxis))
            RotateModel(currentRotationAxis);
    }

    // 旋转模型
    public void RotateModel(string axis)
    {
        if (currentModel)
        {
            float rotationAmount = rotationSpeed * Time.deltaTime;

            switch (axis)
            {
                case "X":
                    currentModel.transform.Rotate(Vector3.right * rotationAmount, Space.World);
                    break;
                case "-X":
                    currentModel.transform.Rotate(Vector3.left * rotationAmount, Space.World);
                    break;
                case "Y":
                    currentModel.transform.Rotate(Vector3.up * rotationAmount, Space.World);
                    break;
                case "-Y":
                    currentModel.transform.Rotate(Vector3.down * rotationAmount, Space.World);
                    break;
            }
        }
    }

    // 重置模型旋转
    public void ResetRotation()
    {
        if (currentModel != null)
            currentModel.transform.rotation = Quaternion.identity;
    }

    // 重置模型位置
    public void ResetPosition()
    {
        if (currentModel != null && arMainCamera != null)
        {
            Ray ray = arMainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            if (arRaycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                currentModel.transform.position = hitPose.position;
                currentModel.transform.rotation = hitPose.rotation;
                Debug.Log("模型位置重置为平面交点位置");
            }
            else
            {
                currentModel.transform.position = arMainCamera.transform.position + arMainCamera.transform.forward * 1.98f;
                currentModel.transform.rotation = arMainCamera.transform.rotation;
                Debug.Log("未命中平面，模型位置重置为摄像头前方位置");
            }
        }
    }

    // 重置模型缩放
    public void ResetScale()
    {
        if (currentModel != null)
            currentModel.transform.localScale = initialScale;
    }

    // 切换模型可见性
    public void ToggleModel()
    {
        if (currentModel != null)
            currentModel.SetActive(!currentModel.activeSelf);
    }

    // 旋转按钮按下
    public void OnRotateButtonDown(string axis)
    {
        currentRotationAxis = axis;
        isRotationActive = true;
    }

    // 旋转按钮松开
    public void OnRotateButtonUp()
    {
        isRotationActive = false;
    }

    // 切换 UI 面板
    public void SwitchPanel(int panelIndex)
    {
        panelSwitcher?.SwitchPanel(panelIndex);
    }

    // 显示/隐藏 UI 面板
    public void TogglePanel(int panelIndex, bool show)
    {
        panelSwitcher?.TogglePanel(panelIndex, show);
    }

    // 处理触摸交互
    private void HandleTouchInteraction()
    {
        HandleDrag();
        HandleScale();
    }

    // 处理拖拽
    private void HandleDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    previousTouchPos = touch.position;
                    isDragging = true;
                    break;
                case TouchPhase.Moved when isDragging:
                    Vector2 delta = touch.position - previousTouchPos;
                    MoveModel(delta);
                    previousTouchPos = touch.position;
                    break;
                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
    }

    // 处理缩放
    private void HandleScale()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            if (!isScaling)
            {
                // 当双指距离变化时，标记开始缩放
                previousTouchDistance = currentDistance;
                isScaling = true;
            }
            else
            {
                // 计算距离变化
                float deltaDistance = currentDistance - previousTouchDistance;

                // 根据双指距离变化调整模型缩放
                if (Mathf.Abs(deltaDistance) > 0.01f) // 防止在手指距离变化很小的情况下进行不必要的缩放
                {
                    ScaleModel(deltaDistance);
                }

                previousTouchDistance = currentDistance;
            }
        }
        else
        {
            // 如果触摸数量不是 2，就结束缩放
            isScaling = false;
        }
    }

    // 移动模型
    private void MoveModel(Vector2 delta)
    {
        if (currentModel)
        {
            Vector3 move = new Vector3(delta.x * 0.001f, delta.y * 0.001f, 0);
            currentModel.transform.position += move;
        }
    }

    // 缩放模型
    private void ScaleModel(float deltaDistance)
    {
        if (currentModel)
        {
            // 双指距离变大模型放大，变小模型缩小
            float scaleFactor = deltaDistance * 0.005f; // 缩放比例
            float newScale = Mathf.Clamp(currentModel.transform.localScale.x + scaleFactor, 0.05f, 4f);
            currentModel.transform.localScale = Vector3.one * newScale;
        }
    }

    // 隐藏所有 UI 元素
    public void HideUI()
    {
        foreach (var uiElement in uiElements)
        {
            if (uiElement != null)
                uiElement.SetActive(false);
        }
    }

    // 显示所有 UI 元素
    public void ShowUI()
    {
        foreach (var uiElement in uiElements)
        {
            if (uiElement != null)
                uiElement.SetActive(true);
        }
    }
}
