using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public Grid _grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(_grid.NumPoints + " || " + _grid.Dimensions);

        for (int i = 0; i < _grid.NumPoints; i++)
        {
            Debug.Log(_grid[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _grid.NumPoints; i++)
        {
            //Gizmos.color =;
            Gizmos.DrawSphere(_grid[i] + transform.position, 0.1f);
        }
    }
}
