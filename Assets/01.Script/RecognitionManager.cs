using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class RecognitionManager : MonoBehaviour
{
    [SerializeField] private Drawable m_drawable;
    [SerializeField] private RecognitionPanel m_recogntionPanel;
    [SerializeField] private TemplateReviewer m_templateReviewer;
    private GestureTemplates m_templates => GestureTemplates.Get();
    private static Dollar1Recognizer m_dollar1Recognizer = new Dollar1Recognizer("$1 Recognizer"); 
    private IRecognizer m_currentRecognizer;
    [SerializeField] private RecognizerMode m_mode = RecognizerMode.TEMPLATE;

    public static List<IRecognizer> m_recognizerList = new List<IRecognizer>();

    private string _templateName => input_templateName.text;

    [SerializeField] private Button btn_recognizeMode;
    [SerializeField] private Button btn_templateMode;
    [SerializeField] private Button btn_reviewMode;
    [SerializeField] private TextMeshProUGUI txt_recognitionResult;
    [SerializeField] private TMP_InputField input_templateName;

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
        m_recognizerList.Add(m_dollar1Recognizer);

        btn_recognizeMode.onClick.AddListener(() => SetMode(RecognizerMode.RECOGNITION));
        btn_templateMode.onClick.AddListener(() => SetMode(RecognizerMode.TEMPLATE));
        btn_reviewMode.onClick.AddListener(() => SetMode(RecognizerMode.REVIEW));

        m_recogntionPanel.Initialize(SwitchRecognitionAlgorithm, m_recognizerList);
        m_drawable.OnDrawFinished += OnDrawFinished;

        SetMode(m_mode);
    }
    
    private void SwitchRecognitionAlgorithm(int algorithm)
    {
        m_currentRecognizer = m_recognizerList[algorithm];
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
            m_templates.RawTemplates.Add(newTemplate);
            //GestureTemplate preparedTemplate = new GestureTemplate(_templateName, m_currentRecognizer.Normalize(points, 64));
            //m_templates.ProceedTemplates.Add(preparedTemplate);
        }
        else if(m_mode == RecognizerMode.RECOGNITION)
        {
            //Do Recognition
            if (m_currentRecognizer == null)
            {
                Debug.LogError("currentRecognizer is null");
                return;
            }
            (string, float) result = m_currentRecognizer.DoRecognition(points, 64, m_templates.RawTemplates);
            string resultText = "";

            if(m_currentRecognizer is Dollar1Recognizer)
            {
                resultText = $"Recognized: {result.Item1}, Score: {result.Item2}";
            }

            txt_recognitionResult.text = resultText;
            Debug.Log(resultText);
        }
    }

    private void OnApplicationQuit()
    {
        m_templates.Save();
    }
}
