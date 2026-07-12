using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using RegexBuilder.Models;
using RegexBuilder.Services;
using RegexBuilder.ViewModels;

namespace RegexBuilder
{
    public partial class MainWindow : Window
    {
        private readonly RegexGeneratorService _generatorService = new();
        private MainViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            InitializeComponent();
            
            // Subscribe to VM events and property changes
            _viewModel.RequestMatchHighlighting += (s, e) => HighlightMatches();
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            Loaded += Window_Loaded;
            PreviewKeyDown += Window_PreviewKeyDown;

            // Bind F1 key (Help command) to toggle the syntax cheatsheet
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, (s, ev) =>
            {
                _viewModel.IsSyntaxHelpOpen = !_viewModel.IsSyntaxHelpOpen;
                ev.Handled = true;
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load the first template (Email) by default to give a great first impression
            if (_viewModel.Templates.Count > 0)
            {
                _viewModel.SelectedTemplate = _viewModel.Templates[0];
            }
            
            // Ensure first rendering of line numbers
            UpdateLineNumbers();
            HighlightMatches();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SampleText))
            {
                string vmText = _viewModel.SampleText;
                string richText = GetRichTextBoxText();
                if (vmText != richText)
                {
                    TargetRichTextBox.TextChanged -= OnRichTextBoxTextChanged;
                    TargetRichTextBox.Document.Blocks.Clear();
                    TargetRichTextBox.Document.Blocks.Add(new Paragraph(new Run(vmText)));
                    TargetRichTextBox.TextChanged += OnRichTextBoxTextChanged;
                    
                    UpdateLineNumbers();
                    HighlightMatches();
                }
            }
        }

        private string GetRichTextBoxText()
        {
            var range = new TextRange(TargetRichTextBox.Document.ContentStart, TargetRichTextBox.Document.ContentEnd);
            string text = range.Text;
            
            // Strip trailing newlines standard in WPF RichTextBox
            if (text.EndsWith("\r\n"))
                return text.Substring(0, text.Length - 2);
            if (text.EndsWith("\n"))
                return text.Substring(0, text.Length - 1);
            
            return text;
        }

        private void OnRichTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            // Sync rich text contents back to ViewModel
            _viewModel.SampleText = GetRichTextBoxText();
            UpdateLineNumbers();
            HighlightMatches();
        }

        private void OnRichTextBoxScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Sync vertical offset to line numbers panel
            LineNumbersScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void UpdateLineNumbers()
        {
            string text = GetRichTextBoxText();
            int lineCount = text.Split('\n').Length;
            
            if (lineCount < 1) lineCount = 1;

            var sb = new StringBuilder();
            for (int i = 1; i <= lineCount; i++)
            {
                sb.AppendLine(i.ToString());
            }

            LineNumbersTextBlock.Text = sb.ToString();
        }

        private void HighlightMatches()
        {
            if (TargetRichTextBox == null || _viewModel == null) return;

            // Unsubscribe from TextChanged to avoid infinite loops during formatting
            TargetRichTextBox.TextChanged -= OnRichTextBoxTextChanged;

            try
            {
                // Clear formatting
                TextRange documentRange = new TextRange(TargetRichTextBox.Document.ContentStart, TargetRichTextBox.Document.ContentEnd);
                documentRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                documentRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xE5)));

                string pattern = _viewModel.RegexPattern;
                if (string.IsNullOrEmpty(pattern))
                {
                    _viewModel.MatchCount = 0;
                    _viewModel.GroupCount = 0;
                    _viewModel.ExecutionTime = 0;
                    return;
                }

                string text = documentRange.Text;

                // Run regex compilation and find matches
                var stopwatch = Stopwatch.StartNew();
                var options = _viewModel.GetRegexOptions();
                var regex = new Regex(pattern, options);
                var matches = regex.Matches(text);
                stopwatch.Stop();

                _viewModel.MatchCount = matches.Count;
                _viewModel.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;

                // Compute maximum captures count
                int maxGroups = 0;
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > maxGroups)
                        maxGroups = match.Groups.Count;
                }
                _viewModel.GroupCount = maxGroups > 0 ? maxGroups - 1 : 0;

                if (matches.Count == 0) return;

                // Highlight matches in single-pass traversal
                var goldBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
                var darkBrush = new SolidColorBrush(Color.FromRgb(0x12, 0x12, 0x12));

                int matchIndex = 0;
                TextPointer navigator = TargetRichTextBox.Document.ContentStart;
                int currentCharIndex = 0;
                TextPointer? currentMatchStart = null;

                while (navigator != null && navigator.CompareTo(TargetRichTextBox.Document.ContentEnd) < 0 && matchIndex < matches.Count)
                {
                    TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Forward);

                    if (context == TextPointerContext.Text)
                    {
                        string runText = navigator.GetTextInRun(LogicalDirection.Forward);
                        int runLength = runText.Length;

                        while (matchIndex < matches.Count)
                        {
                            Match currentMatch = matches[matchIndex];
                            if (currentMatch.Length == 0)
                            {
                                // Skip zero-length matches to prevent infinite loops
                                matchIndex++;
                                continue;
                            }

                            int startOffsetInRun = currentMatch.Index - currentCharIndex;
                            int endOffsetInRun = (currentMatch.Index + currentMatch.Length) - currentCharIndex;

                            // Match start is within this run
                            if (currentMatchStart == null && startOffsetInRun >= 0 && startOffsetInRun < runLength)
                            {
                                currentMatchStart = navigator.GetPositionAtOffset(startOffsetInRun);
                            }

                            // Match end is within this run
                            if (currentMatchStart != null && endOffsetInRun >= 0 && endOffsetInRun <= runLength)
                            {
                                TextPointer currentMatchEnd = navigator.GetPositionAtOffset(endOffsetInRun);
                                var matchRange = new TextRange(currentMatchStart, currentMatchEnd);
                                matchRange.ApplyPropertyValue(TextElement.BackgroundProperty, goldBrush);
                                matchRange.ApplyPropertyValue(TextElement.ForegroundProperty, darkBrush);

                                currentMatchStart = null;
                                matchIndex++;
                            }
                            else
                            {
                                // Match extends to next text run
                                break;
                            }
                        }

                        currentCharIndex += runLength;
                    }
                    else if (context == TextPointerContext.ElementStart && navigator.GetAdjacentElement(LogicalDirection.Forward) is LineBreak)
                    {
                        currentCharIndex += 2; // LineBreak is \r\n (2 characters in range text)
                    }
                    else if (context == TextPointerContext.ElementEnd && navigator.Parent is Paragraph)
                    {
                        currentCharIndex += 2; // Paragraph bounds is \r\n (2 characters in range text)
                    }

                    TextPointer next = navigator.GetNextContextPosition(LogicalDirection.Forward);
                    if (next == null || next.CompareTo(navigator) == 0)
                        break;
                    navigator = next;
                }
            }
            catch (ArgumentException)
            {
                // Regex compilation error (e.g. while editing)
                _viewModel.MatchCount = 0;
                _viewModel.GroupCount = 0;
            }
            finally
            {
                TargetRichTextBox.TextChanged += OnRichTextBoxTextChanged;
            }
        }

        private void OnBrickClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string token)
            {
                InsertTokenInRegexTextBox(token);
            }
        }

        private void InsertTokenInRegexTextBox(string token)
        {
            int caretIndex = RegexInputTextBox.SelectionStart;
            string text = RegexInputTextBox.Text;

            string newText = text.Insert(caretIndex, token);
            RegexInputTextBox.Text = newText;

            // Intelligently place caret in logical focus points
            int newCaretIndex = caretIndex + token.Length;
            if (token == "()")
            {
                newCaretIndex = caretIndex + 1; // inside ()
            }
            else if (token == "(?:)")
            {
                newCaretIndex = caretIndex + 3; // inside (?:)
            }
            else if (token == "(?<name>)")
            {
                newCaretIndex = caretIndex + 8; // inside (?<name>) after name
            }

            RegexInputTextBox.Focus();
            RegexInputTextBox.SelectionStart = newCaretIndex;
            RegexInputTextBox.SelectionLength = 0;
            
            // Explicitly sync to ViewModel since WPF Text binding updates on focus lost or PropertyChanged
            _viewModel.RegexPattern = RegexInputTextBox.Text;
        }

        private void OnCreateRegexFromSelectionClick(object sender, RoutedEventArgs e)
        {
            string selection = TargetRichTextBox.Selection.Text;
            if (string.IsNullOrEmpty(selection))
            {
                MessageBox.Show(
                    "Пожалуйста, выделите текст в центральном поле для автоматической генерации регулярного выражения.",
                    "Генерация Regex по выделению",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            string generated = _generatorService.GenerateRegex(selection);
            _viewModel.RegexPattern = generated;
            
            RegexInputTextBox.Focus();
            RegexInputTextBox.SelectAll();
        }

        private void OnTemplateDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.SelectedTemplate != null)
            {
                _viewModel.RegexPattern = _viewModel.SelectedTemplate.Pattern;
                
                TargetRichTextBox.TextChanged -= OnRichTextBoxTextChanged;
                TargetRichTextBox.Document.Blocks.Clear();
                TargetRichTextBox.Document.Blocks.Add(new Paragraph(new Run(_viewModel.SelectedTemplate.ExampleText)));
                TargetRichTextBox.TextChanged += OnRichTextBoxTextChanged;
                
                _viewModel.SampleText = _viewModel.SelectedTemplate.ExampleText;
                UpdateLineNumbers();
                HighlightMatches();
            }
        }

        private void OnFavoriteDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.SelectedFavorite != null)
            {
                _viewModel.RegexPattern = _viewModel.SelectedFavorite.Pattern;
                HighlightMatches();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Esc closes open overlays
            if (e.Key == Key.Escape)
            {
                if (_viewModel.IsSaveDialogOpen)
                {
                    _viewModel.IsSaveDialogOpen = false;
                    e.Handled = true;
                }
                else if (_viewModel.IsSyntaxHelpOpen)
                {
                    _viewModel.IsSyntaxHelpOpen = false;
                    e.Handled = true;
                }
            }
            // F1 - Syntax Help
            else if (e.Key == Key.F1)
            {
                _viewModel.IsSyntaxHelpOpen = !_viewModel.IsSyntaxHelpOpen;
                e.Handled = true;
            }
            // Ctrl combinations
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.F)
                {
                    RegexInputTextBox.Focus();
                    RegexInputTextBox.SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    _viewModel.ApplyRegex();
                    e.Handled = true;
                }
                else if (e.Key == Key.S)
                {
                    if (!_viewModel.IsSaveDialogOpen && !string.IsNullOrEmpty(_viewModel.RegexPattern))
                    {
                        _viewModel.OpenSaveDialogCommand.Execute(null);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.C)
                {
                    if (Keyboard.FocusedElement is not TextBox textBox || textBox.SelectionLength == 0)
                    {
                        _viewModel.CopyRegexCommand.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }

        private void OnLanguageRuClick(object sender, RoutedEventArgs e)
        {
            App.ChangeLanguage("ru");
            if (sender is Button button)
            {
                UpdateButtonVisuals(button, "ru");
            }
            _viewModel.RefreshLocalizedProperties();
        }

        private void OnLanguageEnClick(object sender, RoutedEventArgs e)
        {
            App.ChangeLanguage("en");
            if (sender is Button button)
            {
                UpdateButtonVisuals(button, "en");
            }
            _viewModel.RefreshLocalizedProperties();
        }

        private void UpdateButtonVisuals(Button clickedButton, string languageCode)
        {
            if (clickedButton.Parent is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is Button btn)
                    {
                        if (btn == clickedButton)
                        {
                            btn.Style = (Style)FindResource(languageCode == "ru" ? "GoldButton" : "BlueButton");
                        }
                        else
                        {
                            btn.Style = (Style)Application.Current.Resources[typeof(Button)];
                        }
                    }
                }
            }
        }
    }
}
