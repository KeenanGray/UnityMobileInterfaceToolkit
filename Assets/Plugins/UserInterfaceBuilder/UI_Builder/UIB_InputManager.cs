using System;
using System.Collections;
using System.Collections.Generic;
using UI_Builder;
using UnityEngine;
using static UIB_InputManager;

public enum Direction
{
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public struct SwipeData
{
    public Direction dir;
    public float value;
    public bool full; //Whether the swipe is a complete swipe. False before user lifts finger.
    public float swipeTime;
    public int fingers;


    public SwipeData(float val, Direction direction, int touchCount, bool final = false)
    {
        value = val;
        dir = direction;
        full = final;
        swipeTime = 0;
        fingers = touchCount;
    }
    public float SwipeSpeed()
    {
        return Mathf.Abs(this.value) / this.swipeTime;
    }
}

public class UIB_InputManager : MonoBehaviour
{
    //whether or not to reset the input check, happens on mouse up (when swipe/touch ends)
    bool clearInputs;

    public delegate void OnSwipe(SwipeData swipe);
    public static event OnSwipe SwipeDelegate;

    public delegate void OnTouch(Touch[] touches, int tapCount);
    public static event OnTouch TouchDelegate;

    public delegate void OnTap(int touches, int tapCount);
    public static event OnTap TapDelegate;

    float SwipeVal;
    Direction SwipeDir;
    bool canSwipe;
    bool canTap;

    private void Start()
    {
      //  SwipeDelegate += PrintSwipeData;
      //  TapDelegate += PrintTapData;

        StartCoroutine("CheckSwipeInput");
      // StartCoroutine("ContinousSwipeDetection");
        StartCoroutine("CheckTapInput");

        UIB_AudioPlayerTools.AudioDragSelected += DisableSwipe;
        UIB_AudioPlayerTools.AudioDragDeSelected += Enableswipe;
        canSwipe = true;
        canTap = true;

        TapDelegate += TapCountHandler;

    }


    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Touch[] t = { new Touch() };
            if (canTap)
            {
                TouchDelegate(t, 1);
            }
        }
#endif
    }

    private void Enableswipe()
    {
        canSwipe = true;
    }

    private void DisableSwipe()
    {
        canSwipe = false;
    }

    private void Update()
    {

    }

    SwipeData CreateSwipe(Vector3 startPos, Vector3 endPos)
    {
        var swipe = new SwipeData();
        PrintSwipeData(swipe);
        return swipe;
    }

    void PrintSwipeData(SwipeData swipe)
    {
        if (swipe.full)
        {
            Debug.Log(swipe.fingers + " finger(s) " + swipe.dir + " swipe detected with value " + swipe.SwipeSpeed());
        }
        else
        {
            // Debug.Log("user is swiping " + swipe.dir);
        }

    }

    private void PrintTapData(int touches, int tapCount)
    {
        Debug.Log(touches + " finger " + tapCount + " taps!");
    }

    IEnumerator CheckSwipeInput()
    {
        Vector2 firstPoint;
        Vector2 secondPoint;

        float StartTime = 0;
        float EndTime = 0;

        firstPoint = Vector3.negativeInfinity;
        secondPoint = Vector3.negativeInfinity;
        //First check for full swipes.
        var fingercount = 0;

        while (true)
        {
            //Get the initial touch point
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                fingercount = Input.touchCount;

                if (fingercount > 0)
                {
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        clearInputs = false;
                        firstPoint = Input.touches[0].position;
                        StartTime = Time.time;
                    }
                }
#if UNITY_EDITOR
                clearInputs = false;
                firstPoint = Input.mousePosition;
                StartTime = Time.time;
#endif
            }

#if UNITY_EDITOR
            fingercount = 1;
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                fingercount = 2;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                fingercount = 3;
            }
#endif
            //Get the second touch point
            if (Input.GetMouseButtonUp(0))
            {
                clearInputs = true;
                secondPoint = Input.mousePosition;
                EndTime = Time.time;
            }

            //Determine the direction of the swipe
            if (firstPoint.x < -UIB_AspectRatioManager.ScreenWidth || secondPoint.x < -UIB_AspectRatioManager.ScreenWidth)
            {
                //no swipe
            }
            else
            {
                //swipe
                var diff = secondPoint - firstPoint;

                //disallow diagonal swiping
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                {
                    //left or right swipe
                    SwipeVal = diff.x;
                    if (SwipeVal > 0)
                        SwipeDir = Direction.RIGHT;
                    else
                        SwipeDir = Direction.LEFT;
                }
                else
                {
                    //up or down swipe
                    SwipeVal = diff.y;
                    if (SwipeVal > 0)
                        SwipeDir = Direction.UP;
                    else
                        SwipeDir = Direction.DOWN;
                }

                //Create a new swipe delegate so that other functions can handle the fullswipe         
                SwipeData FinalSwipe = new SwipeData(SwipeVal, SwipeDir, fingercount, true);
                FinalSwipe.swipeTime = EndTime - StartTime;

                fingercount = 0;

                if (canSwipe)
                {
                    //turn off tap briefly if big enough swipe
                    if (Mathf.Abs(FinalSwipe.value) > 100)
                    {
                        SwipeDelegate(FinalSwipe);
                        tapCD = 0.5f;
                    }
                }
            }

            if (clearInputs)
            {
                firstPoint = Vector3.negativeInfinity;
                secondPoint = Vector3.negativeInfinity;
            }

            yield return null;
        }
    }

#region continousSwipeDetection
    IEnumerator ContinousSwipeDetection()
    {
        var touches = 0;

        while (true)
        {
            //While the mouse is held down
            //We need to determine the direction the user is swiping continously

            Vector2 current;
            Vector2 last;

            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    //Debug.Log("Began touch with " + Input.touchCount);
                }

                touches = Input.touchCount;
                current = Input.mousePosition;
                yield return new WaitForFixedUpdate();
                last = Input.mousePosition;

                //swipe
                var diff = last - current;

                //disallow diagonal swiping
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                {
                    //left or right swipe
                    SwipeVal = diff.x;
                    if (SwipeVal > 0)
                        SwipeDir = Direction.RIGHT;
                    else
                        SwipeDir = Direction.LEFT;
                }
                else
                {
                    //up or down swipe
                    SwipeVal = diff.y;
                    if (SwipeVal > 0)
                        SwipeDir = Direction.UP;
                    else
                        SwipeDir = Direction.DOWN;
                }

#if UNITY_EDITOR
                touches++;
#endif
                //Create a new swipe delegate so that other functions can handle the fullswipe
                if (canSwipe)
                    SwipeDelegate(new SwipeData(SwipeVal, SwipeDir, touches));
            }

            yield return null;
        }
    }
#endregion
    float tapCD = 0.5f;
    IEnumerator CheckTapInput()
    {
        var touchDelay = new WaitForEndOfFrame();// new WaitForSeconds(.15f);
        bool shouldExecute = false;
        float cooldown = 0.5f;
        int tapCount = 0;
        int touches = 0;
        int marked = 0;

        while (true)
        {
            shouldExecute = false;

            if (canTap && (Input.touchCount > 0 || Input.GetMouseButtonDown(0)))
            {
                if (Input.touchCount > 0)
                {
                    if (Input.touches[0].phase == TouchPhase.Began)
                    {
                        touches = Input.touchCount;
                        shouldExecute = true;
                    }
                }
#if UNITY_EDITOR
                if (Input.GetMouseButtonDown(0))
                {
                    shouldExecute = true;
                    touches = 1;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                    touches++;
                if (Input.GetKey(KeyCode.LeftAlt))
                    touches++;
#endif
                if (shouldExecute)
                {
                    marked = touches;

                    if (cooldown > 0 && tapCount == 1)
                    {
                        //Has double tapped
                        TapDelegate(touches, 2);
                        marked = 0;
                        tapCount = 0;
                        yield return touchDelay;
                    }
                    else
                    {
                        cooldown = 0.5f;
                        tapCount += 1;
                    }
                }
            }

            if (cooldown > 0)
            {
                cooldown -= 1f * Time.deltaTime;
            }
            else
            {
                tapCount = 0;

                if (tapCD > 0)
                {
                    tapCD -= 1f * Time.deltaTime;
                    //we swiped too recently, abort the tap
                    marked = 0;
                }
                else
                {
                    if (marked > 0)
                    {
                        TapDelegate(marked, 1);
                        yield return touchDelay;
                    }
                    marked = 0;
                }
            }

            yield return null;
        }
    }

    private void TouchHandler(Touch[] touches, int taps)
    {
        throw new NotImplementedException();
    }

    private void TapCountHandler(int touches, int taps)
    {
        var fingers = touches;

        if (fingers == 2 && taps == 2)
        {
            //  UAP_AccessibilityManager.StopSpeaking();

            UAP_AccessibilityManager.Say(" \n\r");
            GameObject.Find("Accessibility Manager").GetComponent<UAP_AccessibilityManager>().SayPause(.1f);
            
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("ios");
            iOSTTS.StopSpeaking();
            iOSTTS.StopSpeaking();
            iOSTTS.Shutdown();
            iOSTTS.Shutdown();
#endif

        }
        if (fingers == 3 && taps == 1)
        {
            // Debug.Log("repeating from " + UAP_AccessibilityManager.GetCurrentFocusObject().name);
          //  UAP_AccessibilityManager.Say(UAP_AccessibilityManager.GetCurrentFocusObject().GetComponent<UAP_BaseElement>().m_);
            UAP_AccessibilityManager.GetCurrentFocusObject().GetComponent<UAP_BaseElement>().SelectItem(true);
        }
    }
}
