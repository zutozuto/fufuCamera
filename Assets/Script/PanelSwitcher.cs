using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject[] panels;  // 所有面板
    public GameObject uiPanel;   // 主 UI 面板，显示/隐藏控制

    // 切换到指定的面板（仅显示该面板，隐藏其他面板）
    public void SwitchPanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= panels.Length) return;

        // 隐藏所有面板
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }

        // 显示指定的面板
        panels[panelIndex].SetActive(true);

        // 可选：切换主 UI 面板的显示或隐藏
        ToggleUIPanel(true);  // 根据需求可调整
    }

    // 控制指定面板的显示与隐藏（接受 bool 参数来控制显示/隐藏）
    public void TogglePanel(int panelIndex, bool show)
    {
        if (panelIndex < 0 || panelIndex >= panels.Length) return;

        // 设置面板的显示或隐藏状态
        panels[panelIndex].SetActive(show);
    }

    // 控制 uiPanel（主 UI 面板）显示或隐藏
    private void ToggleUIPanel(bool show)
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(show);  // 显示或隐藏 UI 面板
        }
    }

    // 新方法：控制 uiPanel 的显示或隐藏
    public void ToggleUIPanelVisibility()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(!uiPanel.activeSelf);  // 切换 uiPanel 的显示状态
        }
    }
}