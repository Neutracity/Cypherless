using Godot;
using System;
using Cypherless;

public partial class HackLevels : Control, IHack
{
	private Label percentageLevel1;
	private Label percentageLevel2;
	private Label percentageLevel3;
	private Label percentageLevel4;
	
	private bool done1 = false;
	private bool done2 = false;
	private bool done3 = false;
	private bool done4 = false;
	
	public int[] Conditions = new int[4] { 10, 20, 30, 40 };

	public bool Done { get; set; } = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		string path = "background/levels/level";
		percentageLevel1 = GetNode<Label>(path + "1/percentage");
		percentageLevel1.Text = "30 %";
		percentageLevel2 = GetNode<Label>(path + "2/percentage");
		percentageLevel2.Text = "30 %";
		percentageLevel3 = GetNode<Label>(path + "3/percentage");
		percentageLevel3.Text = "30 %";
		percentageLevel4 = GetNode<Label>(path + "4/percentage");
		percentageLevel4.Text = "30 %";
		
		_on_slider_value_changed_1(30f);
		_on_slider_value_changed_2(30f);
		_on_slider_value_changed_3(30f);
		_on_slider_value_changed_4(30f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (AllDone())
		{
			SoundManager.Instance.PlaySound(SoundEffect.déblocage_piratage);
			Done = true;
		}
		// To delete
		if (Done)
			QueueFree();
	}

	bool AllDone()
	{
		return done1 && done2 && done3 && done4;
	}
	
	public void _on_slider_value_changed_1(float input)
	{
		int value = (int)input;
		percentageLevel1.Text = value + " %";
		done1 = value == Conditions[0];
		SoundManager.Instance.PlaySound(SoundEffect.click_piratage_levels);
	}

	public void _on_slider_value_changed_2(float input)
	{
		int value = (int)input;
		percentageLevel2.Text = value + " %";
		done2 = value == Conditions[1];
		SoundManager.Instance.PlaySound(SoundEffect.click_piratage_levels);
	}

	public void _on_slider_value_changed_3(float input)
	{
		int value = (int)input;
		percentageLevel3.Text = value + " %";
		done3 = value == Conditions[2];
		SoundManager.Instance.PlaySound(SoundEffect.click_piratage_levels);
	}

	public void _on_slider_value_changed_4(float input)
	{
		int value = (int)input;
		percentageLevel4.Text = value + " %";
		done4 = value == Conditions[3];
		SoundManager.Instance.PlaySound(SoundEffect.click_piratage_levels);
	}
}
