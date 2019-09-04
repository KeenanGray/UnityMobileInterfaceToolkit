using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UI_Builder;

public class UIB_AudioPlayerTools : MonoBehaviour
{
    public delegate void BeginDrag();
    public static event BeginDrag AudioDragSelected;

    public delegate void EndDrag();
    public static event EndDrag AudioDragDeSelected;

    public AudioSource source;
    Button playbutton;
    Button backButton;
    Button fwdButton;
    Text time_label;
    TextMeshProUGUI maxtime_label;
    InputField AudioTimerInput;

    private GameObject frame;
    private Vector2 initPos;
    private Vector2 initSize;
    private Vector2 moveDist;

    string oldValue;
    bool hasMoved;

    GameObject AudioPlayerData;

    Image playImage;
    Image pauseImage;

    Scrollbar timeScroll;

    bool trig;
    private bool DragOccurring;

    Transform ParentOfAudioToolComponents;

    // Use this for initialization
    public void Init()
    {
        oldValue = "";
        frame = GetComponentInParent<Mask>().gameObject;
        initSize = frame.GetComponent<RectTransform>().sizeDelta;
        initPos = frame.GetComponent<RectTransform>().localPosition;
        ParentOfAudioToolComponents = transform.parent;

        source = gameObject.GetComponentInChildren<AudioSource>();

        //Set up buttons for audiocontroller
        foreach (Button b in ParentOfAudioToolComponents.GetComponentsInChildren<Button>())
        {
            if (b.gameObject.name.Equals("Play"))
                playbutton = b;
        }
        if (playbutton != null)
        {
            playbutton.onClick.RemoveAllListeners();

            playbutton.onClick.AddListener(PlayButtonPressed);
            playbutton.transform.GetChild(0).gameObject.SetActive(true); //turn on the play button
            playbutton.transform.GetChild(1).gameObject.SetActive(false); //turn off the pause button
        }
        else
        {
            Debug.LogWarning("No play button");
        }

        foreach (Button b in ParentOfAudioToolComponents.GetComponentsInChildren<Button>())
        {
            if (b.gameObject.name.Equals("Back"))
                backButton = b;
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackButtonPressed);
        }
        else
        {
            Debug.LogWarning("No Back button button");
        }

        foreach (Button b in ParentOfAudioToolComponents.GetComponentsInChildren<Button>())
        {
            if (b.gameObject.name.Equals("Forward"))
                fwdButton = b;
        }
        if (fwdButton != null)
        {
            fwdButton.onClick.RemoveAllListeners();
            fwdButton.onClick.AddListener(FwdButtonPressed);
        }
        else
        {
            Debug.LogWarning("No fwd button");
        }

        AudioPlayerData = GameObject.Find("AudioPlayer_Page");

        //Set up scrollbar for audio controls
        foreach (Scrollbar sb in ParentOfAudioToolComponents.GetComponentsInChildren<Scrollbar>())
        {
            if (sb.gameObject.name.Equals("Time_Scroll"))
            {
                if (source == null || source.clip==null)
                {
                   // Debug.Log("no source 2 " +gameObject.name);
                    return;
                }
                timeScroll = sb;
                var t = Mathf.InverseLerp(0, source.clip.length, source.time);

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.InitializePotentialDrag;
                entry.callback.AddListener((eventData) => { OnDragBegin(); });
                timeScroll.GetComponent<EventTrigger>().triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback.AddListener((eventData) => { OnDragEnd(); });
                timeScroll.GetComponent<EventTrigger>().triggers.Add(entry);
            }
        }
        if (timeScroll != null)
        {

        }
        else
        {
            Debug.LogWarning("No time scroll button");
        }

        /*  //Add time field for audio length
          foreach (Text tl in transform.parent.GetComponentsInChildren<Text>())
          {
              if (tl.gameObject.name.Contains("DisplayText"))
                  time_label = tl;
          }
          if (time_label != null)
          {

          }
          else
          {
              Debug.LogWarning("No time_label");
          }
          */

        //Add time field for audio length
        foreach (TextMeshProUGUI tl in ParentOfAudioToolComponents.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (tl.gameObject.name.Contains("MaxTime_Text"))
                maxtime_label = tl;
        }
        if (maxtime_label != null)
        {
            maxtime_label.text = ConvertToClockTime(source.clip.length);
        }
        else
        {
            Debug.LogWarning("No maxtime_label");
        }

        //Set up the input field
        AudioTimerInput = ParentOfAudioToolComponents.GetComponentInChildren<InputField>();

      // AudioTimerInput.shouldHideMobileInput = true;
        AudioTimerInput.keyboardType = TouchScreenKeyboardType.NumberPad;

        time_label = GameObject.Find("DisplayText noedit").GetComponent<Text>();
        if (time_label == null)
            Debug.LogWarning("Uh Oh gameObject is missing");

        AudioTimerInput.onValueChanged.AddListener(OnInputFieldChanged);
        AudioTimerInput.onEndEdit.AddListener(OnInputFieldSubmitted);
        AudioTimerInput.onEndEdit.AddListener(fieldDeSelected);

        // AudioTimerInput.OnSubmit.AddListener(OnInputFieldSubmitted);

    }

    public void LoadtimeCodeToPrefs()
    {
        if (PlayerPrefs.HasKey("desc_timecode"))
        {
            source.Play();
            source.time = PlayerPrefs.GetInt("desc_timecode");
            source.Pause();
        }
    }

    public void SavetimeCodeToPrefs()
    {
        var outstr = "";
        PlayerPrefs.SetInt("desc_timecode", StringToSecondsCount(AudioTimerInput.text, ref outstr));
    }

    private void OnInputFieldSubmitted(string arg0)
    {
        string str = time_label.text.Split(':')[0] + time_label.text.Split(':')[1];
        
        //AudioTimerInput.text = "";
        if (source.clip.length > StringToSecondsCount(str, ref arg0))
        {
            source.time = StringToSecondsCount(str, ref arg0);
        }
        else
        {
            Debug.LogWarning("length exceeds time remaining in clip");
        }

        AudioTimerInput.DeactivateInputField();

        if (GetComponentInParent<UIB_Page>().gameObject.GetComponent<Canvas>().isActiveAndEnabled)
        {
            PlayMethod(1);
        }

        if (TouchScreenKeyboard.isSupported)
        {
            if (AudioTimerInput.touchScreenKeyboard.status == TouchScreenKeyboard.Status.Done)
            {
                PlayMethod(1);
            }
        }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        StartCoroutine("SetInputPositionBack");
#endif
        //TODO:deselect the input field
    }

    int timerIndex;
    private void OnInputFieldChanged(string arg0)
    {
        UAP_AccessibilityManager.BlockInput(true);

        var currentVal = arg0;
        if (currentVal.Length > oldValue.Length)
        {
            if (currentVal.Length <= 5)
            {
                //say character added and full sentence
                UAP_AccessibilityManager.Say(currentVal[currentVal.Length - 1].ToString() + " added ", true, true, UAP_AudioQueue.EInterrupt.All);
            }
            else
            {
                // UAP_AccessibilityManager.Say(currentVal, true, true, UAP_AudioQueue.EInterrupt.All);
            }
        }
        else
        {
            //say character deleted
            if (UAP_AccessibilityManager.IsActive())
            {
                UAP_AccessibilityManager.Say(oldValue[oldValue.Length - 1].ToString() + " deleted", true, true, UAP_AudioQueue.EInterrupt.All);
            }
        }
        oldValue = currentVal;

        var outstr = "";
        time_label.text = ConvertToClockTime(StringToSecondsCount(AudioTimerInput.text, ref outstr));
        UAP_AccessibilityManager.Say(AudioTimerInput.GetComponent<Special_AccessibleTimeCode>().GetTargetGameObject().GetComponent<Text>().text);

        if (!AudioTimerInput.isFocused)
            UAP_AccessibilityManager.SelectElement(UAP_AccessibilityManager.GetCurrentFocusObject());

    }

    // Update is called once per frame
    void Update()
    {
        if (source == null || source.clip == null)
        {
           // Debug.LogWarning("no source " + gameObject.name);
            return;
        }

        if (time_label != null && !AudioTimerInput.isFocused)
            time_label.text = ConvertToClockTime(source.time);

        if (timeScroll != null && !DragOccurring)
        {
            timeScroll.value = Mathf.InverseLerp(0, source.clip.length, source.time);
        }
        else
        {
            if (source != null)
                time_label.text = ConvertToClockTime(Mathf.Lerp(source.clip.length, source.time, 1 - timeScroll.value));
        }

        if (source != null)
        {
            //detect end of audio clip
            if (float.Equals(source.time, source.clip.length))
            {
                OnAudioClipEnd();
            }
        }
    }

    void OnAudioClipEnd()
    {
        timeScroll.value = 0;
        source.time = 0;
        if (source.isPlaying)
            PlayButtonPressed();
        playbutton.transform.GetChild(1).gameObject.SetActive(false); //turn off the play button
        playbutton.transform.GetChild(0).gameObject.SetActive(true); //turn on the pause button
    }

    void OnScrollValueChanged()
    {
        source.time = Mathf.Lerp(0, source.clip.length, timeScroll.value);
    }

    public void PlayButtonPressed()
    {
        PlayMethod();
    }


    ///<summary>PlayMethod A lot has to happen when we play the audio : Use this instead of "AudioSource.Play"  
    /// for best results 
    /// the parameter lets you set a flag:
    /// 0 - > default behavior toggle
    /// 1 - > force audio to startup play  
    /// 2 - > force audio to stop playing and cleanup</summary>  
    public void PlayMethod(int option = 0)
    {
        if (source == null)
        {
#if !UNITY_EDITOR
            Debug.LogWarning("Warning: No Audio Source to Play");
#endif
            return;
        }
        switch (option)
        {
            case 0:
                //Toggle
                if (source.isPlaying)
                {
                    PlayHelperStop();
                }
                else if (!source.isPlaying)
                {
                    PlayHelperStart();
                }
                break;
            case 1:
                //Force Start
                //Debug.Log("Force to play");
                PlayHelperStart();
                break;
            case 2:
                //Force Stop
                //Debug.Log("Force to stop");
                PlayHelperStop();
                break;
            default:
                Debug.LogError("Invalid Parameter: Flag must be either 0,1, or 2 ");
                break;
        }
    }

    void PlayHelperStart()
    {
        source.Play();
        playbutton.transform.GetChild(0).gameObject.SetActive(false); //turn off the play button
        playbutton.transform.GetChild(1).gameObject.SetActive(true); //turn on the pause button
        var sab = playbutton.GetComponent<Special_AccessibleButton>();
        sab.m_Text = "Pause";
        sab.SelectItem(true);

        //start the captions
        AudioPlayerData.GetComponent<UIB_AudioPlayer>().StartCoroutine("PlayCaptionsWithAudio");
        return;
    }
    void PlayHelperStop()
    {
        source.Pause();
        playbutton.transform.GetChild(0).gameObject.SetActive(true); //turn off the play button
        playbutton.transform.GetChild(1).gameObject.SetActive(false); //turn on the pause button
        var sab = playbutton.GetComponent<Special_AccessibleButton>();
        sab.m_Text = "Play";
        sab.SelectItem(true);

        //Stop the captions
        AudioPlayerData.GetComponent<UIB_AudioPlayer>().StopCoroutine("PlayCaptionsWithAudio");
        return;
    }

    void FwdButtonPressed()
    {
        if (source.time < source.clip.length - 30)
        {
            source.time += 30;
        }
        else
        {
            source.time = source.clip.length - .01f;
        }
        AudioPlayerData.GetComponent<UIB_AudioPlayer>().SetCaptionsStart();

    }

    void BackButtonPressed()
    {
        if (source.time > 30)
            source.time -= 30;
        else
            source.time = 0;

        AudioPlayerData.GetComponent<UIB_AudioPlayer>().SetCaptionsStart();

    }

    string ConvertToClockTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);

        string secondsAsString;
        string minutesAsString;

        if (seconds < 10)
            secondsAsString = "0" + seconds;
        else
            secondsAsString = "" + seconds;

        if (minutes < 10)
            minutesAsString = "0" + minutes;
        else
            minutesAsString = "" + minutes;

        return minutesAsString + ":" + secondsAsString;
    }

    private int StringToSecondsCount(string v, ref string outStr)
    {
        var min = "00";
        var sec = "00";

        switch (v.Length)
        {
            case 0:
                min = "00";
                sec = "00";
                break;
            case 1:
                min = "00";
                sec = "0" + v;
                break;
            case 2:
                min = "00";
                sec = v;
                break;
            case 3:
                min = "0" + v[0];
                sec = v[1] + "" + v[2];
                break;
            case 4:
                min = v[0] + "" + v[1];
                sec = v[2] + "" + v[3];
                break;
            default:
                min = v[0] + "" + v[1];
                sec = v[2] + "" + v[3];
                //AudioTimerInput.OnSelect(null);
                AudioTimerInput.DeactivateInputField();
                //  OnInputFieldSubmitted(min + ":" + sec + v);
                break;
        }
        outStr = min + ":" + sec + v;

        try
        {
            return (int.Parse(min) * 60) + int.Parse(sec);
        }
        catch (Exception e)
        {
            Debug.Log("Exception" + e);
            return ((int)source.time);
        }
    }

    public void OnDragBegin()
    {
        AudioDragSelected();

        if (source.isPlaying)
        {
            DragOccurring = true;
        }
    }

    public void OnDragEnd()
    {
        StopCoroutine("DeselectscrollBar");
        StartCoroutine("DeselectscrollBar");

        DragOccurring = false;
        if (timeScroll.value < 1)
            source.time = Mathf.Lerp(0, source.clip.length, timeScroll.value);
        else
            source.time = source.clip.length - .1f;

        if (!source.isPlaying)
        {
            PlayMethod(1);
            DragOccurring = false;
        }

        AudioPlayerData.GetComponent<UIB_AudioPlayer>().SetCaptionsStart();

    }

    IEnumerator DeselectscrollBar()
    {
        yield return new WaitForSeconds(0.5f);

        AudioDragDeSelected();

        yield break;
    }

    public void OnSelect(BaseEventData eventData)
    {
//        Debug.Log("HERE");
        fieldSelected();
    }
    private void fieldSelected()
    {
        StartCoroutine("fieldSelectedCo");
    }

    IEnumerator fieldSelectedCo()
    {
        hasMoved = false;

        //move the text field up so it is not obscured by keyboard
        var h = 0f;
        if (TouchScreenKeyboard.isSupported)
        {
            while (!TouchScreenKeyboard.visible)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Mobile keyboard not supported");
#endif
        }

        h = TouchScreenKeyboard.area.height;

#if UNITY_EDITOR
        h = 873; //iphone X
        h = 264; //ipad 12-9
#endif
        //we have to change the mask size in case movement causes colision with logo and back button
        var sizeAdjust = new Vector2(0, GetComponent<RectTransform>().rect.height * 1.9f);
        frame.GetComponent<RectTransform>().sizeDelta -= sizeAdjust;
        moveDist = new Vector2(0, h);

        //set to elevated position;
        frame.GetComponent<RectTransform>().anchoredPosition += moveDist;
        hasMoved = true;
        yield break;
    }

    private void fieldDeSelected(string arg0)
    {
        UAP_AccessibilityManager.BlockInput(false);


        if (TouchScreenKeyboard.isSupported)
        {
            if (hasMoved && !TouchScreenKeyboard.visible)
            {
                //set back to initial position;
                frame.GetComponent<RectTransform>().localPosition = initPos;
                frame.GetComponent<RectTransform>().sizeDelta = new Vector2(initSize.x, initSize.y);
                hasMoved = false;
            }
        }
        else
        {
            //set back to initial position;
            frame.GetComponent<RectTransform>().localPosition = initPos;
            frame.GetComponent<RectTransform>().sizeDelta = new Vector2(initSize.x, initSize.y);
            hasMoved = false;
        }
    }

    IEnumerator SetInputPositionBack()
    {
        while (true)
        {
            Debug.Log("THIS IS HAPPENING");
            if (TouchScreenKeyboard.isSupported && TouchScreenKeyboard.visible)
            {

                yield return null;
            }
            else
            {
                //set back to initial position;
                frame.GetComponent<RectTransform>().localPosition = initPos;
                frame.GetComponent<RectTransform>().sizeDelta = new Vector2(initSize.x, initSize.y);
                hasMoved = false;
                yield break;
            }
        }
    }
}
