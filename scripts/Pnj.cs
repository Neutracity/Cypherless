using Cypherless;
using Godot;
using Godot.Collections;

public partial class Pnj : StaticBody3D, IInteractable
{
	private Dictionary<int, string> dialogue = new()
	{
		{ 0, "Hello Bonjour Pouet Pouet aoipdjfopiadfh adsmcfaefxasdfmasudf09ua" },
		{ 1, "Bonjour, je suis David, le plus gros bg de Cypherless, aujourd'hui on va faire des crêpes avec le poto K7, et je vous propose pour cela de commencer par verser la farine dans le bol, puis l'eau, et la pate est prete !"},
		{ 2, "Vous n'avez pas le droit d'etres ici !"},
		{3, "On m'as dit que le code n'était pas très sécurisé "}
	};
	private Dictionary<int, string> pnjNames = new()
	{
		{ 0, "K7" },
		{ 1, "David" },
		{ 2, "Soldat"},
		{3,"Didier"}
	};

	private DialogueBox dialoguebox;
	private PackedScene dialogueboxScene = GD.Load<PackedScene>("res://scenes/UIs/dialogue.tscn");
	private Label3D _interactionLabel;

	private MeshInstance3D mesh;
	private ShaderMaterial nextpass;
	[Export] public int PnjId;

	public override void _Ready()
	{
		mesh = GetNodeOrNull<MeshInstance3D>("Soldat_militaire/Armature/Skeleton3D/006_001");
		if (mesh is null)
			GetNodeOrNull<MeshInstance3D>("Soldat_militaire/Armature/Skeleton3D/006_001");
		nextpass = mesh.GetMaterialOverlay() as ShaderMaterial;
		dialoguebox = dialogueboxScene.Instantiate() as DialogueBox;
		GetTree().Root.CallDeferred("add_child", dialoguebox);
		
		
		// Configuration de l'étiquette d'interaction
		_interactionLabel = new Label3D();
		_interactionLabel.Text = "Appuyez sur E pour interagir";
		_interactionLabel.Position = new Vector3(0, 2.0f, 0);
		_interactionLabel.Billboard = BaseMaterial3D.BillboardModeEnum.FixedY;
		_interactionLabel.Visible = false;
		AddChild(_interactionLabel);
	}

	public void Interact()
	{
		if (dialoguebox.Visible)
		{
			dialoguebox.HideDialogue();
		}
		else
		{
			dialoguebox.SpeakerName = pnjNames[PnjId]; // ← ICI on envoie le nom
			_ = dialoguebox.ShowDialogueAsync(dialogue[PnjId]);
		}
	}

	public void IsNowInteractable()
	{
		nextpass.SetShaderParameter("visible", 1);
		_interactionLabel.Visible = true;
	}

	public void NoMoreInteractable()
	{
		nextpass.SetShaderParameter("visible", 0);
		dialoguebox.HideDialogue();
		_interactionLabel.Visible = false;
	}
}
