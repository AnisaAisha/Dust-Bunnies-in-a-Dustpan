using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CubePuzzleBuilder : MonoBehaviour {

    [SerializeField] [Range(1, 10)] private int dimension;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private GameObject segmentPrefab;

    public GameObject[,,] CubeConstruct { get; private set; }

    public int Dimension => dimension;
    public float Spacing => spacing;

    private void Awake() {
        BuildCube();
    }

    public void BuildCube() {

        ClearCube();
        
        GameObject[,,] cubes = new GameObject[dimension, dimension, dimension];

        float offset = (dimension - 1) * spacing / 2f;

        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                for (int z = 0; z < dimension; z++)
                {
                    Vector3 position = new Vector3(
                        x * spacing - offset,
                        y * spacing - offset,
                        z * spacing - offset
                    );

                    GameObject cube = Instantiate(segmentPrefab, transform);
                    cube.transform.localPosition = position;

                    cubes[x, y, z] = cube;
                }
            }
        }

        CubeConstruct = cubes;
        
        UpdateColliderBounds();
    }

    public void ToggleCollisions(bool isEnabled) {
        GetComponent<BoxCollider>().enabled = isEnabled;
    }

    private void ClearCube()
    {
        if (CubeConstruct == null)
        {
            WipeChildren(transform);
            return;
        }

        foreach (GameObject segment in CubeConstruct)
        {
            if (segment != null)
                if (!Application.isPlaying) DestroyImmediate(segment);
                else Destroy(segment);
        }

        CubeConstruct = null;
    }
    
    private static void WipeChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            if (parent.GetChild(i).GetComponent<CubePiece>() != null) DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
    
    private void UpdateColliderBounds()
    {
        BoxCollider col = GetComponent<BoxCollider>();

        float size = dimension * spacing;

        col.size = new Vector3(size, size, size);
        col.center = Vector3.zero;
    }
}
