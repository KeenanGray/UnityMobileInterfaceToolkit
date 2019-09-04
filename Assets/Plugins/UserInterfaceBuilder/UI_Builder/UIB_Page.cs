using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace UI_Builder
{
    //Page Interface
    /// <summary>
    /// Declares the "PageActivatedHandler" Function which will be used by all Pages
    /// A "Page" is an instance of an App-Screen. Pages take up the entire screen. 
    ///     
    /// </summary>
    /// Pages can be individually set to swipe in, at a custom speed, from the "Left" "Top" "Bottom" or "Right" of the screen.
    /// Specify "Instant" to have a page instantly appear on button press

    public interface UIB_IPage
    {
        //Pages are activated by button presses. 
        //A Page should be named <name of page>_Page
        //This matches a corresponding App_Button <name of page>_Button
        void Init();
        void PageActivatedHandler();
        void PageDeActivatedHandler();
    }

    //App_Page
    //App_Page implements the standard behavior for ALL pages
    //
    public class UIB_Page : MonoBehaviour, UIB_IPage
    {
        public bool isTemplate;
        public bool ActivateUAPOnEnter;

        Canvas page_Canvas;

        public delegate void Activated();
        public event Activated OnActivated;
        public delegate void DeActivated();
        public event DeActivated OnDeActivated;

        public float rate = 1.0f;
        public bool AssetBundleRequired; //Kind of a misnamed variable: has more to do with whether files have been downloaded from web

        GameObject mainCanvas;
        GameObject subCanvas;
        public static GameObject pageParent;

        //        private List<RectTransform> views;

        private RectTransform rt;

        UnityEngine.UI.Button close_button;
        private bool PageOnScreen;

        public bool GetPageOnScreen()
        {
            return PageOnScreen;
        }

        public void Init()
        {
            OnActivated += new Activated(PageActivatedHandler);
            OnDeActivated += new DeActivated(PageDeActivatedHandler);

            if (rt == null)
            {
                rt = GetComponent<RectTransform>();
            }

            if (AssetBundleRequired)
            {
                //TODO:Maybe extend this to handle missing asset bundles
            }

            rt.sizeDelta = new Vector2(UIB_AspectRatioManager.ScreenWidth, UIB_AspectRatioManager.ScreenHeight);

            //assign the close_button
            //gameobject must be named close_button and be a child of this gameobject
            foreach (UnityEngine.UI.Button b in GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                if (b.gameObject.name.Equals("close_button"))
                    close_button = b;
            }

            if (close_button != null)
            {
                close_button.onClick.AddListener(DeActivate);
            }
            else
            {
                //Debug.LogWarning(gameObject.name + ": You need to create a button named \"close_button\" as a child of this gameobject, otherwise the screen will never be closed");
            }

            #region Views
            //TODO: Someday i'll re-make the view system


            #endregion
            page_Canvas = gameObject.GetComponent<Canvas>();
            if (page_Canvas == null)
            {
                Debug.LogWarning("no canvas on this object " + name);
            }
            else
                page_Canvas.enabled = false;
        }

        private void Start()
        {
            UIB_InputManager.SwipeDelegate += SwipeHandler;
            if (PagesOnScreen == null)
                PagesOnScreen = new List<Transform>();
        }

        public void ResetOnActivated()
        {
            OnActivated = null;
        }

        #region SwipeHandler
        //Original swipe handler for Views
        //Unfortunately this code is not integrated with the Unity Accessibility plugin 
        //TODO: Update SwipeHandling + Views with UAP

        public static List<Transform> PagesOnScreen;

        public static void UpdatePagesOnScreen()
        {
            PagesOnScreen.Clear();
            foreach (Transform t in ChildTransformsSorted(pageParent.transform))
            {
                var p = t.GetComponent<UIB_Page>();
                if (p == null)
                    continue;

                if (p.PageOnScreen)
                {
                    //HACK::audio player does not have a back button so we can't swip back on it if it is on top
                    if (p.gameObject.name != "AudioPlayer_Page")
                        PagesOnScreen.Add(p.transform);
                }
            }

            if (PagesOnScreen.Count > 1)
            {
                PagesOnScreen.Reverse();
                //                Debug.Log("page on top " + PagesOnScreen[0] + " 2 " + PagesOnScreen[1]);
            }

            /*
            PagesOnScreen = new List<UIB_Page>();

            foreach (UIB_Page upage in pageParent.GetComponentsInChildren<UIB_Page>())
            {
                if (upage.PageOnScreen && upage.GetComponent<Canvas>().enabled)
                {
                    PagesOnScreen.Add(upage);
                    Debug.Log("i " + upage.transform.GetSiblingIndex());
                }
            }

            if (PagesOnScreen.Count > 0)
            {
                //sort pages by sibling index
                PagesOnScreen = PagesOnScreen.OrderBy(page => page.transform.GetSiblingIndex()).ToList();

                Debug.Log("page on top " + PagesOnScreen[0]);
            }
            */
        }

        static List<Transform> ChildTransformsSorted(Transform t)
        {
            List<Transform> sorted = new List<Transform>();
            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);

                var tPage = child.GetComponent<UIB_Page>();

                if (tPage != null)
                {
                    sorted.Add(child);
                }
                else
                {
                }

                foreach (var item in ChildTransformsSorted(child.transform))
                {
                    var tPage2 = item.GetComponent<UIB_Page>();
                    if (tPage2 != null)
                    {
                        sorted.Add(item);
                    }
                    else
                    {

                    }

                }

            }

            return sorted;
        }


        void SwipeHandler(SwipeData swipe)
        {
            if (!PageOnScreen)
            {
                return;
            }

            var touches = swipe.fingers;
            //we swipe if 1 touch && no UAP OR 2 touch and UAP
            //AND swipe is full, direction is right, and page canvas is enabled

            if (((!UAP_AccessibilityManager.IsActive() && touches == 1) || (UAP_AccessibilityManager.IsActive() && touches == 2))
            && swipe.full && swipe.dir == Direction.RIGHT && gameObject.GetComponent<Canvas>().enabled)
            {
                var minDistance = Screen.width / 2.8;

                if (Math.Abs(swipe.value) < minDistance)
                {
                    return;
                }

                if (PagesOnScreen.Count <= 0)
                {
                    // throw new Exception("NoPageException: There are no pages on the screen. This is a major problem");
                }
                //get all the buttons on the page, if it is backbutton invoke it.
                foreach (UIB_Button ub in PagesOnScreen[0].GetComponentsInChildren<UIB_Button>())
                {
                    if (ub.isBackButton)
                    {
                        ub.GetComponent<Button>().onClick.Invoke();

                        return;
                    }
                }
            }
        }

        #endregion

        #region ViewHandling
        //Views are pieces of pages that can be slid in and out. 
        //Views are under development and not used in the HLD app
        //TODO: create View Handling

        #endregion

        //When a button is pressed, the app screen will slide in at a specified rate. Rate=1.0f will move instantly reveal the screen,
        //Other rates will allow the screen to slide in from the right.
        public IEnumerator MoveScreenIn(bool initializing = false)
        {
            //if (!(InternetRequired && !UIB_PageManager.InternetActive))
            ToggleCanvas(true);

            float lerp = 0;
            var tmp = rate;

            if (initializing)
                tmp = 1;

            // while (true && !(InternetRequired && !UIB_PageManager.InternetActive))
            while (true)
            {
                rt.anchoredPosition = new Vector2(0, 0);
                lerp += tmp;
                if (rt.anchoredPosition == new Vector2(0, 0))
                {
                    break;
                }

                yield return null;
            }
            PageOnScreen = true;
            GetComponent<AspectRatioFitter>().enabled = true;

            OnActivated?.Invoke(); //should always be last

            yield break;
        }

        //Converse of "MoveScreenIn". When the close button is pressed the screen will move out.s
        public IEnumerator MoveScreenOut(bool initializing = true)
        {
            //  yield return new WaitForEndOfFrame();
            rt.anchoredPosition = new Vector3(0, 0, 0);
            var offscreenpos = rt.anchoredPosition.x + 1920;

            float lerp = 0;

            var tmp = rate;

            try
            {
                GetComponentInChildren<Canvas>().enabled = false;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            if (initializing)
            {
                rt.anchoredPosition = new Vector3(offscreenpos, 0, 0);
            }
            else
            {
                while (true)
                {
                    rt.anchoredPosition = Vector3.Lerp(rt.anchoredPosition, new Vector3(offscreenpos, 0, 0), lerp);
                    lerp += tmp;

                    if (Mathf.Approximately(offscreenpos, rt.anchoredPosition.x + UIB_AspectRatioManager.ScreenWidth) ||
                    rt.anchoredPosition.x + lerp >= GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<CanvasScaler>().referenceResolution.x)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            PageOnScreen = false;
            GetComponent<AspectRatioFitter>().enabled = false;

            //toggle the canvas at the end to prevent flicker
            ToggleCanvas(false);
            OnDeActivated?.Invoke(); //should always be last

            yield break;
        }

        public void DeActivate()
        {
            StartCoroutine("MoveScreenOut", false);
        }

        public void PageActivatedHandler()
        {
            if (AssetBundleRequired && !UIB_PageManager.InternetActive)
            {
                //TODO:REfactor this
                //if internet is necessary and we haven't downloaded the required files. do not allow access to this page
                /*
                var tmpLastPage = UIB_PageManager.LastPage;
                var go = GameObject.Find("InternetFileError_Page").GetComponent<UIB_Page>();
                go.StartCoroutine("MoveScreenIn", false);
                StartCoroutine("MoveScreenOut", true);
                UIB_PageManager.LastPage = tmpLastPage;
                return;
                */
            }

            // ActivateUAP();

            //Say the newly selected element when the page loads
            if (UAP_AccessibilityManager.IsActive())
            {
                StartCoroutine("SayNewItem");
            }

            if (ActivateUAPOnEnter)
                StartCoroutine(ResetUAP(true));

        }

        public void PageDeActivatedHandler()
        {
            if (gameObject.name == UIB_PageManager.CurrentPage.name)
            {
                try
                {
                    UAP_AccessibilityManager.GetCurrentFocusObject().gameObject.GetComponent<UAP_BaseElement>().enabled = false;
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(NullReferenceException))
                    {

                    }
                }
            }
            if (GetComponent<AccessibleUIGroupRoot>() != null)
                GetComponent<AccessibleUIGroupRoot>().m_Priority = 0;

            StartCoroutine(ResetUAP(false));
        }

        public static bool paused = false;
        public IEnumerator ResetUAP(bool toggle)
        {
            foreach (Button b in GetComponentsInChildren<Button>())
            {
                b.enabled = toggle;
            }
            foreach (UAP_BaseElement uap in GetComponentsInChildren<UAP_BaseElement>())
            {
                uap.enabled = toggle;
            }

            foreach (AccessibleUIGroupRoot agui in GetComponentsInChildren<AccessibleUIGroupRoot>())
            {
                agui.enabled = false;
                agui.enabled = true;
            }

            foreach (AccessibleUIGroupRoot ugui in GetComponentsInChildren<AccessibleUIGroupRoot>())
            {
                ugui.enabled = toggle;
            }

            if (toggle)
            {
                //select the first element
                try
                {
                    UAP_AccessibilityManager.SelectElement(UAP_AccessibilityManager.TrueFirstElement());
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            yield break;
        }

        #region Helpers
        public void SetOnScreen(bool Enabled)
        {
            PageOnScreen = Enabled;
        }

        void ToggleCanvas(bool set)
        {
            if (page_Canvas == null)
            {
                Debug.Log("No Canvas on " + name);
            }
            else
            {
                //                Debug.Log("canvas enabled " + page_Canvas.enabled);
                page_Canvas.enabled = set;
            }
        }
        #endregion

        IEnumerator SayNewItem()
        {
            if (UAP_AccessibilityManager.IsSpeaking())
                yield break;
            UAP_AccessibilityManager.StopSpeaking();
            yield return new WaitForSeconds(1.0f);
            //  UAP_AccessibilityManager.Say(GameObject.Find("Active Item Frame").GetComponentsInParent<UAP_BaseElement>()[0].m_Text);

            yield break;
        }

    }
}