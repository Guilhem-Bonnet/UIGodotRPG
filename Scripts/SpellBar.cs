using Godot;
using System.Collections.Generic;

public partial class SpellBar : HBoxContainer
{
    // Chemin de la scène SpellButton
    private PackedScene spellButtonScene = GD.Load<PackedScene>("res://SpellButton.tscn");

    public void AddSpell(Texture2D icon, float cooldown)
    {
        // Instancier le SpellButton
        SpellButton spellButtonInstance = (SpellButton)spellButtonScene.Instantiate();
        spellButtonInstance.SetSpellIconAndCooldown(icon, cooldown);

        // Connecter le signal de clic en utilisant Callable
        spellButtonInstance.Connect("pressed", Callable.From(() => spellButtonInstance.OnSpellPressed()));

        // Ajouter le SpellButton comme enfant du HBoxContainer
        AddChild(spellButtonInstance);
    }
}
