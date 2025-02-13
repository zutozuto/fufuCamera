using UnityEngine;

public class ModelSelectionManager : MonoBehaviour
{
    public GameObject[] modelPrefabs;  // 3D 模型预制件
    private GameObject currentModel;   // 当前显示的模型
    public string modelParentName = "Model";  // 模型父物体名称

    private Transform modelParent;  // 模型的父物体

    void Start()
    {
        // 查找模型父物体
        modelParent = GameObject.Find(modelParentName)?.transform;
        if (modelParent == null)
        {
            Debug.LogError("Model parent object with name '" + modelParentName + "' not found.");
        }
    }

    // 切换到指定的模型
    public void SwitchModel(int modelIndex)
    {
        // 如果已有模型，先销毁当前模型
        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        // 确保索引有效
        if (modelIndex >= 0 && modelIndex < modelPrefabs.Length)
        {
            // 实例化新的模型，并设置它的父物体为 Model 节点
            if (modelParent != null)
            {
                currentModel = Instantiate(modelPrefabs[modelIndex], modelParent.position, modelParent.rotation);
                currentModel.transform.SetParent(modelParent);  // 设置父物体为 Model 节点
            }
        }
    }

    // 获取当前模型
    public GameObject GetCurrentModel()
    {
        return currentModel;
    }
}