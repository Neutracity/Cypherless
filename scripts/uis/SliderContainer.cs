using Godot;

public partial class SliderContainer : HBoxContainer
{
	private Label _percentageLabel;
	private HSlider _slider;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_slider = GetNode<HSlider>("PanelContainer/slider");
		_percentageLabel = GetNode<Label>("slider_value");

		_on_slider_value_changed((float)_slider.Value);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_slider_value_changed(float value)
	{
		var percentage = Mathf.RoundToInt((value - _slider.MinValue) / (_slider.MaxValue - _slider.MinValue) * 100);
		_percentageLabel.Text = $" {percentage} %";
	}
}
