using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Cypherless;

public partial class HackDigicode : Control, IHack
{
	public List<int> Wanted = new List<int>() { 0,0,0,0};
	
	private Queue<int> inputs;

	private double startedRed = 5;
	
	public bool Done { get; set; }= false;
	private void PlayUIButtonSound()
	{
		SoundManager.Instance.PlaySound(SoundEffect.click_piratage_digicode);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		inputs = new Queue<int>();
		UnsetAllWrong();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (startedRed < 0.5)
		{
			startedRed += delta;
			if (startedRed >= 0.5)
				UnsetAllWrong();
		}
		// To delete
		if (Done)
		{
			QueueFree();
			SoundManager.Instance.PlaySound(SoundEffect.déblocage_piratage);
		}
	}

	void SetAllWrong()
	{
		string constant = "background/GridContainer/button";
		for (int i = 0; i < 10; i++)
			GetNode<TextureRect>(constant + i + "/texture_wrong").Visible = true;
		GetNode<TextureRect>(constant + "A/texture_wrong").Visible = true;
		GetNode<TextureRect>(constant + "B/texture_wrong").Visible = true;
	}
	
	void UnsetAllWrong()
	{
		string constant = "background/GridContainer/button";
		for (int i = 0; i < 10; i++)
			GetNode<TextureRect>(constant + i + "/texture_wrong").Visible = false;
		GetNode<TextureRect>(constant + "A/texture_wrong").Visible = false;
		GetNode<TextureRect>(constant + "B/texture_wrong").Visible = false;
	}
	
	private bool AllDone()
	{
		var WantedCount = Wanted.Count;
		var LCount = inputs.Count;
		if (LCount != WantedCount)
			return false;
		var L = inputs.ToList();
		int i = 0;
		while (i < LCount && L[i] == Wanted[i])
			i++;
		return i == LCount;
	}
	
	private void AddInput(int value)
	{
		inputs.Enqueue(value);
		
		if (inputs.Count >= Wanted.Count)
		{
			Done = AllDone();
			if (!Done)
			{
				inputs = new Queue<int>();
				SetAllWrong();
				startedRed = 0;
			}
		}
	}
	
	void _on_button_1_pressed()
	{
		PlayUIButtonSound();
		AddInput(1);
	}
	void _on_button_2_pressed()
	{
		PlayUIButtonSound();
		AddInput(2);
	}
	void _on_button_3_pressed()
	{
		PlayUIButtonSound();
		AddInput(3);
	}
	void _on_button_4_pressed()
	{
		PlayUIButtonSound();
		AddInput(4);
	}
	void _on_button_5_pressed()
	{
		PlayUIButtonSound();
		AddInput(5);
	}
	void _on_button_6_pressed()
	{
		PlayUIButtonSound();
		AddInput(6);
	}
	void _on_button_7_pressed()
	{
		PlayUIButtonSound();
		AddInput(7);
	}
	void _on_button_8_pressed()
	{
		PlayUIButtonSound();
		AddInput(8);
	}
	void _on_button_9_pressed()
	{
		PlayUIButtonSound();
		AddInput(9);
	}
	void _on_button_a_pressed()
	{
		PlayUIButtonSound();
		AddInput(10);
	}
	void _on_button_0_pressed()
	{
		PlayUIButtonSound();
		AddInput(0);
	}
	void _on_button_b_pressed()
	{
		PlayUIButtonSound();
		AddInput(11);
	}

}
