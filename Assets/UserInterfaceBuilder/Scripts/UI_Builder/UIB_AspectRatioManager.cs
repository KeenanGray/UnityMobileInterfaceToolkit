using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Builder
{
    //This script should be added to the main canvas of the app
    [ExecuteInEditMode]
    public class UIB_AspectRatioManager : MonoBehaviour
    {
        public static Resolution resolution;

        static UIB_AspectRatioManager aspectRatioManager;

        private void Update()
        {
            var canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

            if (resolution == null)
            {
                resolution = Resources.Load("ScriptableObjects/ResolutionAsset") as Resolution;
            }
            // if (System.Math.Abs(resolution.Width - Screen.width) > float.Epsilon && System.Math.Abs(resolution.Height - Screen.height) > float.Epsilon)
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
            resolution.Width = Screen.width;
            resolution.Height = Screen.height;
        }

    }
}
