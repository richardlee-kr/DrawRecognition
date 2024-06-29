using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawable : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private GameObject m_brush;

    [SerializeField] private float WIDTH = 10;
    [SerializeField] private float HEIGHT = 10;

    LineRenderer m_LineRenderer;
    Vector2 lastPos;

    private List<DollarPoint> m_drawPoints = new List<DollarPoint>();

    public event Action<DollarPoint[]> OnDrawFinished;

    private void Update()
    {
        if(InRange())
        {
            Drawing();
        }
    }

    private void Drawing()
    {
        if(Input.GetMouseButtonDown(0))
        {
            CreateBrush();
        }
        if(Input.GetMouseButton(0))
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            if(mousePos != lastPos)
            {
                AddPoint(mousePos);
                lastPos = mousePos; 
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            m_LineRenderer = null;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Finish");
            OnDrawFinished?.Invoke(m_drawPoints.ToArray());
            ClearDrawing();
        }
    }

    private void CreateBrush()
    {
        GameObject _brushInstance = Instantiate(m_brush, this.transform);
        m_LineRenderer = _brushInstance.GetComponent<LineRenderer>();

        Vector2 _mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);

        m_LineRenderer.SetPosition(0, _mousePos);
        m_LineRenderer.SetPosition(1, _mousePos);
    }


    private void AddPoint(Vector2 pointPos)
    {
        int _positionIndex = m_LineRenderer.positionCount;
        m_LineRenderer.positionCount++;
        m_LineRenderer.SetPosition(_positionIndex, pointPos);

        //m_drawPoints.Add(new DollarPoint(pointPos.x, pointPos.y));
        m_drawPoints.Add(new DollarPoint(pointPos.x*100+500, pointPos.y*100+375));
    }

    private bool InRange()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < transform.position.x - WIDTH / 2)
            return false;
        if (mousePos.x > transform.position.x + WIDTH / 2)
            return false;
        if (mousePos.y < transform.position.y - HEIGHT / 2)
            return false;
        if (mousePos.y > transform.position.y + HEIGHT / 2)
            return false;

        return true;
    }

    public void ClearDrawing()
    {
        m_drawPoints.Clear();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(WIDTH, HEIGHT, 0));
    }
}
