using UnityEngine;

internal sealed class ChunkShuffleBag
{
    private readonly int[] order;
    private int cursor;
    private int previous = -1;

    public ChunkShuffleBag(int count)
    {
        order = new int[count];
        for (int i = 0; i < count; i++) order[i] = i;
        cursor = count;
    }

    public int Next()
    {
        if (cursor == order.Length)
        {
            for (int i = order.Length - 1; i > 0; i--)
            {
                int other = Random.Range(0, i + 1);
                (order[i], order[other]) = (order[other], order[i]);
            }

            if (order.Length > 1 && order[0] == previous)
            {
                int other = Random.Range(1, order.Length);
                (order[0], order[other]) = (order[other], order[0]);
            }
            cursor = 0;
        }

        previous = order[cursor++];
        return previous;
    }
}
