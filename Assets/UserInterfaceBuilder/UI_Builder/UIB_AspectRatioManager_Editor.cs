using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Builder
{
    //This script should be added to the main canvas of the app
    [ExecuteInEditMode]
    public class UIB_AspectRatioManager_Editor : MonoBehaviour
    {
#if UNITY_EDITOR
        public static float ScreenHeight;
        public static float ScreenWidth;

        public bool IsInEditor;

        static UIB_AspectRatioManager_Editor aspectRatioManager;

        private void Awake()
        {
            GetScreenResolution();
        }

        public static UIB_AspectRatioManager_Editor Instance()
        {
            if (aspectRatioManager == null)
            {
                aspectRatioManager = GameObject.FindWithTag("MainCanvas").GetComponent<UIB_AspectRatioManager_Editor>();
                return aspectRatioManager;
            }
            else
                return aspectRatioManager;
        }

        private void Update()
        {
            if (IsInEditor)
            {
                //            Debug.Log("Run");
                GetScreenResolution();
            }
        }

        public void GetScreenResolution()
        {
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;

            float right = ScreenWidth * 2;
            float up = ScreenHeight / 2;

            int buffer = 100;
            int rowcount = 0;
            int rowTotal = 6;

            int cnt = 0;
            Dictionary<string, int> nameMap = new Dictionary<string, int>();

            foreach (AspectRatioFitter arf in GetComponentsInChildren<AspectRatioFitter>())
            {

                arf.enabled = true;
                arf.aspectRatio = (ScreenWidth) / (ScreenHeight);
                arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                arf.enabled = false;

                // var tmp = arf.GetComponent<RectTransform>().position;
                //if get component has a "page", move it into a nice position;
                var page = arf.GetComponent<UIB_Page>();
                if (page != null && arf.tag != "App_Biography" && arf.tag != "Pool" && arf.transform.parent.tag!="Pool")
                {
                    rowcount++;
                    if (rowcount > rowTotal)
                    {
                        rowcount = 0;
                        right = ScreenWidth * 2;
                        up -= (ScreenHeight + buffer);
                    }

                    var pos = new Vector3(right, up, 0);
                    right += ScreenWidth + buffer;
                    arf.GetComponent<RectTransform>().position = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
                }


               if (arf.transform.parent.tag == "Pool")
                {
                    var commonName = arf.transform.parent.name.Split('(')[0];
                    if (!nameMap.ContainsKey(arf.transform.parent.name))
                    {
                        nameMap.Add(commonName, cnt);
                        nameMap[commonName] = cnt;

                        rowcount++;
                        if (rowcount > rowTotal)
                        {
                            rowcount = 0;
                            right = ScreenWidth * 2;
                            up -= (ScreenHeight + buffer);
                        }

                        var pos = new Vector3(-right, up, 0);
                        right += ScreenWidth + buffer;
                        arf.transform.parent.GetComponent<RectTransform>().position = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
                        arf.GetComponent<RectTransform>().position = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
                    }
                    else
                    {
                        cnt++;
                       
                    } 
                }

            }
        }
#endif
    }
}