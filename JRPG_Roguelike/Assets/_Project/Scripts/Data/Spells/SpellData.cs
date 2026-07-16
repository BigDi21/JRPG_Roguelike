using UnityEngine;

[CreateAssetMenu(fileName = "NewSpell", menuName = "JRPG/Spell")]
public class SpellData : ScriptableObject
{
    public string spellName;
    public Sprite icon;
    [TextArea] public string description;
    public int manaCost;

    [SerializeReference] public Effect effect; // ссылка на эффект заклинания
}