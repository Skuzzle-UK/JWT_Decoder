namespace JWT_decoder.Helpers;

public static class DarkMessageBox
{
    public static void Show(string message, string title, MessageBoxIcon icon)
    {
        using Form dialog = new()
        {
            Text = title,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(220, 220, 220),
            Width = 400,
            Height = 180,
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowIcon = false
        };

        DarkTitleBar.ApplyTo(dialog);

        Label iconLabel = new()
        {
            Text = icon switch
            {
                MessageBoxIcon.Error => "✖",
                MessageBoxIcon.Warning => "⚠",
                MessageBoxIcon.Information => "ℹ",
                _ => "ℹ"
            },
            Font = new Font("Segoe UI", 32F, FontStyle.Bold),
            ForeColor = icon switch
            {
                MessageBoxIcon.Error => Color.FromArgb(255, 100, 100),
                MessageBoxIcon.Warning => Color.FromArgb(255, 200, 100),
                MessageBoxIcon.Information => Color.FromArgb(100, 180, 255),
                _ => Color.FromArgb(100, 180, 255)
            },
            Location = new Point(20, 15),
            Width = 50,
            Height = 60,
            TextAlign = ContentAlignment.MiddleCenter
        };
        dialog.Controls.Add(iconLabel);

        Label messageLabel = new()
        {
            Text = message,
            Location = new Point(85, 20),
            Width = 290,
            Height = 70,
            ForeColor = Color.FromArgb(220, 220, 220),
            Font = new Font("Segoe UI", 10F),
            AutoSize = false
        };
        dialog.Controls.Add(messageLabel);

        Button okButton = new()
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(150, 100),
            Width = 100,
            Height = 30,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        okButton.FlatAppearance.BorderColor = Color.Black;
        okButton.FlatAppearance.BorderSize = 2;
        okButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 142, 224);
        okButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 82, 164);
        dialog.Controls.Add(okButton);

        dialog.AcceptButton = okButton;
        dialog.ShowDialog();
    }
}
