using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private GameObject brush;

    LineRenderer currentLineRenderer;

    Vector2 lastPos;

    private void Update()
    {
        Drawing();
    }

    void Drawing()
    {
        if(Input.GetMouseButtonDown(0))
        {
            CreateBrush();
        }
        if(Input.GetMouseButton(0))
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            if(mousePos != lastPos )
            {
                AddPoint(mousePos);
                lastPos = mousePos; 
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            currentLineRenderer = null;
        }
    }

    void CreateBrush()
    {
        GameObject brushInstance = Instantiate(brush, this.transform);
        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);

        currentLineRenderer.SetPosition(0, mousePos);
        currentLineRenderer.SetPosition(1, mousePos);
    }


    void AddPoint(Vector2 pointPos)
    {
        int positionIndex = currentLineRenderer.positionCount;
        currentLineRenderer.positionCount++;
        currentLineRenderer.SetPosition(positionIndex, pointPos);
    }
}
