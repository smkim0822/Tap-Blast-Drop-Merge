using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스포너의 현재 원소와 다음 원소 큐를 관리합니다.
/// </summary>
public class SpawnerQueue
{
    readonly Spawner.ElementData[] elements;
    readonly int previewCount;
    readonly List<Spawner.ElementData> nextElements = new List<Spawner.ElementData>();

    public Spawner.ElementData CurrentElement { get; private set; }
    public IReadOnlyList<Spawner.ElementData> NextElements => nextElements;

    public SpawnerQueue(Spawner.ElementData[] elements, int previewCount)
    {
        this.elements = elements;
        this.previewCount = previewCount;
    }

    public void Reset()
    {
        nextElements.Clear();
        for (int i = 0; i < previewCount; i++)
        {
            nextElements.Add(PickRandomElement());
        }

        Advance();
    }

    public void Advance()
    {
        if (nextElements.Count == 0)
        {
            Reset();
            return;
        }

        int lastIndex = nextElements.Count - 1;
        CurrentElement = nextElements[lastIndex];
        nextElements.RemoveAt(lastIndex);
        nextElements.Insert(0, PickRandomElement());
    }

    Spawner.ElementData PickRandomElement()
    {
        if (elements != null && elements.Length > 0)
        {
            return elements[Random.Range(0, elements.Length)];
        }

        return new Spawner.ElementData();
    }
}
