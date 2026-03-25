using DwarfImpulse;
using Godot;

public partial class ShakeController : Camera3D
{
    [Export] private FastNoiseLite _noise;
    [Export] private ShakeDirector3D _shakeDirector;
    [Export] public float CameraDistance;

    private Marker3D followTarget;
    private Marker3D lookTarget;
    private Character playerCharacter; // Changer le type
    private Marker3D runningTarget;

    public override void _Ready()
    {
        followTarget = GetParent().GetNode<Marker3D>("FollowTarget");
        lookTarget = GetParent().GetNode<Marker3D>("LookTarget");
        runningTarget = GetParent().GetNode<Marker3D>("RunningTarget");
        _noise = new FastNoiseLite();
        playerCharacter = GetParent<Character>(); // Obtenir le parent comme Character
    }

    public override void _Process(double delta)
    {
        GlobalPosition =
            Position.Lerp(followTarget.GlobalPosition + new Vector3(0, 0, CameraDistance), (float)delta * 7);
        lookTarget.GlobalPosition = lookTarget.GlobalPosition.Lerp(lookTarget.GlobalPosition, (float)delta * 7);
        LookAt(lookTarget.GlobalPosition);
    }

    public void OnGunHaveShoot()
    {
        _shakeDirector.Shake(NoiseShake.CreateWithNoise(_noise)
            .WithDuration(0.15f)
            .WithEulersAmount(new Vector3(0.05f, -0.1f, 0.01f)));
    }

    public void OnHit()
    {
        _shakeDirector.Shake(NoiseShake.CreateWithNoise(_noise)
            .WithDuration(0.15f)
            .WithEulersAmount(new Vector3(0.1f, -0.1f, 0.01f)));
    }
}