using Godot;

public partial class Tooltip : Panel
{
    private Label tooltipLabel;

    public override void _Ready()
    {
        tooltipLabel = GetNode<Label>("Label");
        Visible = false;
    }

    public void ShowTooltip(string text, Vector2 position)
    {
        tooltipLabel.Text = text;
        GlobalPosition = position; 
        Visible = true;
    }

    public void HideTooltip()
    {
        Visible = false;
    }
}
