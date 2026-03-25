using Godot;
using System;
using System.Collections.Generic;

public enum SoundEffect
{
	défilement_dialogues,
	UI_button,
	click_piratage_slider,
	click_piratage_digicode,
click_piratage_levels,
	déblocage_piratage,
	explosion_building,
	tremblement_de_terre,

}

public partial class SoundManager : Node
{
	public static SoundManager Instance { get; private set; }

	private AudioStreamPlayer _musicPlayer;
	private Dictionary<SoundEffect, AudioStream> _sfx = new();

	public override void _Ready()
	{
		Instance = this;

		_musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
		if (_musicPlayer == null)
			GD.PrintErr("[SoundManager] MusicPlayer node not found!");
		else
        GD.Print("[SoundManager] MusicPlayer found.");
		// Chargement automatique de tous les sons de l'enum
		foreach (SoundEffect sfx in Enum.GetValues(typeof(SoundEffect)))
		{
			string path = $"res://Sounds/{sfx.ToString().ToLower()}.mp3";
			var stream = GD.Load<AudioStream>(path);

			if (stream != null)
			{
				_sfx[sfx] = stream;
			}
			else
			{
				GD.PrintErr($"[SoundManager] Fichier audio non trouvé : {path}");
			}
		}
	}

	//  Jouer une musique de fond
	public void PlayMusic()
	{
		/*if (!_musicPlayer.Playing)
			_musicPlayer.Play();*/
	}

	public void StopMusic()
	{
		/*if (_musicPlayer.Playing)
			_musicPlayer.Stop();*/
	}

	// Jouer un effet sonore
	public void PlaySound(SoundEffect effect)
	{
		if (!_sfx.ContainsKey(effect))
		{
			GD.PrintErr($"[SoundManager] Effet sonore manquant : {effect}");
			return;
		}

		var player = new AudioStreamPlayer();
		AddChild(player);
		player.Stream = _sfx[effect];
		player.Play();

		// Supprimer le player une fois le son joué
		var timer = new Timer { OneShot = true, WaitTime = (float)player.Stream.GetLength() };
		AddChild(timer);
		timer.Timeout += () =>
		{
			player.QueueFree();
			timer.QueueFree();
		};
		timer.Start();
	}

	public void ChangeMusic(string path)
	{
		_musicPlayer.Stop();
		_musicPlayer.Stream = GD.Load<AudioStream>(path);
		_musicPlayer.Play();
	}
}
