using Godot;

public partial class CreditsPage : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_pause")) _on_back_button_pressed();
    }

    public void _on_back_button_pressed()
    {
        QueueFree();
        SoundManager.Instance.PlaySound(SoundEffect.UI_button);
    }
}