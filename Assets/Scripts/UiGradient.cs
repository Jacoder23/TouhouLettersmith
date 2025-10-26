using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiGradient : BaseMeshEffect
{
    public Color LeftColor;
    public Color RightColor;
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;
        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);

        if (vertices.Count == 0)
            return;

        float leftMostX = vertices[0].position.x;
        float rightMostX = vertices[0].position.x;

        foreach (var vertex in vertices)
        {
            if (vertex.position.x < leftMostX)
                leftMostX = vertex.position.x;
            if (vertex.position.x > rightMostX)
                rightMostX = vertex.position.x;
        }

        float width = rightMostX - leftMostX;

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];
            float normalizedX = (vertex.position.x - leftMostX) / width;
            vertex.color = Color.Lerp(LeftColor, RightColor, normalizedX);
            vertices[i] = vertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }
}
