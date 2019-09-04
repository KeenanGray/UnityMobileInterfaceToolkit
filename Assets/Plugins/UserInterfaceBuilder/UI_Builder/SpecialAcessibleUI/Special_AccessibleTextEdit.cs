
using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;

[AddComponentMenu("Accessibility/UI/Special_Accessibility_TextEdit_Inspector")]
public class Special_AccessibleTextEdit : UAP_BaseElement
{
#if ACCESS_NGUI
	private EventDelegate m_Callback = null;
#endif

    string prevText = "";
    string deltaText = "";

    //////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        UIB_InputManager.TouchDelegate += OnTouchWhileInputSelected;
    }

    private void OnTouchWhileInputSelected(Touch[] touches, int taps)
    {
        if (!GetInputField().isFocused)
            return;

        if (touches.Length == 1)
        {
            string newText = "";
            string fullText = "";
            InputField inputField = GetInputField();
            if (inputField != null)
            {
                newText = inputField.text;
            }

#if ACCESS_NGUI
        UIInput element = GetNGUIInputField();
        if (element != null)
        {
            newText = element.value;
        }
#endif
            fullText = newText;
            UAP_AccessibilityManager.Say(fullText);
            deltaText = fullText;

            inputField.Select();
            inputField.caretPosition = fullText.Length;
        }
    }

    Special_AccessibleTextEdit()
    {
        m_Type = AccessibleUIGroupRoot.EUIElement.ETextEdit;
    }

    //////////////////////////////////////////////////////////////////////////

    public override bool IsElementActive()
    {
        // Return whether this element is visible (and maybe usable)
        if (!base.IsElementActive())
            return false;

        if (m_ReferenceElement != null)
            if (!m_ReferenceElement.gameObject.activeInHierarchy)
                return false;

        if (!UAP_AccessibilityManager.GetSpeakDisabledInteractables())
            if (!IsInteractable())
                return false;

        return true;
    }

    //////////////////////////////////////////////////////////////////////////

    InputField GetInputField()
    {
        InputField refElement = null;
        if (m_ReferenceElement != null)
            refElement = m_ReferenceElement.GetComponent<InputField>();
        if (refElement == null)
            refElement = GetComponent<InputField>();

        return refElement;
    }

    //////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	private UIInput GetNGUIInputField()
	{
		UIInput refElement = null;
		if (m_ReferenceElement != null)
			refElement = m_ReferenceElement.GetComponent<UIInput>();
		if (refElement == null)
			refElement = GetComponent<UIInput>();

		return refElement;
	}
#endif

    //////////////////////////////////////////////////////////////////////////

    public override string GetCurrentValueAsText()
    {
        InputField inputField = GetInputField();
        if (inputField != null)
            return inputField.text;

#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
			return element.value;
#endif

        return "";
    }

    //////////////////////////////////////////////////////////////////////////

    public override bool IsInteractable()
    {
        InputField inputField = GetInputField();
        if (inputField != null)
        {
            if (inputField.enabled == false || inputField.interactable == false)
                return false;
            else
                return true;
        }

        // NGUI
#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			if (element.enabled == false || element.isActiveAndEnabled == false)
				return false;
			else
				return true;
		}

#endif

        // We couldn't find any buttons...
        return false;
    }

    //////////////////////////////////////////////////////////////////////////

    public override void Interact()
    {
        InputField inputField = GetInputField();
        if (inputField != null)
        {
            prevText = inputField.text;
            deltaText = prevText;
            inputField.onEndEdit.AddListener(delegate { ValueChangeCheck(); });

            // Give the Text Field the focus (to bring up the keyboard entry)
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }

#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			//Debug.Log("Enabling NGUI Input field");
			if (m_Callback == null)
				m_Callback = new EventDelegate(this, "ValueChangeCheck");
			element.onChange.Add(m_Callback);
			element.isSelected = true;
		}
#endif

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		TouchScreenKeyboard.Open(GetCurrentValueAsText());
#endif
    }

    //////////////////////////////////////////////////////////////////////////

    public void ValueChangeCheck()
    {
        string newText = "";
        string fullText = "";
        InputField inputField = GetInputField();
        if (inputField != null)
        {
            newText = inputField.text;
        }

#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			newText = element.value;
		}
#endif

        fullText = newText;
        // Remove the previous text from the string to get just the new bits
        if (newText.StartsWith(deltaText))
            newText = newText.Substring(deltaText.Length);
     //  if (newText.Length > 0)
     //       UAP_AccessibilityManager.Say(newText);
        deltaText = fullText;
    }

    //////////////////////////////////////////////////////////////////////////

    public override void InteractAbort()
    {
        InputField inputField = GetInputField();
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });

            // Restore previous value
            inputField.text = prevText;
        }

#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			element.onChange.Remove(m_Callback);
			element.RemoveFocus();
			//element.isSelected = false;

			// Restore previous value
			element.value = prevText;
		}
#endif

        prevText = "";
    }

    //////////////////////////////////////////////////////////////////////////

    public override void InteractEnd()
    {
        InputField inputField = GetInputField();
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });
        }

#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			element.onChange.Remove(m_Callback);
			element.RemoveFocus();
			//element.isSelected = false;
		}
#endif
    }

    //////////////////////////////////////////////////////////////////////////

    public override void HoverHighlight(bool enable)
    {
        InputField inputField = GetInputField();
        if (inputField != null)
        {
            var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
            if (enable)
                inputField.OnPointerEnter(pointer);
            else
                inputField.OnPointerExit(pointer);
        }

#if ACCESS_NGUI
		UIInput element = GetNGUIInputField();
		if (element != null)
		{
			// There is currently no hover effect on NGUI UIInputs
			//element.isSelected = true;
		}
#endif
    }

    //////////////////////////////////////////////////////////////////////////

    public override bool AutoFillTextLabel()
    {
        // If there is no name label set, don't set anything as name
        if (!base.AutoFillTextLabel())
            m_Text = "";

        return false;
    }

    //////////////////////////////////////////////////////////////////////////
}
