using System.Collections.Generic;

public interface IRecognizer
{
    public string GetName();
    public DollarPoint[] Normalize(DollarPoint[] points, int n);

    public (string, float) DoRecognition(DollarPoint[] points, int n, 
        List<RecognitionManager.GestureTemplate> gestureTemplates);
}
