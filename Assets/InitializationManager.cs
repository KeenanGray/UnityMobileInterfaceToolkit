using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UI_Builder;
using UnityEngine;
using UnityEngine.UI;

public class InitializationManager : MonoBehaviour
{
    GameObject aspectManager;

    GameObject AccessibilityInstructions;
    GameObject blankPage;

    public float InitializeTime;
    float t1;
    float t2;

    public static float DownloadCount = 0;
    public static float TotalDownloads { get; private set; }

    public static int checkingForUpdates = 0;
    public static float PercentDownloaded = 0;

    public bool DebugLocalAssetBundles;

    public TextMeshProUGUI percentText;

    private bool hasCheckedFiles;


    void Start()
    {
#if (!UNITY_EDITOR)
        DebugLocalAssetBundles=false;
#endif
#if UNITY_EDITOR
        UIB_AspectRatioManager_Editor.Instance().IsInEditor = false;
#endif
        StartCoroutine("Init");
    }

    private void Update()
    {
        if (UIB_FileManager.HasUpdatedAFile && DownloadCount <= 0)
        {
            PlayerPrefs.SetString("LastUpdated", DateTime.UtcNow.ToString());
            UIB_FileManager.HasUpdatedAFile = false;
        }
    }

    IEnumerator Init()
    {
        //set T1 for timing Init;
        t1 = Time.time;
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        UIB_PlatformManager.Init();

        try
        {
            blankPage = GameObject.Find("BlankPage");
        }
        catch (Exception e)
        {
            Debug.Log("No blankpage " + e);
            yield break;
        }
        try
        {
            aspectManager = GameObject.FindGameObjectWithTag("MainCanvas");
        }
        catch (Exception e)
        {
            Debug.Log("no aspect ratio manager " + e);
            yield break;
        }

        try
        {
            AccessibilityInstructions = GameObject.Find("AccessibleInstructions_Button");
        }
        catch (Exception e)
        {
            Debug.Log("no instructions " + e);
            yield break;
        }

        //this coroutine checks the local files and starts any necessary downloads
        StartCoroutine("CheckLocalFiles");

        //this coroutine continously checks if we have wifi and downloads are happening
        //it updates the download icon accordingly
        StartCoroutine("CheckWifiAndDownloads");

        //this coroutine updates download percentage over time
        StartCoroutine("UpdateDownloadPercent");

        //this coroutine waits until we have checked for all the files
        //then it begins loading asset bundles in the background
        //it must be started after pages have initialized
        StartCoroutine("ManageAssetBundleFiles");

        //Set the main page container
        //Can't remember why i did this
        UIB_PageContainer MainContainer = null;
        foreach (UIB_PageContainer PageContainer in GetComponentsInChildren<UIB_PageContainer>())
        {
            MainContainer = PageContainer;
            MainContainer.Init();
        }

        //set scroll rects to top
        foreach (Scrollbar sb in GetComponentsInChildren<Scrollbar>())
        {
            sb.value = 1;
        }

        //turn aspect ratio fitters on
        //causes all pages to share origin with canvas and be correct dimensions
        foreach (AspectRatioFitter arf in GetComponentsInChildren<AspectRatioFitter>())
        {
            arf.aspectRatio = (UIB_AspectRatioManager.ScreenWidth) / (UIB_AspectRatioManager.ScreenHeight);
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.enabled = true;
        }

        //initialize each button
        foreach (UI_Builder.UIB_Button ab in GetComponentsInChildren<UI_Builder.UIB_Button>())
        {
            //before initializing buttons, we may change some names based on player_prefs
            /* if (ab.name == "Displayed-Code_Button")
                 CheckAndUpdateLinks("Displayed-Info_Page");
             if (ab.name == "OnDisplay-Code_Button")
                 CheckAndUpdateLinks("OnDisplay-Info_Page");
             if (ab.name == "Unfinished-Code_Button")
                 CheckAndUpdateLinks("Unfinished-Info_Page");
                 */
            ab.Init();
        }

        //initialize each page
        foreach (UIB_IPage p in GetComponentsInChildren<UIB_IPage>())
        {
            p.Init();
        }

        foreach (UIB_Page p in GetComponentsInChildren<UIB_Page>())
        {
            //TODO:Fix this bad bad shit
            if (p.gameObject.name == "Landing_Page")
                yield return p.MoveScreenOut(true);
            else
            {
                p.StartCoroutine("MoveScreenOut", true);
            }
        }

        //initialize each scrolling menu
        foreach (UIB_ScrollingMenu uibSM in GetComponentsInChildren<UIB_ScrollingMenu>())
        {
            uibSM.Init();
        }



        //setup the first screen
        var firstScreen = GameObject.Find("Landing_Page");
        yield return firstScreen.GetComponent<UIB_Page>().StartCoroutine("MoveScreenIn", true);

        //remove the cover
        MainContainer.DisableCover();

        //if we finish initializing faster than expected, take a moment to finish the video
        t2 = Time.time;
        var elapsed = t2 - t1;
        if (InitializeTime > elapsed)
            yield return new WaitForSeconds(InitializeTime - elapsed);
        else if (Mathf.Approximately(InitializeTime, float.Epsilon))
            Debug.Log("took " + elapsed + "s to initialize");
        else
            Debug.LogWarning("Took longer to initialize than expected");

        yield break;
    }



    internal static void ReloadAssetBundle(string filename)
    {
        GameObject.Find("MainCanvas").GetComponent<UIB_AssetBundleHelper>().RefreshBundle(filename);
    }

    private void TryDownloadFile(string filename, bool fallbackUsingBundle = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
         if (!(UIB_FileManager.FileExists(UIB_PlatformManager.persistentDataPath +"android/assets/"+ UIB_PlatformManager.platform + filename)))
        {
            //We don't have the file, first thing is to copy it from streaming assets
            //On Android, streaming assets are zipped so we need a special accessor

            GameObject.Find("FileManager").GetComponent<UIB_FileManager>().StartCoroutine("CreateStreamingAssetDirectories", filename);
            //record here that we have never updated files from the internet;
            PlayerPrefs.SetString("LastUpdated", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString());

            if (CheckInternet()&& !DebugLocalAssetBundles)
            {
                print("Just copied all asset bundle files, checking for update");
                db_Manager.CheckIfObjectHasUpdate(UIB_PlatformManager.platform  + filename, "heidi-latsky-dance");
            }
            else
            {
                
            }
        }
        else{
         //we have the file check for update
            if (CheckInternet() && !DebugLocalAssetBundles)
            {
                print("we have the file checking for update");
                db_Manager.CheckIfObjectHasUpdate(UIB_PlatformManager.platform + filename, "heidi-latsky-dance");
            }
        }

#else
        if (!(UIB_FileManager.FileExists(UIB_PlatformManager.persistentDataPath + UIB_PlatformManager.platform + filename)))
        {
            //we don't have the file, firs thing to do is copy it from streaming assets
            UIB_FileManager.WriteFromStreamingToPersistent(filename);
            // AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + UIB_PlatformManager.platform + filename);

            //record here that we have never updated files from the internet;
            PlayerPrefs.SetString("LastUpdated", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString());

            if (CheckInternet() && !DebugLocalAssetBundles)
            {
                //db_Manager.CheckIfObjectHasUpdate(UIB_PlatformManager.platform + filename, "heidi-latsky-dance");
            }
            else
            {

            }
        }
        else
        {
            //we have the file check for update
            if (CheckInternet() && !DebugLocalAssetBundles)
            {
                //db_Manager.CheckIfObjectHasUpdate(UIB_PlatformManager.platform + filename, "heidi-latsky-dance");
            }


        }

#endif
        //delete the streaming asset files
        UIB_FileManager.DeleteFile(filename);
        UIB_AssetBundleHelper.InsertAssetBundle(filename);

    }

    private void ActivateLimitedFunctionality()
    {
        //TODO: refactor this
        /*
        //Bring up no internet logo. 
        UIB_PageManager.InternetActive = false;

        tmpLandingPage = GameObject.Find("NoInternetCriticalLanding");
        */
    }

    private void DownloadFileFromDatabase(string fName, bool fallbackUsingBundle = false)
    {
        //TODO: Alert the user we are about to begin a large download
        //How often can we call this download function before it costs too much $$$
        //db_Manager.GetObjectFromBucketByName(name, "heidi-latsky-dance");
        if (fallbackUsingBundle)
        {
            //db_Manager.GetObjectWithFallback(fName, "heidi-latsky-dance");
        }
        else { }
        //db_Manager.GetObject(fName, "heidi-latsky-dance");
    }

    private bool CheckInternet()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                UIB_PageManager.InternetActive = false;
                return false;
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                UIB_PageManager.InternetActive = true;
                return true;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                UIB_PageManager.InternetActive = true;
                return true;
        }
        return false;
    }

    private string[] GetListOfDancers()
    {
        AssetBundle tmp = null;
        string[] list;

        foreach (AssetBundle b in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (b.name == "hld/general")
                tmp = b;
        }
        if (tmp != null)
            list = tmp.LoadAsset<TextAsset>("listofdancers").ToString().Split(',');

        else
        {
            Debug.LogWarning("No list of dancers");
            return null;
        }

        var str_list = list.OrderBy(x => x).ToArray();
        for (int i = 0; i < list.Length; i++)
        {
            //clean up newlines;
            list[i] = list[i].Replace("\n", "");
            list[i] = list[i].Replace("\r", "");
            list[i] = list[i].TrimEnd(System.Environment.NewLine.ToCharArray());
            list[i] = list[i].TrimStart(System.Environment.NewLine.ToCharArray());
        }
        return list;
    }

    IEnumerator CheckWifiAndDownloads()
    {
        GameObject WifiInUseIcon = null;
        string persistantDataPath = UIB_PlatformManager.persistentDataPath;

        WifiInUseIcon = GameObject.Find("DownloadIcon");

        while (true)
        {
            //Debug.Log("DL Count " + DownloadCount + " checking for " + checkingForUpdates);
            if (WifiInUseIcon == null)
            {
                Debug.Log("Bad");
            }

            if (CheckInternet())
            {
                //Debug.Log("We have internet");
                if (DownloadCount > 0 && checkingForUpdates <= 0)
                {
                    WifiInUseIcon.SetActive(true);
                }
                else
                {
                    WifiInUseIcon.SetActive(false);
                }
                yield return null;

            }
            else
            {
                Debug.Log("No internet ");
            }

            if (PercentDownloaded.Equals(100))
            {
                //Debug.Log("Finished File Check and Downloads " + Time.time + " Seconds");
                WifiInUseIcon.SetActive(false);

                yield break;
            }

            yield return null;
        }
    }

    IEnumerator UpdateDownloadPercent()
    {
        while (PercentDownloaded < 100)
        {
            if (TotalDownloads > 0)
            {
                PercentDownloaded = (float)((TotalDownloads - DownloadCount) / TotalDownloads) * 100;
                if (PercentDownloaded > 0)
                { }
                else
                {
                    PercentDownloaded = 0;
                }
                percentText.text = PercentDownloaded + "%";
            }
            yield return null;
        }
        yield break;
    }

    private void CheckAndUpdateLinks(string key)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("LockedPageButton"))
        {
            GameObject CodeToInfoObject = null;
            if (go.name == key.Replace("Info_Page", "Code_Button"))
            {
                CodeToInfoObject = go;
            }
            GameObject InfoToCodeObject = null;
            if (go.name == key.Replace("Info_Page", "Info_Button"))
            {
                InfoToCodeObject = go;
            }

            // if we have entered passcode previously.
            //If date of passcode entry doesn't check out. we don't change the name
            if (PlayerPrefs.HasKey(key))
            {
                var codeEntered = DateTime.Parse(PlayerPrefs.GetString(key)).ToUniversalTime();

                //Debug.Log("code previously entered " + codeEntered + " now " + DateTime.UtcNow );

                if (codeEntered.AddHours(48).CompareTo(DateTime.UtcNow) < 0)
                {
                    try
                    {
                        //exceeded time limit. Reactivte code-entry page
                        InfoToCodeObject.name = key.Replace("Info_Page", "Code_Button");
                        InfoToCodeObject.GetComponent<UIB_Button>().Init();
                    }
                    catch (Exception e)
                    {
                        if (e.GetType() == typeof(NullReferenceException))
                        {

                        }
                    }
                }
                else
                {
                    //We have access.
                    //Change the code page to the info page

                    //Debug.Log("THINK WE HAVE ACCESS");
                    try
                    {
                        CodeToInfoObject.name = key.Replace("Info_Page", "Info_Button");
                        CodeToInfoObject.GetComponent<UIB_Button>().Init();
                    }
                    catch (Exception e)
                    {
                        if (e.GetType() == typeof(NullReferenceException))
                        {

                        }
                    }
                }
                //Swap info button for code button
            }
            else
            {
                try
                {
                    //if you do not have the player pref
                    //set info page to code page
                    InfoToCodeObject.name = key.Replace("Info_Page", "Code_Button");
                    InfoToCodeObject.GetComponent<UIB_Button>().Init();
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(NullReferenceException))
                    {

                    }
                }
            }
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        CheckAndUpdateLinks("Displayed-Info_Page");
        CheckAndUpdateLinks("OnDisplay-Info_Page");
        CheckAndUpdateLinks("Unfinished-Info_Page");

    }

}