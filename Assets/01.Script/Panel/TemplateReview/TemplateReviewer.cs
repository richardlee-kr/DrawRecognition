using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class TemplateReviewer : MonoBehaviour
{
    [SerializeField] private DisplayTemplate m_display;

    [SerializeField] private TMP_Dropdown dd_templateNameList;
    [SerializeField] private Button btn_previous;
    [SerializeField] private Button btn_next;
    [SerializeField] private Button btn_remove;

    private List<string> m_templateNames;
    private string m_currentTemplateName;
    private int m_currentTemplateIndex;

    private GestureTemplates m_templates => GestureTemplates.Get();

    private void Awake()
    {
        btn_previous.onClick.AddListener(() => ChooseTemplateIndex(-1));
        btn_next.onClick.AddListener(() => ChooseTemplateIndex(1));
        btn_remove.onClick.AddListener(() => RemoveTemplate());

        dd_templateNameList.onValueChanged.AddListener(ChooseTemplateToShow);

        UpdateTemplates();
    }

    public void SetVisibility(bool visible)
    {
        this.gameObject.SetActive(visible);
        m_display.gameObject.SetActive(visible);
        if(visible)
        {
            UpdateDisplay();
            ChooseTemplateToShow(0);
        }
    }

    private void UpdateDisplay()
    {
        UpdateTemplates();
        RecognitionManager.GestureTemplate[] _templates = m_templates.GetRawTemplatesByName(m_currentTemplateName);
        if(_templates.Length > 0)
        {
            m_display.Draw(_templates[m_currentTemplateIndex]);
        }
        else
        {
            m_display.Clear();
        }
    }

    private void UpdateTemplates()
    {
        m_templateNames = m_templates
            .RawTemplates
            .Select(templates => templates.name)
            .Distinct().ToList();

        dd_templateNameList.options = m_templateNames
            .Select(templateName => new TMP_Dropdown.OptionData(templateName))
            .ToList();

        bool anyTemplateAvailable = m_templateNames.Any();
        btn_remove.gameObject.SetActive(anyTemplateAvailable);
        btn_previous.gameObject.SetActive(anyTemplateAvailable);
        btn_next.gameObject.SetActive(anyTemplateAvailable);
        dd_templateNameList.gameObject.SetActive(anyTemplateAvailable);
    }

    private void ChooseTemplateIndex(int increament)
    {
        m_currentTemplateIndex += increament;
        if (m_currentTemplateIndex < 0 || m_currentTemplateIndex > m_templates.GetRawTemplatesByName(m_currentTemplateName).Length - 1)
        {
            m_currentTemplateIndex -= increament;
            return;
        }
        else
        {
            UpdateDisplay();
        }
    }

    private void RemoveTemplate()
    {
        //IEnumerable<RecognitionManager.GestureTemplate> templatesByName = m_templates.ProceedTemplates
        //    .Where(template => template.name == m_currentTemplateName).ToList();
        IEnumerable<RecognitionManager.GestureTemplate> templatesByName = m_templates.RawTemplates
            .Where(template => template.name == m_currentTemplateName).ToList();
        Debug.Log(templatesByName.Count());
        RecognitionManager.GestureTemplate templateToRemove = templatesByName
            .ElementAt(m_currentTemplateIndex);

        //int indexToRemove = m_templates.ProceedTemplates.IndexOf(templateToRemove);
        int indexToRemove = m_templates.RawTemplates.IndexOf(templateToRemove);
        m_templates.RemoveAtIndex(indexToRemove);

        if(m_currentTemplateIndex != 0)
        {
            m_currentTemplateIndex--;
        }

        if(templatesByName.Count() == 1)
        {
            ChooseTemplateToShow(0);
        }

        UpdateDisplay();
    }

    private void ChooseTemplateToShow(int index)
    {
        string chosenTemplateName = m_templateNames[index];
        m_currentTemplateName = chosenTemplateName;
        m_currentTemplateIndex = 0;
        UpdateDisplay();
    }
}
