using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "JRPG/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [SerializeReference] public Effect effect;
}