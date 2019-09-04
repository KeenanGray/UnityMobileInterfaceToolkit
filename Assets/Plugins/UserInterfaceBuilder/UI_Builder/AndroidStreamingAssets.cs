using UnityEngine;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System;
using UI_Builder;

public static class AndroidStreamingAssets
{
    private const string STREAMING_ASSETS_DIR = "assets/";
    private const string STREAMING_ASSETS_INTERNAL_DATA_DIR = "assets/bin/";
    private const string META_EXTENSION = ".meta";

    private static string m_path = null;
    public static string Path
    {
        get
        {
            if (m_path == null)
                Extract();

            return m_path;
        }
    }

    public static void Extract()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        string targetPath = UIB_PlatformManager.persistentDataPath+UIB_PlatformManager.platform;
        string result = UIB_PlatformManager.persistentDataPath+UIB_PlatformManager.platform;

        Directory.CreateDirectory( targetPath );

        if( targetPath[targetPath.Length - 1] != '/' || targetPath[targetPath.Length - 1] != '\\' )
            targetPath += '/';

        HashSet<string> createdDirectories = new HashSet<string>();
        
        ZipFile zf = null;
        try
        {
            using( FileStream fs = File.OpenRead( Application.dataPath  ) )
            {
                zf = new ZipFile( fs );
                foreach( ZipEntry zipEntry in zf )
                {
                    if( !zipEntry.IsFile )
                        continue;

                    string name = zipEntry.Name;
                    if( name.StartsWith( STREAMING_ASSETS_DIR ) && !name.EndsWith( META_EXTENSION ) && !name.StartsWith( STREAMING_ASSETS_INTERNAL_DATA_DIR ) )
                    {
                        string relativeDir = System.IO.Path.GetDirectoryName( name );
                        if( !createdDirectories.Contains( relativeDir ) )
                        {
                            Directory.CreateDirectory( targetPath + relativeDir );
                            createdDirectories.Add( relativeDir );
                        }

                        byte[] buffer = new byte[4096];
                        using( Stream zipStream = zf.GetInputStream( zipEntry ) )
                        using( FileStream streamWriter = File.Create( targetPath + name ) )
                        {
                            StreamUtils.Copy( zipStream, streamWriter, buffer );
                        }
                    }
                }
            }
        }
        catch(Exception e){
            Debug.Log("PROBLEM:" + e);
        }
        finally
        {
            if( zf != null )
            {
                zf.IsStreamOwner = true;
                zf.Close();
            }
        }


        m_path = result;
#else
        m_path = UIB_PlatformManager.persistentDataPath+UIB_PlatformManager.platform;
#endif
    }
}