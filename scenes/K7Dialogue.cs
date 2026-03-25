using Godot;
using System;
using Cypherless;
using Godot.Collections;

public partial class K7Dialogue : Node3D, IInteractable
{
    
    private Dictionary<int, string> dialogue = new()
    {
        { 0, "Attention ! La barrière est électrifié, je peux t'aider à la pirater il suffit juste que tu appuie sur les boutons au bon moment ! Tu devrais appuyer sur E pour la pirater" },
        { 2, "Vous n'avez pas le droit d'êtres ici !"},
        {3, "On m'as dit que le code n'était pas très sécurisé "},
        {4,"Saluuuuuut, Je m'appelle K7, je vais t'accompagner dans ton incroyable aventure dans CYPHERLESS !"},
        {5,"Commençons par les bases, Pour te déplacer de droite à gauche avec les touches Q et D, Espace pour sauter, et Maj pour courir"},
        {6,"Maintenant voila comment utiliser une arme, tu as juste à marcher dessus pour la ramasser (à savoir que tu peux en garder 2 sur toi et elle seront échangé automatiquement)"},
        {7,"Enfin, pour tirer avec celle-ci il suffit de faire un clique gauche, si tu n'aimes pas l'arme tu peux la jeter avec la touche F"}
        
    };
    private Dictionary<int, string> pnjNames = new()
    {
        { 0, "K7" },
        { 2, "Soldat"},
        {3,"Didier"},
        {4,"K7"},
        {5,"K7"},
        {6,"K7"},
        {7,"K7"},
    };
    private DialogueBox dialoguebox;
    private PackedScene dialogueboxScene = GD.Load<PackedScene>("res://scenes/UIs/dialogue.tscn");

    public override void _Ready()
    {
        dialoguebox = dialogueboxScene.Instantiate() as DialogueBox;
        GetTree().Root.CallDeferred("add_child", dialoguebox);
    }

    [Export] public int PnjId;


    public void Interact()
    {
        return;
    }

    public void IsNowInteractable()
    {
        dialoguebox.SpeakerName = pnjNames[PnjId]; // ← ICI on envoie le nom
        _ = dialoguebox.ShowDialogueAsync(dialogue[PnjId]);
    }

    public void NoMoreInteractable()
    {
        dialoguebox.HideDialogue();
    }
}
