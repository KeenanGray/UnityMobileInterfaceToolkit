using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_Builder
{
    public class UIB_PageManager : MonoBehaviour
    {
        public static GameObject LastPage;
        public static GameObject CurrentPage;

        public static bool InternetActive { get; set; }

        // Use this for initialization
        void Start()
        {
            CurrentPage = GameObject.Find("Landing_Page");
            UIB_InputManager.SwipeDelegate += SwipeHandler;
            UIB_Page.pageParent = GameObject.Find("Pages");
        }

        // Update is called once per frame
        void Update()
        {
            UIB_Page.UpdatePagesOnScreen();
        }

        void SwipeHandler(SwipeData swipe)
        {
            /*
            if (swipe.touches == 1 && swipe.dir == Direction.LEFT)
            {
                if (LastPage != null && swipe.full && CurrentPage.name != "Landing_Page")
                {
                    if (LastPage != null && CurrentPage!=null)
                    {
                        if (LastPage == CurrentPage)
                        {
                            Debug.Log("this is bad");
                            return;
                        }
                        LastPage.GetComponent<UIB_Page>().StartCoroutine("MoveScreenIn", false);
                        CurrentPage.GetComponent<UIB_Page>().StartCoroutine("MoveScreenOut", false);
                        Debug.Log("Should be activating the page " + LastPage.name);
                    }
                }
                LastPage.GetComponent<UIB_Page>().StartCoroutine("MoveScreenIn", false);
                */

        }

    }
}