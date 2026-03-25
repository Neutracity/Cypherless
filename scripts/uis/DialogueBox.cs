using Godot;
using System.Threading.Tasks;


public partial class DialogueBox : Control
{
    private bool _cancelDialogue = false;
    private Label _dialogueText;

    private Label _nameLabel;

    public string SpeakerName
    {
        get => _nameLabel.Text;
        set => _nameLabel.Text = value;
    }
    public string Text
    {
        get => _dialogueText.Text;
        set => _dialogueText.Text = value;
    }

    [Export] public float CharacterDelay = 0.04f; // délai en secondes entre chaque lettre

    public override void _Ready()
    {
        Visible = false;
        _dialogueText = GetNode<Label>("dialogue_box/MarginContainer/dialogue_text");
        _nameLabel = GetNode<Label>("dialogue_box/name_label"); // ← Assure-toi que ce chemin est correct
        Text = "";
        SpeakerName = "";
    }

    public async Task ShowDialogueAsync(string text)
    {
        _cancelDialogue = false;
        Visible = true;
        Text = "";

        foreach (char c in text)
        {
            if (_cancelDialogue)
                break;
            Text += c;

            if (!char.IsWhiteSpace(c))
                SoundManager.Instance.PlaySound(SoundEffect.défilement_dialogues);
            await ToSignal(GetTree().CreateTimer(CharacterDelay), "timeout");
        }
    }

    public void HideDialogue()
    {
        _cancelDialogue = true;
        Visible = false;
        Text = "";
    }
}