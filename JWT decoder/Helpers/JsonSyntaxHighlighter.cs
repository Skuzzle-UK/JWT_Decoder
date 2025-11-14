using System.Text.RegularExpressions;

namespace JWT_decoder.Helpers;

public static class JsonSyntaxHighlighter
{
    /// <summary>
    /// Applies syntax highlighting to JSON text in a RichTextBox
    /// </summary>
    /// <param name="textBox">The RichTextBox containing JSON text</param>
    public static void ApplyTo(RichTextBox textBox)
    {
        int selectionStart = textBox.SelectionStart;
        int selectionLength = textBox.SelectionLength;

        string text = textBox.Text;
        textBox.SelectAll();
        textBox.SelectionColor = Color.FromArgb(220, 220, 220);
        textBox.SelectionFont = new Font(textBox.Font, FontStyle.Regular);

        HashSet<int> keyPositions = GetKeyPositions(text);

        HighlightStringValues(textBox, text, keyPositions);
        HighlightNumberValues(textBox, text);
        HighlightBooleanAndNullValues(textBox, text);
        HighlightJsonKeys(textBox, text);

        textBox.Select(selectionStart, selectionLength);
    }

    private static HashSet<int> GetKeyPositions(string text)
    {
        HashSet<int> keyPositions = new HashSet<int>();
        Regex keyRegex = new(@"""([^""\\]*(\\.[^""\\]*)*)""(\s*):");
        foreach (Match match in keyRegex.Matches(text))
        {
            keyPositions.Add(match.Index);
        }
        return keyPositions;
    }

    private static void HighlightJsonKeys(RichTextBox textBox, string text)
    {
        Regex keyRegex = new(@"""([^""\\]*(\\.[^""\\]*)*)""(\s*):");
        foreach (Match match in keyRegex.Matches(text))
        {
            int keyStart = match.Index;
            int keyLength = match.Length - match.Groups[3].Length - 1;
            textBox.Select(keyStart, keyLength);
            textBox.SelectionColor = Color.FromArgb(86, 196, 255);
            textBox.SelectionFont = new Font(textBox.Font, FontStyle.Bold);
        }
    }

    private static void HighlightStringValues(RichTextBox textBox, string text, HashSet<int> keyPositions)
    {
        Regex stringRegex = new(@"""([^""\\]*(\\.[^""\\]*)*)""");
        foreach (Match match in stringRegex.Matches(text))
        {
            if (keyPositions.Contains(match.Index))
            {
                continue;
            }

            textBox.Select(match.Index, match.Length);
            textBox.SelectionColor = Color.FromArgb(206, 145, 120);
            textBox.SelectionFont = new Font(textBox.Font, FontStyle.Regular);
        }
    }

    private static void HighlightNumberValues(RichTextBox textBox, string text)
    {
        Regex numberRegex = new(@"(?:[:,\[])\s*(-?\d+\.?\d*([eE][+-]?\d+)?)");
        foreach (Match match in numberRegex.Matches(text))
        {
            int valueStart = match.Groups[1].Index;
            int valueLength = match.Groups[1].Length;
            textBox.Select(valueStart, valueLength);
            textBox.SelectionColor = Color.FromArgb(181, 206, 168);
            textBox.SelectionFont = new Font(textBox.Font, FontStyle.Regular);
        }
    }

    private static void HighlightBooleanAndNullValues(RichTextBox textBox, string text)
    {
        Regex boolNullRegex = new(@"(?:[:,\[])\s*(true|false|null)\b");
        foreach (Match match in boolNullRegex.Matches(text))
        {
            int valueStart = match.Groups[1].Index;
            int valueLength = match.Groups[1].Length;
            textBox.Select(valueStart, valueLength);
            textBox.SelectionColor = Color.FromArgb(86, 156, 214);
            textBox.SelectionFont = new Font(textBox.Font, FontStyle.Regular);
        }
    }
}
