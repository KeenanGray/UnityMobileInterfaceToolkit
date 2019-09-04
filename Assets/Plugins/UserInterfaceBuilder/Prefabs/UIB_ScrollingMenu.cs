using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI_Builder
{
    public class UIB_ScrollingMenu : MonoBehaviour
    {
        public bool ShouldScroll;
        public bool AutoFormat;

        //public Color ImageColor;

        public Color normal_ButtonColor;
        public Color pressed_ButtonColor;
        public Color highlight_ButtonColor;
        public Color disabled_ButtonColor;

        public Color TextColor;
        public Color TextBackgroundColor;

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
        }

        public void Init()
        {
            if (!AutoFormat)
                return;

            var rt = GameObject.FindWithTag("MainCanvas").GetComponent<RectTransform>();
            var myRt = GetComponent<RectTransform>();

            var percentageScreenSize = .76f;

            //myRt.sizeDelta = new Vector2(rt.rect.width, (rt.sizeDelta.y * (rt.anchorMax.y - rt.anchorMin.y) * percentageScreenSize));
            var logo = GameObject.Find("Logo").GetComponent<RectTransform>();

            myRt.sizeDelta = new Vector2(rt.rect.width, (logo.anchoredPosition.y * .5f));
            myRt.anchorMin = new Vector2(0, 0);
            myRt.anchorMax = new Vector2(0, percentageScreenSize);
            myRt.pivot = new Vector2(0, 0);
            myRt.anchoredPosition = new Vector2(0, 0);

            var contentRect = GetComponent<ScrollRect>().content.GetComponent<RectTransform>();

            //if height is greater
            if (contentRect.sizeDelta.y >= myRt.rect.height)
            {
                contentRect.pivot = new Vector2(0, 1);
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(0, 0);
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0);
            }
            else
            {
                contentRect.pivot = new Vector2(0, 0);
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(0, 0);
                contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0);
            }

            myRt.anchoredPosition = new Vector3(contentRect.anchoredPosition.x, contentRect.anchoredPosition.y + 60);
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
                                text.color = TextColor;
                                text.fontSize = fontSize;
                                text.font = font;

                                //only set image color and text size for buttons that have text
                                SetAlignment(text);
                                var I = b.GetComponent<Image>();
                                I.color = TextBackgroundColor;

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
                        text2.color = TextColor;
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