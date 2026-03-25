using Godot;

public partial class RichTextLabel : Godot.RichTextLabel
{
    // Called when the node enters the scene tree for the first time.
    private FileAccess logfile;

    public override void _Ready()
    {
        var logPath = ProjectSettings.GetSetting("debug/file_logging/log_path");

        logfile = FileAccess.Open(logPath.ToString(), FileAccess.ModeFlags.Read);
        /*create_tween().set_loops().tween_callback(UpdateConsole).set_delay(console_update_interval)*/
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        while (logfile.GetPosition() < logfile.GetLength())
        {
            Clear();
            AddText(logfile.GetLine() + "\n");
        }
    }
}