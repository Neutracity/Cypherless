using Godot;

namespace Cypherless.StateMachine;

public abstract partial class State : Node
{
    [Export] public virtual string AnimationName { get; set; }
    [Export] float move_speed { get; set; }

    private int gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsInt32();

    private Character parent;

    public virtual void enter()
    {
    }

    public virtual void exit()
    {
        
    }

    public virtual State process_input(InputEvent inputEvent)
    {
        return null;
    }

    public virtual State process_frame(double delta)
    {
        return null;
    }

    public virtual State process_physics(float delta)
    {
        return null;
    }
}