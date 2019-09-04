using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UI_Builder
{
    public class UIB_PlatformManager
    {
        public static string persistentDataPath;
        public static string platform;

        public static void Init()
        {
            UIB_PlatformManager.persistentDataPath = Application.persistentDataPath + "/heidi-latsky-dance/";

            platform = "android/";
#if UNITY_IOS && !UNITY_EDITOR
        platform="ios/";
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        platform = "android/";
#endif
            Debug.Log(UIB_PlatformManager.persistentDataPath);

        }

    }
}
