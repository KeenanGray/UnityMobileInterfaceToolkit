using UnityEditor;
using UnityEngine;

namespace UI_Builder
{
    [CustomEditor(typeof(UIB_ScrollingMenu))]
    [CanEditMultipleObjects]
    public class UIB_SrollingMenu_Editor : Editor
    {
        private void OnSceneGUI()
        {
            if (Selection.gameObjects.Length == 1)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    if (go.GetComponent<UIB_ScrollingMenu>() != null)
                    {
                        go.GetComponent<UIB_ScrollingMenu>().Setup();
                    }
                }
            }
        }
    }
}