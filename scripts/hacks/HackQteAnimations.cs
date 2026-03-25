using Godot;
using System;
using Cypherless;

public partial class HackQteAnimations : TextureRect, IHack
{
	private TextureRect tick1;
	private TextureRect tick2;
	private TextureRect tick3;
	private TextureRect tick4;
	private TextureRect tick5;
	
	private TextureRect area1;
	private TextureRect area2;
	private TextureRect area3;
	private TextureRect area4;
	private TextureRect area5;
	
	private TextureRect wrong_area1;
	private TextureRect wrong_area2;
	private TextureRect wrong_area3;
	private TextureRect wrong_area4;
	private TextureRect wrong_area5;
	
	private AnimationPlayer _tick1_anim;
	private AnimationPlayer _tick2_anim;
	private AnimationPlayer _tick3_anim;
	private AnimationPlayer _tick4_anim;
	private AnimationPlayer _tick5_anim;
	
	private bool _tick1_succeed;
	private bool _tick2_succeed;
	private bool _tick3_succeed;
	private bool _tick4_succeed;
	private bool _tick5_succeed;

	public bool Done { get; set; } = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		string constant = "VBoxContainer/HBoxContainer";
		_tick1_anim = GetNode<AnimationPlayer>(constant + "1/tick1_anim");
		_tick2_anim = GetNode<AnimationPlayer>(constant + "2/tick2_anim");
		_tick3_anim = GetNode<AnimationPlayer>(constant + "3/tick3_anim");
		_tick4_anim = GetNode<AnimationPlayer>(constant + "4/tick4_anim");
		_tick5_anim = GetNode<AnimationPlayer>(constant + "5/tick5_anim");
		
		tick1 = GetNode<TextureRect>("tick_1");
		tick2 = GetNode<TextureRect>("tick_2");
		tick3 = GetNode<TextureRect>("tick_3");
		tick4 = GetNode<TextureRect>("tick_4");
		tick5 = GetNode<TextureRect>("tick_5");
		
		area1 = GetNode<TextureRect>("area_1");
		area2 = GetNode<TextureRect>("area_2");
		area3 = GetNode<TextureRect>("area_3");
		area4 = GetNode<TextureRect>("area_4");
		area5 = GetNode<TextureRect>("area_5");
		
		wrong_area1 = area1.GetNode<TextureRect>("wrong");
		wrong_area2 = area2.GetNode<TextureRect>("wrong");
		wrong_area3 = area3.GetNode<TextureRect>("wrong");
		wrong_area4 = area4.GetNode<TextureRect>("wrong");
		wrong_area5 = area5.GetNode<TextureRect>("wrong");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (AllTickSucceed() && !Done)
		{
			SoundManager.Instance.PlaySound(SoundEffect.déblocage_piratage);
			Done = true;
			
		}
	}
	private void PlayUIButtonSound()
	{
		SoundManager.Instance.PlaySound(SoundEffect.click_piratage_slider);
	}

	bool AllTickSucceed()
	{
		return _tick1_succeed && _tick2_succeed && _tick3_succeed && _tick4_succeed && _tick5_succeed;
	}
	
	void _on_button_1_pressed()
	{
		PlayUIButtonSound();
		if (_tick1_anim.IsPlaying())
		{
			_tick1_anim.Pause();
			if (area1.Position.X <= tick1.Position.X && tick1.Position.X + tick1.Size.X <= area1.Position.X + area1.Size.X)
			{
				wrong_area1.Visible = false;
				_tick1_succeed = true;
			}
			else
			{
				wrong_area1.Visible = true;
				_tick1_succeed = false;
			}
		}
		else
		{
			_tick1_anim.Play();
			wrong_area1.Visible = false;
			_tick1_succeed = false;
		}
	}
	
	void _on_button_2_pressed()
	{
		PlayUIButtonSound();
		if (_tick2_anim.IsPlaying())
		{
			_tick2_anim.Pause();
			if (area2.Position.X <= tick2.Position.X && tick2.Position.X + tick2.Size.X <= area2.Position.X + area2.Size.X)
			{
				wrong_area2.Visible = false;
				_tick2_succeed = true;
			}
			else
			{
				wrong_area2.Visible = true;
				_tick2_succeed = false;
			}
		}
		else
		{
			_tick2_anim.Play();
			wrong_area2.Visible = false;
			_tick2_succeed = false;
		}
	}
	
	void _on_button_3_pressed()
	{
		PlayUIButtonSound();
		if (_tick3_anim.IsPlaying())
		{
			_tick3_anim.Pause();
			if (area3.Position.X <= tick3.Position.X && tick3.Position.X + tick3.Size.X <= area3.Position.X + area3.Size.X)
			{
				wrong_area3.Visible = false;
				_tick3_succeed = true;
			}
			else
			{
				wrong_area3.Visible = true;
				_tick3_succeed = false;
			}
		}
		else
		{
			_tick3_anim.Play();
			wrong_area3.Visible = false;
			_tick3_succeed = false;
		}
	}
	
	void _on_button_4_pressed()
	{
		PlayUIButtonSound();
		if (_tick4_anim.IsPlaying())
		{
			_tick4_anim.Pause();
			if (area4.Position.X <= tick4.Position.X && tick4.Position.X + tick4.Size.X <= area4.Position.X + area4.Size.X)
			{
				wrong_area4.Visible = false;
				_tick4_succeed = true;
			}
			else
			{
				wrong_area4.Visible = true;
				_tick4_succeed = false;
			}
		}
		else
		{
			_tick4_anim.Play();
			wrong_area4.Visible = false;
			_tick4_succeed = false;
		}
	}
	
	void _on_button_5_pressed()
	{
		PlayUIButtonSound();
		if (_tick5_anim.IsPlaying())
		{
			_tick5_anim.Pause();
			if (area5.Position.X <= tick5.Position.X && tick5.Position.X + tick5.Size.X <= area5.Position.X + area5.Size.X)
			{
				wrong_area5.Visible = false;
				_tick5_succeed = true;
			}
			else
			{
				wrong_area5.Visible = true;
				_tick5_succeed = false;
			}
		}
		else
		{
			_tick5_anim.Play();
			wrong_area5.Visible = false;
			_tick5_succeed = false;
		}
	}
}
