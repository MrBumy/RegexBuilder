using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexBuilder.Services
{
    public class RegexGeneratorService
    {
        // Predefined patterns for common formats, matching start to end
        private static readonly (string Pattern, string Result)[] PredefinedPatterns = new[]
        {
            // Email
            (@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}"),
            
            // Russian Phone (e.g. +7 (999) 123-45-67 or 89991234567)
            (@"^(?:\+7|8)?\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$", @"(?:\+7|8)?\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}"),
            
            // Date YYYY-MM-DD
            (@"^\d{4}-\d{2}-\d{2}$", @"\d{4}-\d{2}-\d{2}"),
            
            // IP Address
            (@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", @"(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)"),
            
            // HEX Color
            (@"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$", @"#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})"),
            
            // UUID
            (@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"),
            
            // URL
            (@"^https?://(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&//=]*)$", @"https?://(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&//=]*)")
        };

        public string GenerateRegex(string selectedText)
        {
            if (string.IsNullOrEmpty(selectedText))
            {
                return string.Empty;
            }

            string trimmedText = selectedText.Trim();

            // 1. Check if the selected text matches any predefined templates
            foreach (var (pattern, result) in PredefinedPatterns)
            {
                try
                {
                    if (Regex.IsMatch(trimmedText, pattern, RegexOptions.IgnoreCase))
                    {
                        return result;
                    }
                }
                catch
                {
                    // Ignore regex errors
                }
            }

            // 2. Tokenizer fallback logic
            var sb = new StringBuilder();
            int i = 0;

            while (i < selectedText.Length)
            {
                char c = selectedText[i];

                if (char.IsDigit(c))
                {
                    while (i < selectedText.Length && char.IsDigit(selectedText[i]))
                    {
                        i++;
                    }
                    sb.Append(@"\d+");
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // English uppercase run
                    bool hasLowercaseFollowup = false;
                    int start = i;
                    while (i < selectedText.Length && ((selectedText[i] >= 'A' && selectedText[i] <= 'Z') || (selectedText[i] >= 'a' && selectedText[i] <= 'z')))
                    {
                        if (selectedText[i] >= 'a' && selectedText[i] <= 'z')
                            hasLowercaseFollowup = true;
                        i++;
                    }
                    if (hasLowercaseFollowup)
                    {
                        sb.Append("[a-zA-Z]+");
                    }
                    else
                    {
                        sb.Append("[A-Z]+");
                    }
                }
                else if (c >= 'a' && c <= 'z')
                {
                    // English lowercase run
                    while (i < selectedText.Length && (selectedText[i] >= 'a' && selectedText[i] <= 'z'))
                    {
                        i++;
                    }
                    sb.Append("[a-z]+");
                }
                else if ((c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё')
                {
                    // Russian letters run
                    while (i < selectedText.Length && ((selectedText[i] >= 'а' && selectedText[i] <= 'я') || 
                                                       (selectedText[i] >= 'А' && selectedText[i] <= 'Я') || 
                                                       selectedText[i] == 'ё' || selectedText[i] == 'Ё'))
                    {
                        i++;
                    }
                    sb.Append("[а-яА-ЯёЁ]+");
                }
                else if (char.IsWhiteSpace(c))
                {
                    while (i < selectedText.Length && char.IsWhiteSpace(selectedText[i]))
                    {
                        i++;
                    }
                    sb.Append(@"\s+");
                }
                else
                {
                    // Punctuation or special regex chars
                    sb.Append(Regex.Escape(c.ToString()));
                    i++;
                }
            }

            return sb.ToString();
        }
    }
}
