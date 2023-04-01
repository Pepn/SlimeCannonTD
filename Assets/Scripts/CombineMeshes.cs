using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMeshes : MonoBehaviour
{

    private void Start()
    {
        CreateNewCombinedMesh();
    }

    /// <summary>
    /// Make sure the mesh origin is in the center of all the meshes, as this will be the new object space origin
    /// </summary>
    private void CreateNewCombinedMesh()
    {

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix * transform.worldToLocalMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }


        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        transform.gameObject.SetActive(true);
        transform.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }
}
