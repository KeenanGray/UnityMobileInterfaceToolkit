using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Builder
{
    /*
     * This class is the container for every page that will be visible in the app
     * Initializing it modifies its size to be slightly larger than the canvas.
     * This way, children using the Aspect Ratio Fitter will fill the canvas instead of "fitting" inside 
     * this works around a slight border of pixels when using AspectRatioFitter - fit in canvas
     * */
    public class UIB_PageContainer : MonoBehaviour
    {
        GameObject Cover;
        // Use this for initialization
        public void Init()
        {
            Cover = GameObject.Find("CoverImage").gameObject;
            Cover.GetComponent<Image>().enabled = true;
            float offset = 1.0f;
            AspectRatioFitter arf;
            if (GetComponent<AspectRatioFitter>() == null)
            {
                arf = gameObject.AddComponent<AspectRatioFitter>();
            }
            else if (arf = GetComponent<AspectRatioFitter>())
            {
                arf.aspectRatio = (UIB_AspectRatioManager.ScreenWidth) / (UIB_AspectRatioManager.ScreenHeight);
                arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                arf.enabled = false;
            }
            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x + offset, rt.sizeDelta.y + offset);
            //            GetComponent<RectTransform>().sizeDelta = new Vector2(UIB_AspectRatioManager_Editor.ScreenHeight, UIB_AspectRatioManager_Editor.ScreenWidth);

        }

        public void DisableCover()
        {
            Cover.SetActive(false);
        }

    }

}