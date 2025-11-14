using JWT_decoder.Helpers;

namespace JWT_decoder;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        DarkTitleBar.ApplyTo(this);
        jwtInputBox.TextChanged += JwtInputBox_TextChanged;
    }

    private void JwtInputBox_TextChanged(object? sender, EventArgs e)
    {
        JwtTokenColorizer.ApplyTo(jwtInputBox);
    }

    private void decodeBtn_Click(object sender, EventArgs e)
    {
        try
        {
            string jwt = jwtInputBox.Text.Trim();
            if (string.IsNullOrEmpty(jwt))
            {
                DarkMessageBox.Show("Enter a JWT token.", "Error", MessageBoxIcon.Warning);
                return;
            }

            string[] parts = jwt.Split('.');
            if (parts.Length != 3)
            {
                DarkMessageBox.Show("Invalid JWT format. A JWT should have three parts separated by dots.", "Error", MessageBoxIcon.Error);
                return;
            }

            string headerJson = JwtDecoder.DecodeBase64Url(parts[0]);
            string formattedHeader = JwtDecoder.FormatJson(headerJson);
            jwtDecodedHeaderBox.Text = formattedHeader;
            JsonSyntaxHighlighter.ApplyTo(jwtDecodedHeaderBox);

            string payloadJson = JwtDecoder.DecodeBase64Url(parts[1]);
            string formattedPayload = JwtDecoder.FormatJson(payloadJson);
            jwtDecodedPayloadBox.Text = formattedPayload;
            JsonSyntaxHighlighter.ApplyTo(jwtDecodedPayloadBox);
        }
        catch (Exception ex)
        {
            DarkMessageBox.Show($"Error decoding JWT: {ex.Message}", "Error", MessageBoxIcon.Error);
        }
    }

}
