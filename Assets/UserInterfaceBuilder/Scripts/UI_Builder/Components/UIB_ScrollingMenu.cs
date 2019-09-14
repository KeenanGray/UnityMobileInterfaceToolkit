using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Builder
{
    [ExecuteInEditMode]
    public class UIB_ScrollingMenu : MonoBehaviour
    {
        public bool ShouldScroll;
        public bool AutoFormat;

        public Resolution resolution;

        public Color normal_ButtonColor;
        public Color pressed_ButtonColor;
        public Color highlight_ButtonColor;
        public Color disabled_ButtonColor;

        public ColorReference TextColor;
        public ColorReference TextHighlightColor;
        public ColorReference TextBackgroundColor;

        public TMP_FontAsset font;
        public float fontSize;

        public TextAlignmentOptions alignment;
        public int padding;

        public bool Playing = false;
        public bool playedOnce = false;


        private void Start()
        {
            Setup();
            Playing = true;
            Init();
        }

        private void Update()
        {
            Init();
        }

        public void Init()
        {
            var myRt = GetComponent<RectTransform>();

            if (resolution == null)
                resolution = Resources.Load("ScriptableObjects/ResolutionAsset") as Resolution;

            myRt.sizeDelta = new Vector2(resolution.Width, resolution.Height);
            var contentRect = GetComponent<ScrollRect>().content.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(resolution.Width, contentRect.rect.height);
        }

        private void OnEnable()
        {
            Playing = false;
            Setup();
            Playing = true;
        }

        public void Setup()
        {
            GetComponent<ScrollRect>().vertical = ShouldScroll;

            if (!Playing || !playedOnce)
            {
                foreach (Button b in GetComponentsInChildren<Button>())
                {
                    var cb = b.colors;

                    cb.normalColor = normal_ButtonColor;
                    cb.pressedColor = pressed_ButtonColor;
                    cb.highlightedColor = highlight_ButtonColor;
                    cb.disabledColor = disabled_ButtonColor;

                    TextMeshProUGUI text = null;
                    text = b.GetComponentInChildren<TextMeshProUGUI>();

                    if (text != null)
                    {
                        if (!text.gameObject.name.Contains("noedit"))
                        {
                            if (text != null)
                            {
                                text.color = TextColor.Value;
                                text.fontSize = fontSize;
                                text.font = font;

                                //only set image color and text size for buttons that have text
                                SetAlignment(text);
                                var I = b.GetComponent<Image>();
                                I.color = TextBackgroundColor.Value;

                                //  var thingy = (Handles.GetMainGameViewSize().y / Handles.GetMainGameViewSize().x) * GameObject.Find("MainCanvas").GetComponent<CanvasScaler>().referenceResolution.x;
                                //  Debug.Log("Thingy " + thingy / 2);
                                //  b.GetComponent<RectTransform>().sizeDelta = new Vector2(thingy / 2, b.GetComponent<RectTransform>().sizeDelta.y);
                            }
                        }
                    }
                }

                foreach (TextMeshProUGUI text2 in GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (!text2.gameObject.name.Contains("noedit"))
                    {
                        text2.color = TextColor.Value;
                        text2.fontSize = fontSize;
                        text2.font = font;
                        SetAlignment(text2);
                    }

                }

                var vlg = GetComponent<ScrollRect>().content.GetComponent<VerticalLayoutGroup>();
                //  vlg.transform.localPosition = new Vector3(0, 0, 0);
                vlg.enabled = false;
                vlg.enabled = true;

                if (Playing)
                    playedOnce = true;
            }
        }

        private void SetAlignment(TextMeshProUGUI t)
        {
            t.GetComponentsInChildren<TextMeshProUGUI>()[0].alignment = alignment;
            var vlg = GetComponent<ScrollRect>().content.GetComponent<VerticalLayoutGroup>();

            vlg.padding.left = 0;
            vlg.padding.right = 0;
            // vlg.padding.top = 0;
            //  vlg.padding.bottom = 0;

            if (t.alignment.ToString().Contains("Left"))
                vlg.padding.left = padding;
            else if (t.alignment.ToString().Contains("Right"))
                vlg.padding.left = padding;
            else
            {

            }


        }

    }

}