using System.Collections.Generic;
using UnityEngine;

// Behavioral contract implemented by every generated actor so the central
// scheduler can evaluate its rules in a fixed, deterministic order.
public interface IGameRuleActor
{
    void EvalUpdate();

    void EvalFixedUpdate();
}

// Central scheduler that realizes the inter-actor evaluation order of
// Algorithm 1 in the Unity backend. Actors are evaluated in declaration
// order from a single update call (GameManager), instead of each actor
// relying on its own Update/FixedUpdate, whose cross-actor order Unity
// does not guarantee. Kept in a hand-written class (not in the generated
// GameManager) so that Action.cs can notify it on Spawn/Delete without a
// compile-time dependency on generated code.
public static class ActorScheduler
{
    private struct Entry
    {
        public IGameRuleActor Actor;
        public MonoBehaviour Behaviour;
    }

    private static readonly List<Entry> actors = new List<Entry>();
    private static readonly List<Entry> pendingAdds = new List<Entry>();
    private static readonly List<IGameRuleActor> pendingRemoves = new List<IGameRuleActor>();

    // (Re)builds the ordered active set from the declaration order baked
    // into the GameManager. Called at scene load, before any actor Start,
    // so every initial actor is still active and resolvable by name.
    public static void Build(string[] declarationOrder)
    {
        actors.Clear();
        pendingAdds.Clear();
        pendingRemoves.Clear();

        if (declarationOrder == null) return;
        foreach (string name in declarationOrder)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) continue;
            IGameRuleActor actor = go.GetComponent<IGameRuleActor>();
            if (actor == null) continue;
            actors.Add(new Entry { Actor = actor, Behaviour = actor as MonoBehaviour });
        }
    }

    // Runtime-spawned actors are appended at the end, preserving spawn
    // order (Algorithm 1, line 11). Applied at the end of the current pass
    // so they do not begin evaluating until the next iteration.
    public static void RegisterSpawned(IGameRuleActor actor)
    {
        if (actor == null) return;
        pendingAdds.Add(new Entry { Actor = actor, Behaviour = actor as MonoBehaviour });
    }

    // Deleted actors are removed at the end of the current pass
    // (Algorithm 1, line 10).
    public static void Unregister(IGameRuleActor actor)
    {
        if (actor == null) return;
        pendingRemoves.Add(actor);
    }

    public static void RunUpdate()
    {
        int count = actors.Count; // snapshot: actors spawned this pass run next iteration
        for (int i = 0; i < count; i++)
        {
            Entry e = actors[i];
            if (e.Behaviour != null && e.Behaviour.isActiveAndEnabled)
                e.Actor.EvalUpdate();
        }
        Flush();
    }

    public static void RunFixedUpdate()
    {
        int count = actors.Count; // snapshot: actors spawned this pass run next iteration
        for (int i = 0; i < count; i++)
        {
            Entry e = actors[i];
            if (e.Behaviour != null && e.Behaviour.isActiveAndEnabled)
                e.Actor.EvalFixedUpdate();
        }
        Flush();
    }

    private static void Flush()
    {
        if (pendingRemoves.Count > 0)
        {
            actors.RemoveAll(e => e.Behaviour == null || pendingRemoves.Contains(e.Actor));
            pendingRemoves.Clear();
        }
        if (pendingAdds.Count > 0)
        {
            actors.AddRange(pendingAdds); // append at end, preserving spawn order
            pendingAdds.Clear();
        }
    }
}