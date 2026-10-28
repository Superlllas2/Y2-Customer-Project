using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Graph helpers for the pipe chain (previousPipe / nextPipe).
/// </summary>
public static class PipeNetwork
{
    public static bool HasPathToSource(Pipe start, float fallbackSourceRadius = 1.5f)
    {
        if (!start)
        {
            return false;
        }

        var visited = new HashSet<Pipe>();
        var queue = new Queue<Pipe>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var pipe = queue.Dequeue();
            if (!pipe || !visited.Add(pipe))
            {
                continue;
            }

            if (pipe.attachedSource || IsNearSource(pipe, fallbackSourceRadius))
            {
                return true;
            }

            if (pipe.nextPipe)
            {
                queue.Enqueue(pipe.nextPipe);
            }

            if (pipe.previousPipe)
            {
                queue.Enqueue(pipe.previousPipe);
            }
        }

        return false;
    }

    public static void DisconnectFrom(Pipe initialPipe)
    {
        if (!initialPipe)
        {
            return;
        }

        var visited = new HashSet<Pipe>();
        var queue = new Queue<Pipe>();
        queue.Enqueue(initialPipe);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!current || !visited.Add(current))
            {
                continue;
            }

            var next = current.nextPipe;
            var prev = current.previousPipe;

            current.nextPipe = null;
            current.previousPipe = null;
            current.attachedSource = null;
            current.isConnected = false;
            current.isSnapped = false;
            current.SetHeldState(false);

            if (next)
            {
                queue.Enqueue(next);
            }

            if (prev)
            {
                queue.Enqueue(prev);
            }
        }
    }

    private static bool IsNearSource(Pipe pipe, float fallbackRadius)
    {
        foreach (var end in GetEnds(pipe))
        {
            if (!end)
            {
                continue;
            }

            var hits = Physics.OverlapSphere(end.position, fallbackRadius);
            foreach (var hit in hits)
            {
                if (!hit)
                {
                    continue;
                }

                var source = hit.GetComponentInParent<PipeNetworkSource>();
                if (source)
                {
                    var radius = source.connectRadius > 0f ? source.connectRadius : fallbackRadius;
                    var closest = hit.ClosestPoint(end.position);
                    if (Vector3.Distance(end.position, closest) <= radius)
                    {
                        return true;
                    }
                }

                if (hit.CompareTag("Water"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static Transform[] GetEnds(Pipe pipe)
    {
        return new[] { pipe.startEnd, pipe.endEnd };
    }
}
