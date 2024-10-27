using Godot;
using System;

public partial class LogPanel : Panel
{
	// Référence au Label
	private Label label;

	public override void _Ready()
	{
		// Récupère le Label à l'intérieur du Panel
		label = GetNode<Label>("Label");

		// Ajuste la taille initiale du panel en fonction de la taille du label
		UpdatePanelSize();
	}

	// Fonction pour mettre à jour la taille du panel
	private void UpdatePanelSize()
	{
		Vector2 labelSize = label.GetRect().Size; // Récupère la taille réelle du label
		float padding = 10.0f;

		// Ajuste la taille du panel en fonction de la taille du label et ajoute le padding
		this.CustomMinimumSize = new Vector2(labelSize.X + padding, labelSize.Y + padding);
	}



	public override void _Process(double delta)
	{
		// Vérifie si la taille du label a changé
		if (this.CustomMinimumSize != label.GetMinimumSize())
		{
			UpdatePanelSize();
		}
	}
}
