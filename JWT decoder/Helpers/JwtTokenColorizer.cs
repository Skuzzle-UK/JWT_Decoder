namespace JWT_decoder.Helpers;

public static class JwtTokenColorizer
{
    /// <summary>
    /// Applies color coding to the three parts of a JWT token (header.payload.signature)
    /// </summary>
    /// <param name="textBox">The RichTextBox containing the JWT token</param>
    public static void ApplyTo(RichTextBox textBox)
    {
        int selectionStart = textBox.SelectionStart;
        int selectionLength = textBox.SelectionLength;

        string text = textBox.Text;

        int firstDot = text.IndexOf('.');
        int secondDot = text.IndexOf('.', firstDot + 1);

        textBox.SelectAll();
        textBox.SelectionColor = Color.White;

        if (firstDot > 0)
        {
            textBox.Select(0, firstDot);
            textBox.SelectionColor = Color.FromArgb(86, 196, 255);

            textBox.Select(firstDot, 1);
            textBox.SelectionColor = Color.White;

            if (secondDot > firstDot)
            {
                textBox.Select(firstDot + 1, secondDot - firstDot - 1);
                textBox.SelectionColor = Color.FromArgb(255, 159, 64);

                textBox.Select(secondDot, 1);
                textBox.SelectionColor = Color.White;

                if (secondDot + 1 < text.Length)
                {
                    textBox.Select(secondDot + 1, text.Length - secondDot - 1);
                    textBox.SelectionColor = Color.FromArgb(220, 130, 255);
                }
            }
        }

        textBox.Select(selectionStart, selectionLength);
        textBox.SelectionColor = Color.White;
    }
}
