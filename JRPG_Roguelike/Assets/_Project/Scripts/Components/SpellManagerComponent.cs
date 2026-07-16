using System.Collections.Generic;
using UnityEngine;

public class SpellManagerComponent : MonoBehaviour
{
    public List<SpellData> Spells = new List<SpellData>();

    public void AddSpell(SpellData spell) => Spells.Add(spell);
    public void RemoveSpell(SpellData spell) => Spells.Remove(spell);
}


