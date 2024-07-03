using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecognitionManager : MonoBehaviour
{
    [SerializeField] private Drawable m_drawable;

    [SerializeField] private Button btn_recognizeMode;
    [SerializeField] private Button btn_templateMode;
    [SerializeField] private Button btn_reviewMode;
    [SerializeField] private TextMeshProUGUI txt_recognitionResult;
    [SerializeField] private TMP_InputField input_templateName;

    [SerializeField] private RecognitionPanel m_recogntionPanel;
    [SerializeField] private TemplateReviewer m_templateReviewer;

    private string _templateName => input_templateName.text;

    private GestureTemplates m_templates => GestureTemplates.Get();
    private IRecognizer m_currentRecognizer;
    [SerializeField]
    private RecognizerMode m_mode = RecognizerMode.TEMPLATE;

    public enum RecognizerMode
    {
        RECOGNITION,
        TEMPLATE,
        REVIEW,
    }

    [Serializable]
    public struct GestureTemplate
    {
        public string name;
        public DollarPoint[] points;

        public GestureTemplate(string templateName, DollarPoint[] preparedPoint)
        {
            name = templateName;
            points = preparedPoint;
        }
    }

    private void Awake()
    {
        btn_recognizeMode.onClick.AddListener(() => SetMode(RecognizerMode.RECOGNITION));
        btn_templateMode.onClick.AddListener(() => SetMode(RecognizerMode.TEMPLATE));
        btn_reviewMode.onClick.AddListener(() => SetMode(RecognizerMode.REVIEW));

        m_drawable.OnDrawFinished += OnDrawFinished;

        SetMode(m_mode);
    }

    private void SetMode(RecognizerMode mode)
    {
        m_mode = mode;

        m_drawable.ClearDrawing();

        input_templateName.gameObject.SetActive(mode == RecognizerMode.TEMPLATE);
        txt_recognitionResult.gameObject.SetActive(mode == RecognizerMode.RECOGNITION);

        m_drawable.gameObject.SetActive(mode != RecognizerMode.REVIEW);
        m_templateReviewer.SetVisibility(mode == RecognizerMode.REVIEW);
        m_recogntionPanel.gameObject.SetActive(mode == RecognizerMode.RECOGNITION);
    }

    private void OnDrawFinished(DollarPoint[] points)
    {
        if(m_mode == RecognizerMode.TEMPLATE)
        {
            //Save Template
            GestureTemplate newTemplate = new GestureTemplate(_templateName, points);
            GestureTemplate preparedTemplate = new GestureTemplate(_templateName, m_currentRecognizer.Normalize(points, 64));
            m_templates.RawTemplates.Add(newTemplate);
            m_templates.ProceedTemplates.Add(preparedTemplate);
        }
        else
        {
            //Do Recognition
            (string, float) result = m_currentRecognizer.DoRecognition(points, 64, m_templates.RawTemplates);
            string resultText = "";
            switch (m_currentRecognizer)
            {

            }
            txt_recognitionResult.text = resultText;
        }
    }

    private void OnApplicationQuit()
    {
        m_templates.Save();
    }
}
