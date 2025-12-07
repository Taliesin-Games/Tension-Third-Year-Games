using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    [SerializeField] List<LineRenderer> lineRenderers = new List<LineRenderer>();
    [SerializeField] float lifeTime;

    public void SetPos(Vector3 startPos, Vector3 endPos)
    {
        if (lineRenderers.Count > 0)
        {
            for(int i = 0; i < lineRenderers.Count; i++)
            {
                if(lineRenderers[i].positionCount >= 2)
                {
                    lineRenderers[i].SetPosition(0, startPos);
                    lineRenderers[i].SetPosition(1, endPos);
                }
            }
        }
    }

    public void SetLifetime(float time)
    {
        lifeTime = time;
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
