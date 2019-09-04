using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UI_Builder
{
    public static class UIB_Utilities
    {
        public static float Map(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        private static void printElapsedTime(int pos, float t1)
        {
            var t2 = Time.time;
            var elapsed = t2 - t1;
            Debug.Log("elapsed:" + elapsed + " at pos " + pos);
        }

        public static string SplitCamelCase(string s)
        {
            return Regex.Replace(s, "((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", " $1").Remove(0, 1);
        }

        public static string SplitOnFinalUnderscore(string s)
        {
            var outStr = "";
            for (int i = 0; i < s.Split('_').Length - 1; i++)
                outStr += s.Split('_')[i];

            return outStr;
        }

        public static string CleanUpHyphenated(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].Equals('-'))
                    if (i < s.Length - 1)
                        if (s[i + 1].Equals('_'))
                        {
                            Debug.Log("HERE:" + s.Remove(i + 1, 1));
                            return s.Remove(i + 1, 1);
                        }

            }
            return s;
        }

        public static string RemoveAllButLastUnderscore(string s)
        {
            var outStr = "";
            if (s.Split('_').Length <= 2)
                return s;

            for (int i = 0; i < s.Split('_').Length; i++)
            {
                if (i == s.Split('_').Length - 1)
                    outStr += '_' + s.Split('_')[i];
                else
                    outStr += s.Split('_')[i];
            }
            return outStr;
        }
    }
    public static class Epoch
    {

        public static int Current()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;

            return currentEpochTime;
        }

        public static int SecondsElapsed(int t1)
        {
            int difference = Current() - t1;

            return Mathf.Abs(difference);
        }

        public static int SecondsElapsed(int t1, int t2)
        {
            int difference = t1 - t2;

            return Mathf.Abs(difference);
        }
    }
}