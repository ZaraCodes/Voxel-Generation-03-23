using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCubeLineDrawer : MonoBehaviour
{
    private Vector3 cubePosition;
    public Vector3 CubePosition { get { return cubePosition; } 
        set 
        {
            cubePosition = value;
            CubeEnabled = true;
        }
    }

    [SerializeField] private float offset;

    public bool CubeEnabled { get; set; }

    [SerializeField] private Material lineMaterial;
    public void DrawCube()
    {
        if (CubeEnabled)
        {
            lineMaterial.SetPass(0);
            
            //top
            GL.Begin(GL.LINES);
            GL.Color(lineMaterial.color);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + offset, CubePosition.z - offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + offset, CubePosition.z + Vector3.forward.z + offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            //bottom
            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z - offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z + Vector3.forward.z + offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            //vertical
            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z - offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + offset, CubePosition.z - offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z - offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + offset, CubePosition.z + Vector3.forward.z + offset);
            GL.Vertex3(CubePosition.x - offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();

            GL.Begin(GL.LINES);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + offset, CubePosition.z + Vector3.forward.z + offset);
            GL.Vertex3(CubePosition.x + Vector3.right.x + offset, CubePosition.y + Vector3.down.y - offset, CubePosition.z + Vector3.forward.z + offset);
            GL.End();
        }
    }

    void OnPostRender()
    {
        DrawCube();
    }
}
