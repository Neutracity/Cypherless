using Godot;

public partial class SettingsMenu : Control
{
	private HSlider masterVolume;
	private CheckBox muted;
	private OptionButton resolution;
	private HSlider fpsMax;
	private CheckBox fullscreen;
	private CheckBox vsync;
	private CheckBox mouseTrapped;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		masterVolume = GetNode<HSlider>("TextureRect/ScrollContainer/MarginContainer3/categories/audio/MarginContainer/parameters/master_volume/slider_container/PanelContainer/slider");
		muted = GetNode<CheckBox>("TextureRect/ScrollContainer/MarginContainer3/categories/audio/MarginContainer/parameters/mute");
		resolution = GetNode<OptionButton>("TextureRect/ScrollContainer/MarginContainer3/categories/graphics/MarginContainer/parameters/resolutions/options");
		fpsMax = GetNode<HSlider>("TextureRect/ScrollContainer/MarginContainer3/categories/graphics/MarginContainer/parameters/max_fps/slider_container/PanelContainer/slider");
		fullscreen = GetNode<CheckBox>("TextureRect/ScrollContainer/MarginContainer3/categories/controls/MarginContainer/parameters/fullscreen");
		vsync = GetNode<CheckBox>("TextureRect/ScrollContainer/MarginContainer3/categories/graphics/MarginContainer/parameters/vsync");
		mouseTrapped = GetNode<CheckBox>("TextureRect/ScrollContainer/MarginContainer3/categories/controls/MarginContainer/parameters/trapped_mouse");
		
		
		masterVolume.Value = AudioServer.GetBusVolumeDb(0) == 0 ? 100 : AudioServer.GetBusVolumeDb(0)*5;
		muted.ButtonPressed = AudioServer.IsBusMute(0);
		var vect = DisplayServer.WindowGetSize();
		string resol = $"{vect[0].ToString()}x{vect[1].ToString()}";
		if (resol == "1280x720")
			resolution.Selected = 2;
		else if (resol == "1600x900")
			resolution.Selected = 1;
		else
			resolution.Selected = 0;
		fpsMax.Value = Engine.MaxFps == 0 ? 100 : Engine.MaxFps;
		fullscreen.ButtonPressed = DisplayServer.WindowGetMode(0) == DisplayServer.WindowMode.Fullscreen;
		vsync.ButtonPressed = DisplayServer.WindowGetVsyncMode(0) == DisplayServer.VSyncMode.Enabled;
		mouseTrapped.ButtonPressed = Input.MouseMode == Input.MouseModeEnum.Confined;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetParent().SetProcess(false);
		if (Input.IsActionJustPressed("ui_pause"))
		{
			GD.Print("setting menu escape");
			_on_back_button_pressed();
		}
	}

	public void _on_back_button_pressed()
	{
		QueueFree();
		GetParent().SetProcess(true);
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_master_volume_slider_value_changed(float value)
	{
		AudioServer.SetBusVolumeDb(0, value/5);
	}

	public void _on_mute_toggled(bool mute)
	{
		AudioServer.SetBusMute(0, mute);
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_options_item_selected(int index)
	{
		switch (index)
		{
			case 0:
				DisplayServer.WindowSetSize(new Vector2I(1920, 1080));
				break;
			case 1:
				DisplayServer.WindowSetSize(new Vector2I(1600, 900));
				break;
			case 2:
				DisplayServer.WindowSetSize(new Vector2I(1280, 720));
				break;
		}
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_fps_slider_value_changed(float value)
	{
		Engine.MaxFps = (int)value;
	}

	public void _on_fullscreen_toggled(bool fullscreen)
	{
		if (fullscreen)
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		else
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_vsync_toggled(bool vsync)
	{
		if (vsync)
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
		else
			DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_trapped_mouse_toggled(bool trapped)
	{
		if (trapped)
			Input.MouseMode = Input.MouseModeEnum.Confined;
		else
			Input.MouseMode = Input.MouseModeEnum.Visible;
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}
}
