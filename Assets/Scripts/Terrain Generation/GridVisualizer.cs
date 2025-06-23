using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public Grid _grid;
    public PlacementGrid _placeGrid = new PlacementGrid(new Grid.Point(10,10,10), 2f);

    private int globalTester = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        globalTester = 0;
        StartCoroutine(Tester());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator Tester()
    {
        while (true)
        {
            globalTester++;
            globalTester %= _placeGrid.NumPoints;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _grid.NumPoints; i++)
        {
            //Gizmos.color =;
            //Gizmos.DrawWireSphere(_grid[i] + transform.position, 0.1f);
        }

        for (int i = 0; i < _placeGrid.NumPoints; i++)
        {
            if (i == globalTester)
            {
                Gizmos.color = Color.green;
                Debug.Log(_placeGrid.NonLinearIndexMapping(i));
            }
            else
            {
                Gizmos.color = _placeGrid.GetPointState(i) switch
                {
                    PlacementGrid.GridVoxelState.air => Color.white,
                    PlacementGrid.GridVoxelState.unoccupied => Color.yellow,
                    PlacementGrid.GridVoxelState.occupied => Color.red,
                    _ => Color.magenta
                };
            }
            

            Gizmos.DrawWireSphere(_placeGrid[i] + transform.position, 0.1f);
        }
    }
}
