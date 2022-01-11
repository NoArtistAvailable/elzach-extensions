using UnityEngine;

public class GizmoRenderer : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Mesh mesh;
    [SerializeField] private Color color = Color.green;
    [SerializeField] private bool wire = true;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private Vector3 rotation = Vector3.zero;
    [SerializeField] private Vector3 scale = Vector3.one;
    void OnDrawGizmos()
    {
        if (!mesh) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = color;
        if(wire) Gizmos.DrawWireMesh(mesh, position, Quaternion.Euler(rotation), scale);
        else Gizmos.DrawMesh(mesh, position, Quaternion.Euler(rotation), scale);
    }
#endif
}
