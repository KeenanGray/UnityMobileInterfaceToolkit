using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Amazon.S3.Model;
using UnityEngine;
using UnityEngine.Networking;

namespace UI_Builder
{

    public enum UIB_FileTypes
    {
        Images,
        Videos,
        Text
    }

    public class UIB_FileManager : MonoBehaviour
    {
        //db name heroku_pm1crn83
        // Use this for initialization
        public TextAsset config;

        public float TimeOutLength;

        private void Start()
        {

        }
        private void Update()
        {

        }

        private string GetAPIKey(string v)
        {
            foreach (string s in v.Split('\n'))
            {
                if (s.Contains("api_key"))
                {
                    return s.Split('=')[1];
                }
            }
            return v;
        }

        public static byte[] ReadFromBytes(string path, UIB_FileTypes kind)
        {
            var ext = "";
            byte[] fileData = null;
            switch (kind)
            {
                case UIB_FileTypes.Images:
                    ext = ".jpg";
                    if (File.Exists(path + ext))
                    {
                        fileData = File.ReadAllBytes(path + ext);
                    }
                    ext = ".jpeg";
                    if (File.Exists(path + ext))
                    {
                        fileData = File.ReadAllBytes(path + ext);
                    }
                    ext = ".png";
                    if (File.Exists(path + ext))
                    {
                        fileData = File.ReadAllBytes(path + ext);
                    }
                    break;
                case UIB_FileTypes.Videos:
                    Debug.Log("Videos");
                    break;
                case UIB_FileTypes.Text:
                    Debug.Log("Text");
                    break;
                default:
                    Debug.Log("Default case");
                    break;
            }

            return fileData;
        }

        public static void WriteFileFromResponse(GetObjectResponse response, string fileName)
        {
            //check if the directory exists
            //split filepath into directory and filename
            int cont = 0;
            string name = "";
            string directory = "";

            foreach (string i in fileName.Split('/'))
            {
                cont++;
                if (cont >= fileName.Split('/').Length)
                {
                    name = fileName.Split('/')[cont - 1];
                    break;
                }
                else
                {
                    directory = directory + "/" + i;
                }
            }

            print("response:" + response + " fileName" + fileName);

            var newpath = UIB_PlatformManager.persistentDataPath + directory.Replace("/heidi-latsky-dance/", "");

            if (!Directory.Exists(newpath))
            {
                Directory.CreateDirectory(newpath);
            }
            else
            {
            }
            using (var fs = System.IO.File.Create(newpath + "/" + name))
            {
                byte[] buffer = new byte[81920];
                int count;
                while ((count = response.ResponseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, count);
                }
                fs.Flush();
            }

        }

        void WriteJsonFromWeb(string data, string fileName)
        {
            string destination = UIB_PlatformManager.persistentDataPath + "/" + fileName;
            FileStream file;
            StreamReader sr;
            StreamWriter sw;

            //if no data returned, do not continue
            if (data == "" || data == null)
            {
                return;
            }
            string jsonToWrite = "{\"data\":" + data + "}";

            //Open the local file
            if (File.Exists(destination))
            {
                //If the file exists, compare the two versions
                //If they are different. overwrite the old version
                sr = File.OpenText(destination);
                var oldJson = sr.ReadToEnd();
                sr.Close();

                oldJson = oldJson.Remove(oldJson.Length - 1, 1);
                //   oldJson = oldJson.Remove(oldJson.Length - 1, 1);
                if (oldJson.Equals(jsonToWrite))
                {
                    //                Debug.Log("JSON matches");
                }
                else
                {
                    file = File.Create(destination);
                    file.Close();
                    //          Debug.Log("New JSON");
                    sw = new StreamWriter(destination, true);
                    sw.WriteLine(jsonToWrite);
                    sw.Close();
                }
            }
            else
            {
                //If the file does not exist, create the file and write data to it

                file = File.Create(destination);
                file.Close();
                sw = new StreamWriter(destination, true);
                sw.WriteLine(jsonToWrite);
                sw.Close();
            }
        }

        public static bool FileExists(string fileName)
        {
            string destination = fileName;
            return File.Exists(destination);
        }
        internal static bool FileExists(string path, UIB_FileTypes kind)
        {
            var ext = "";
            switch (kind)
            {
                case UIB_FileTypes.Images:
                    ext = ".jpg";
                    if (File.Exists(path + ext))
                    {
                        return true;
                    }
                    ext = ".jpeg";
                    if (File.Exists(path + ext))
                    {
                        return true;
                    }
                    ext = ".png";
                    if (File.Exists(path + ext))
                    {
                        return true;
                    }
                    break;
                case UIB_FileTypes.Videos:
                    Debug.Log("Videos");
                    break;
                case UIB_FileTypes.Text:
                    Debug.Log("Text");
                    break;
                default:
                    Debug.Log("Default case");
                    break;
            }
            return false;
        }


        public static void WriteJsonUnModified(string data, string fileName)
        {
            string destination = UIB_PlatformManager.persistentDataPath + "/" + fileName;
            FileStream file;
            StreamReader sr;
            StreamWriter sw;

            //if no data returned, do not continue
            if (data == "" || data == null)
            {
                return;
            }
            string jsonToWrite = data;

            //Open the local file
            if (File.Exists(destination))
            {
                //If the file exists, compare the two versions
                //If they are different. overwrite the old version
                sr = File.OpenText(destination);
                var oldJson = sr.ReadToEnd();
                sr.Close();

                oldJson = oldJson.Remove(oldJson.Length - 1, 1);
                //   oldJson = oldJson.Remove(oldJson.Length - 1, 1);
                if (oldJson.Equals(jsonToWrite))
                {
                }
                else
                {
                    file = File.Create(destination);
                    file.Close();
                    sw = new StreamWriter(destination, true);
                    sw.WriteLine(jsonToWrite);
                    sw.Close();
                }
            }
            else
            {
                //If the file does not exist, create the file and write data to it

                file = File.Create(destination);
                file.Close();
                sw = new StreamWriter(destination, true);
                sw.WriteLine(jsonToWrite);
                sw.Close();
            }
        }

        internal static string GetFileNameFromKey(string key)
        {
            //remove the file ext
            var val = key.Split('.')[0];
            //split the string by '/' and get the last one
            val = val.Split('/')[val.Split('/').Length - 1];
            return val;
        }

        public static string ReadTextFile(string fileName)
        {
            string destination = UIB_PlatformManager.persistentDataPath + "/" + fileName;
            StreamReader sr;

            string jsonStr = "";
            //Open the local file
            if (File.Exists(destination))
            {
                //If the file exists, read the file
                //TODO:
                sr = File.OpenText(destination);
                jsonStr = sr.ReadToEnd();
                sr.Close();

                return jsonStr;
            }
            else
            {
                Debug.LogWarning("no file found " + fileName);
                return "";
            }
        }

        public static string ReadTextAssetBundle(string fileName, string bundleString)
        {
            AssetBundle tmp = null;
            foreach (AssetBundle b in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (b.name == bundleString)
                    tmp = b;
            }
            if (tmp != null)
            {
                try
                {
                    return tmp.LoadAsset<TextAsset>(fileName).ToString();
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(NullReferenceException))
                    {

                    }
                    //print(e);
                    Debug.Log("no file:" + fileName + " in bundle:" + bundleString);
                    return "";
                }
            }
            else
            {
                Debug.LogWarning("Asset bundle not found " + bundleString);
                return "";
            }
        }

        public static void WriteFromStreamingToPersistent(string fileName)
        {
            var src = Application.streamingAssetsPath + "/" + UIB_PlatformManager.platform + fileName;
            var dest = UIB_PlatformManager.persistentDataPath + UIB_PlatformManager.platform + fileName;
            //check if the src directory exists
            if (FileExists(src))
            {
                //check if the dest directory exists
                if (!FileExists(dest))
                {
                    //create the directory
                    string directory = "";
                    var cont = 0;
                    var name = "";

                    foreach (string i in dest.Split('/'))
                    {
                        cont++;
                        if (cont >= dest.Split('/').Length)
                        {
                            name = dest.Split('/')[cont - 1];
                            break;
                        }
                        else
                        {
                            directory = directory + "/" + i;
                        }
                    }

                    Directory.CreateDirectory(directory);

                }

                //Debug.Log("writing: " + src + " to dest: " + dest);
                File.Copy(src, dest);
                //  File.CopyFileOrDirectory(src, dest);
            }
            else
            {
                Debug.LogWarning("NO BUNDLE EXCEPTION::No asset bundle with this path found in streaming assets. " + src);
            }

        }

        public static bool AndroidCopyIsDone;

        public static bool HasUpdatedAFile { get; set; }

        IEnumerator CreateStreamingAssetDirectories(string fileName)
        {
#if UNITY_ANDROID
            //this copies the file in the streaming assets directory to the persistant data path on android
            //It comes from a special utility called "Android Streaming Assets"
            var samplePath = AndroidStreamingAssets.Path;
            samplePath = AndroidStreamingAssets.Path;
            printDirectory(samplePath);
            //Do not change the above 3 lines!!
#endif
            yield break;

        }

        public static void DeleteFile(string filename)
        {
            //if we are not in the Unity Editor, delete the streaming assets files to save space
            var path = Path.Combine(Application.streamingAssetsPath, "/", UIB_PlatformManager.platform, filename);
            GameObject.Find("FileManager").GetComponent<UIB_FileManager>().StartCoroutine("WaitDeleteFile",path);
           
#if UNITY_ANDROID && !UNITY_EDITOR
#endif

        }

        private IEnumerator WaitDeleteFile(string filepath)
        {
#if UNITY_IOS && !UNITY_EDITOR
            filepath = Path.Combine("/private",filepath);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            filepath = Application.streamingAssetsPath;
#endif

#if !UNITY_EDITOR
            while (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                    Debug.Log("called delete function");
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                yield return null;
            }
#endif

            yield break;
        }

        private void printDirectory(string v)
        {
            //Debug.Log("Dir:" + v);
            foreach (string s in Directory.GetFiles(v))
            {
                //print("File: " + s);
            }
            foreach (string d in Directory.GetDirectories(v))
            {
                printDirectory(d);
            }
        }
    }
}