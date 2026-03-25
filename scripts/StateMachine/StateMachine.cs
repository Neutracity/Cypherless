using Godot;
using System;
using Cypherless.StateMachine;

public partial class StateMachine : Node
{
    [Export] public State InitialState { get; set; }
    public State CurrentState { get; set; }

    public void init(CharacterBody3D parent)
    {
        foreach (Node child in GetChildren())
        {
            child.Reparent(parent);
        }
        
        
    }

    public void changeState(State newState)
    {
        if (CurrentState != null)
        {
            CurrentState.exit();
        }
        CurrentState = newState;
        CurrentState.enter();
    }

    public void process_physics(float delta)
    {
        var newstate = CurrentState.process_physics(delta);
        if (newstate != null)
        {
            changeState(newstate);
        }
    } 
    public void process_input(InputEvent inputEvent)
    {
        var newstate = CurrentState.process_input(inputEvent);
        if (newstate != null)
        {
            changeState(newstate);
        }
    } 
    
    public void process_frame(float delta)
    {
        var newstate = CurrentState.process_frame(delta);
        if (newstate != null)
        {
            changeState(newstate);
        }
    } 
}
