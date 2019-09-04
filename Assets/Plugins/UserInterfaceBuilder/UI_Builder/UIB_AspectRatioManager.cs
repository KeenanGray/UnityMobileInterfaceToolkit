using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Builder
{
    //This script should be added to the main canvas of the app
    public class UIB_AspectRatioManager : MonoBehaviour
    {
        public static float ScreenHeight;
        public static float ScreenWidth;

        static UIB_AspectRatioManager aspectRatioManager;

        private void Awake()
        {
            GetScreenResolution();

        }
        public static UIB_AspectRatioManager Instance()
        {
            if (aspectRatioManager == null)
            {
                aspectRatioManager = GameObject.FindWithTag("MainCanvas").GetComponent<UIB_AspectRatioManager>();
                return aspectRatioManager;
            }
            else
                return aspectRatioManager;
        }
        public void GetScreenResolution()
        {
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
        }
    }
}