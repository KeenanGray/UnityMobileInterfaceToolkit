using System.Collections;
using System.Collections.Generic;
using UI_Builder;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIB_AspectRatioManager))]
[RequireComponent(typeof(UIB_InputManager))]
[RequireComponent(typeof(UIB_PageManager))]
[ExecuteInEditMode]
public class UIB_Root : MonoBehaviour
{
    UIB_AspectRatioManager aspectRatioManager;

    public bool arranged;

    private void Start()
    {
        aspectRatioManager = GetComponent<UIB_AspectRatioManager>();
        arranged = false;
    }

    private void Update()
    {
        if (aspectRatioManager == null)
        {
            aspectRatioManager = GetComponent<UIB_AspectRatioManager>();
        }
        aspectRatioManager.GetScreenResolution();

        AssemblePages();

    }

    void AssemblePages()
    {
        float right = UIB_AspectRatioManager.ScreenWidth * 2;
        float up = UIB_AspectRatioManager.ScreenHeight / 2;

        int buffer = 100;
        int rowcount = 0;
        int rowTotal = 6;

        int cnt = 0;
        Dictionary<string, int> nameMap = new Dictionary<string, int>();

        if (arranged)
        {
            //if we are arranging the tiles, we spread them out in the editor and disable the aspect ratio fitter
            foreach (AspectRatioFitter arf in GetComponentsInChildren<AspectRatioFitter>())
            {
                arf.enabled = true;
                arf.aspectRatio = (UIB_AspectRatioManager.ScreenWidth) / (UIB_AspectRatioManager.ScreenHeight);
                arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                arf.enabled = false;

                // var tmp = arf.GetComponent<RectTransform>().position;
                //if get component has a "page", move it into a nice position;
                var page = arf.GetComponent<UIB_Page>();
                if (page != null && arf.tag != "App_Biography" && arf.tag != "Pool" && arf.transform.parent.tag != "Pool")
                {
                    rowcount++;
                    if (rowcount > rowTotal)
                    {
                        rowcount = 0;
                        right = UIB_AspectRatioManager.ScreenWidth * 2;
                        up -= (UIB_AspectRatioManager.ScreenHeight + buffer);
                    }

                    var pos = new Vector3(right, up, 0);
                    right += UIB_AspectRatioManager.ScreenWidth + buffer;
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
                            right = UIB_AspectRatioManager.ScreenWidth * 2;
                            up -= (UIB_AspectRatioManager.ScreenHeight + buffer);
                        }

                        var pos = new Vector3(-right, up, 0);
                        right += UIB_AspectRatioManager.ScreenWidth + buffer;
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
        else
        {
            //if we are not arranging, let the pages pile up. As they would in the Running App
            foreach (AspectRatioFitter arf in GetComponentsInChildren<AspectRatioFitter>())
            {
                arf.enabled = true;
            }
        }
    }
}
