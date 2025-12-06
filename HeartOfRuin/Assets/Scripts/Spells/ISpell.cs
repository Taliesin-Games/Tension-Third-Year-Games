using System;
using UnityEngine;

public interface ISpell
{
    string SpellName { get; }
    int ManaCost { get; }

    void Cast(SpellContext context);
}

