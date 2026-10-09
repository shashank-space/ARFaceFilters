using UnityEngine;

public class FilterManager : MonoBehaviour
{
    [Header("Available Filters")]
    public GameObject[] filters;

    private int currentFilter = 0;

    void Start()
    {
        ShowFilter(0);
    }

    public void ShowFilter(int index)
    {
        if (filters == null || filters.Length == 0)
            return;

        if (index < 0 || index >= filters.Length)
            return;

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i] != null)
                filters[i].SetActive(i == index);
        }

        currentFilter = index;
    }

    public void NextFilter()
    {
        if (filters == null || filters.Length == 0)
            return;

        int next = (currentFilter + 1) % filters.Length;
        ShowFilter(next);
    }
}
