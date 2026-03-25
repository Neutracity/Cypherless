using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
	}
	public void _on_button_solo_pressed()
	{
		Node loading = GD.Load<PackedScene>("res://scenes/UIs/Menus/loading_screen.tscn").Instantiate();
		((LoadingProgress)loading).Path = "res://scenes/Maps/new_city.tscn";
		QueueFree();
		GetTree().Root.AddChild(loading);
	}

	public void _on_button_multiplayer_pressed()
	{
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/multiplayer_menu.tscn").Instantiate());
		QueueFree();
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_button_quit_pressed()
	{
		GetTree().Quit();
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}


	public void _on_button_settings_pressed()
	{
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/settings_menu.tscn").Instantiate());
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_button_credits_pressed()
	{
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/credits_page.tscn").Instantiate());
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}
}
