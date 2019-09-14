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
    public bool arranged;
    public Resolution resolution;
    public BoolReference InterfaceIsOpen;

    private void Start()
    {
        arranged = false;
        if (resolution == null)
            resolution = Resources.Load("ScriptableObjects/ResolutionAsset") as Resolution;

        InterfaceIsOpen.Value = true;
    }

    private void Update()
    {
        if (resolution == null)
            resolution = Resources.Load("ScriptableObjects/ResolutionAsset") as Resolution;

        AssemblePages();

        gameObject.SetActive(InterfaceIsOpen.Value);

    }

    void AssemblePages()
    {
        float right = resolution.Width * 2;
        float up = resolution.Height / 2;

        int buffer = 100;
        int rowcount = 0;
        int rowTotal = 6;

        int cnt = 0;
        Dictionary<string, int> nameMap = new Dictionary<string, int>();

        if (arranged)
        {
            //if we are arranging the tiles, we spread them out in the editor and disable the aspect ratio fitter
            foreach (UIB_Page page in GetComponentsInChildren<UIB_Page>())
            {


                // var tmp = arf.GetComponent<RectTransform>().position;
                //if get component has a "page", move it into a nice position;

                rowcount++;
                if (rowcount > rowTotal)
                {
                    rowcount = 0;
                    right = resolution.Width * 2;
                    up -= (resolution.Height + buffer);
                }

                var pos = new Vector3(right, up, 0);
                right += resolution.Width + buffer;
                page.GetComponent<RectTransform>().position = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);

            }
        }
        else
        {
            //if we are not arranging, let the pages pile up. As they would in the Running App
            foreach (UIB_Page page in GetComponentsInChildren<UIB_Page>())
            {
                page.GetComponent<RectTransform>().position = new Vector3(0, 0, 0);
            }
        }
    }
}
