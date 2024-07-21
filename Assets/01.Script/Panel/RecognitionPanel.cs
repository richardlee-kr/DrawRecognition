using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RecognitionPanel : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dd_algorithmList;

    public void Initialize(Action<int> onAlgorithmChoose, List<IRecognizer> recognizers)
    {
        dd_algorithmList.onValueChanged.AddListener(onAlgorithmChoose.Invoke);

        foreach(IRecognizer recognizer in recognizers)
        {
            dd_algorithmList.options = recognizers.Select(name => new TMP_Dropdown.OptionData(name.GetName())).ToList();
        }

        onAlgorithmChoose.Invoke(0);
    }
}
