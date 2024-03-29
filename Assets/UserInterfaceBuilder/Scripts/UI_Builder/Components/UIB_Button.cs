﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

//This Script is used on buttons that will change the screen to another view of the app when pressed.

//NOTE:: Buttons should have be named following the syntax NAME_Button
//Script will find a screen for each button that matches NAME_Screen
namespace UI_Builder
{
    [AddComponentMenu("App_Button_Editor")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class UIB_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum UIB_Button_Activates
        {
            None,
            Page,
            SubMenu,
            SpecificPage,
            Video,
            Website,
            Accessibletext,
            Scene,
            InAppUrl
        }

        //        public static InAppBrowser.DisplayOptions options;

        public Resolution resolution;

        public bool isBackButton;
        public static Image backgroundImage;
        public static Sprite OG_Background;
        public Sprite Special_Background;

        public GameObject newScreen;
        public GameObject VO_Select;
        public UIB_Button_Activates Button_Opens;
        public GameObject buttonText;
        public string s_link;
        public string Title;

        private Color originalColor;

        void Start()
        {
            foreach (TextMeshProUGUI tmpg in GetComponentsInChildren<TextMeshProUGUI>())
            {
                buttonText = tmpg.gameObject;
            }
            if (buttonText == null && GetComponent<Button>().image == null)
            {
                Debug.LogError("no buttonText " + gameObject.name);
            }
            Init();
        }

        void Update()
        {
            var str = "_Button";
            if (name.Length < str.Length)
                name = name += str;
            if (name.Substring(name.Length - str.Length, str.Length) != str)
            {
                name += str;
            }
            Init();
        }



        public void Init()
        {
            //set scale of button based on resolution
            var width = GetComponent<RectTransform>().rect.width;
            var height = GetComponent<RectTransform>().rect.height;




            if (resolution == null)
                resolution = Resources.Load("ScriptableObjects/ResolutionAsset") as Resolution;



            if (gameObject.name == "App_SubMenuButton")
            {
                return;
            }
            if (Button_Opens == UIB_Button_Activates.None)
            {
                return;
            }

            var screenName = gameObject.name.ToString().Split('_')[0];
            var typeName = Button_Opens.ToString().Replace(" ", "");
            screenName = screenName + ("_" + typeName);
            var PageObject = GameObject.Find(screenName);

            if (PageObject != null)
            {
                newScreen = PageObject;
            }

            var myBtn = GetComponent<UnityEngine.UI.Button>();
            if (myBtn != null)
            {
                myBtn.onClick.AddListener(OnButtonPressed);
            }
            else
                Debug.LogWarning(gameObject.name + ": There is no button component on this UI element. It cannot use the App_Button script without a button");

            if (Button_Opens == UIB_Button_Activates.Video)
            {
                newScreen = GameObject.FindWithTag("App_VideoPlayer");
            }
        }

        void OnButtonPressed()
        {
            bool shouldDeActivatePage = true;
            switch (Button_Opens)
            {
                case UIB_Button_Activates.Page:
                    //if the new page is a template, we want to keep the current page on screen (this way the object pool won't be cleaned up)
                    shouldDeActivatePage = true;
                    try
                    {
                        shouldDeActivatePage = !newScreen.GetComponent<UIB_Page>().isTemplate;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Problem " + e);
                    }
                    UIB_PageManager.CurrentPage = newScreen;
                    newScreen.GetComponent<UIB_Page>().StartCoroutine("MoveScreenIn", false);
                    break;
                case UIB_Button_Activates.SpecificPage:
                    shouldDeActivatePage = !newScreen.GetComponent<UIB_Page>().isTemplate;
                    newScreen.GetComponent<UIB_Page>().StartCoroutine("MoveScreenIn", false);
                    break;
                case UIB_Button_Activates.Video:
                    newScreen.GetComponent<UIB_Page>().StartCoroutine("MoveScreenIn", false);
                    break;
                case UIB_Button_Activates.Website:
                    shouldDeActivatePage = false;
                    if (s_link != null)
                        Application.OpenURL(s_link);
                    else
                        Debug.LogWarning("Button not assigned a url");
                    break;
                case UIB_Button_Activates.InAppUrl:
                    shouldDeActivatePage = false;
                    if (s_link != null)
                    {

                    }
                    else
                        Debug.LogWarning("Button not assigned a url");
                    break;
                case UIB_Button_Activates.Accessibletext:

                    break;
                case UIB_Button_Activates.Scene:
                    SceneManager.LoadScene(s_link);
                    break;
                default:
                    Debug.Log("No Activity for this button");
                    break;
            }

            if (shouldDeActivatePage)
            {
                UIB_PageManager.LastPage = GetComponentInParent<UIB_Page>().gameObject;
                GetComponentInParent<UIB_Page>().DeActivate();
            }


            if (VO_Select != null)
            {
            }

        }

        public void SetButtonText(string newtext)
        {
            GetComponentInChildren<TextMeshProUGUI>().text = newtext;
        }

        private void OnEnable()
        {
            //   GetComponentInChildren<TextMeshProUGUI>().color = originalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetupButtonColors();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResetButtonColors();

        }

        public void SetVO(GameObject target)
        {
            VO_Select = target;
        }

        public void SetupButtonColors()
        {
            //Changes the color to white or green depending on the button type

            UIB_ScrollingMenu setupComp = null;
            foreach (UIB_ScrollingMenu uibsm in GetComponentsInParent<UIB_ScrollingMenu>())
            {
                setupComp = uibsm;
                break;
            }
            if (setupComp == null)
                return;

            Color32 highlightColor = setupComp.TextHighlightColor.Value;
            if (GetComponent<UnityEngine.UI.Button>().image.sprite != null)
                originalColor = GetComponent<UnityEngine.UI.Button>().image.color;
            else
                originalColor = setupComp.TextColor.Value;

            if (buttonText != null)
            {
                //  originalColor = buttonText.GetComponent<TextMeshProUGUI>().color;
                buttonText.GetComponent<TextMeshProUGUI>().color = highlightColor;
            }

            //The first button in the first menu is set to be green, regardless of mouse pointer
            //SetDisplayedButton();
        }

        public void ResetButtonColors()
        {
            UIB_ScrollingMenu setupComp = null;
            foreach (UIB_ScrollingMenu uibsm in GetComponentsInParent<UIB_ScrollingMenu>())
            {
                setupComp = uibsm;
                break;
            }

            //if there is an image, we don't want to affect the color of it.
            if (GetComponent<UnityEngine.UI.Button>().image.sprite != null)
                GetComponent<UnityEngine.UI.Button>().image.color = new Color(255, 255, 255, 255);

            if (buttonText != null)
                buttonText.GetComponent<TextMeshProUGUI>().color = setupComp.TextColor.Value;

            //SetDisplayedButton();
        }
    }
}