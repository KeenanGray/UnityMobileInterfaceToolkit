using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using TMPro;

[AddComponentMenu("Accessibility/UI/Special_AccessibleImageLabel_Inspector")]
public class Special_AccessibleImageLabel : UAP_BaseElement
{
    //////////////////////////////////////////////////////////////////////////

    Special_AccessibleImageLabel()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ELabel;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool IsElementActive()
	{
		// Return whether this button is usable, and visible
		if (!base.IsElementActive())
			return false;

        TextMeshProUGUI label = GetLabel();
		if (label != null)
		{
			if (!label.gameObject.activeInHierarchy || label.enabled == false)
				return false;
			else
				return true;
		}

#if ACCESS_NGUI
		UILabel nGUILabel = GetNGUILabel();
		if (nGUILabel != null)
		{
			if (!nGUILabel.gameObject.activeInHierarchy || nGUILabel.enabled == false)
				return false;
			else
				return true;
		}
#endif

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public override bool AutoFillTextLabel()
	{
		// The label's default is to the label, so, nothing to do here
		// But override it anyway, so that it doesn't get filled with
		// the GameObject's name.

		return true;
	}

	//////////////////////////////////////////////////////////////////////////

	public override string GetText()
	{
		if (!m_TryToReadLabel)
		{
			if (IsNameLocalizationKey())
				return CombinePrefix(UAP_AccessibilityManager.Localize(m_Text));
			return m_Text;
		}

        TextMeshProUGUI label = GetLabel();
		if (label != null)
		{
			if (IsNameLocalizationKey())
                return CombinePrefix(UAP_AccessibilityManager.Localize(label.GetParsedText()));
			else
                return CombinePrefix(label.GetParsedText());
		}

#if ACCESS_NGUI
		UILabel nGUILabel = GetNGUILabel();
		if (nGUILabel != null)
		{
			if (IsNameLocalizationKey())
				return CombinePrefix(UAP_AccessibilityManager.Localize(nGUILabel.text));
			else
				return CombinePrefix(nGUILabel.text);
		}
#endif

		//Debug.Log("Getting item text " + m_Text);

		if (IsNameLocalizationKey())
			return UAP_AccessibilityManager.Localize(m_Text);

		return m_Text;
	}

	//////////////////////////////////////////////////////////////////////////

	private TextMeshProUGUI GetLabel()
	{
        TextMeshProUGUI label = null;
		if (m_ReferenceElement != null)
			label = m_ReferenceElement.GetComponent<TextMeshProUGUI>();
		if (m_NameLabel != null)
			label = m_NameLabel.GetComponent<TextMeshProUGUI>();
		if (label == null)
			label = GetComponent<TextMeshProUGUI>();

		return label;
	}

	//////////////////////////////////////////////////////////////////////////

#if ACCESS_NGUI
	private UILabel GetNGUILabel()
	{
		UILabel label = null;
		if (m_ReferenceElement != null)
			label = m_ReferenceElement.GetComponent<UILabel>();
		if (m_NameLabel != null)
			label = m_NameLabel.GetComponent<UILabel>();
		if (label == null)
			label = GetComponent<UILabel>();

		return label;
	}
#endif

	//////////////////////////////////////////////////////////////////////////

	protected override void AutoInitialize()
	{
		if (m_TryToReadLabel)
		{
            TextMeshProUGUI label = GetLabel();
			if (label != null)
				m_NameLabel = label.gameObject;

#if ACCESS_NGUI
			UILabel nGUILabel = GetNGUILabel();
			if (nGUILabel != null)
				m_NameLabel = nGUILabel.gameObject;
#endif
		}
		else
		{
			m_NameLabel = null;
		}
	}

	//////////////////////////////////////////////////////////////////////////
}
