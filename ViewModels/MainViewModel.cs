using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegexBuilder.Models;
using RegexBuilder.Services;

namespace RegexBuilder.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private string _regexPattern = string.Empty;

        [ObservableProperty]
        private string _sampleText = string.Empty;

        [ObservableProperty]
        private bool _ignoreCase = true;

        [ObservableProperty]
        private bool _multiline = true;

        [ObservableProperty]
        private bool _singleline = false;

        [ObservableProperty]
        private bool _ignorePatternWhitespace = false;

        [ObservableProperty]
        private int _matchCount;

        [ObservableProperty]
        private double _executionTime;

        [ObservableProperty]
        private int _groupCount;

        [ObservableProperty]
        private string _replacePattern = string.Empty;

        [ObservableProperty]
        private string _replacementResult = string.Empty;

        [ObservableProperty]
        private string _selectedPatternDetails = "Введите регулярное выражение для анализа.";

        [ObservableProperty]
        private bool _isSyntaxHelpOpen;

        [ObservableProperty]
        private bool _isSaveDialogOpen;

        [ObservableProperty]
        private string _newFavoriteName = string.Empty;

        [ObservableProperty]
        private string _newFavoriteDescription = string.Empty;

        public ObservableCollection<FavoritePattern> Favorites { get; } = new();
        public ObservableCollection<RegexTemplate> Templates { get; } = new();

        [ObservableProperty]
        private FavoritePattern? _selectedFavorite;

        [ObservableProperty]
        private RegexTemplate? _selectedTemplate;

        // Event to notify view that matching needs to be re-run on RichTextBox
        public event EventHandler? RequestMatchHighlighting;

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            LoadTemplates();
            // Start loading favorites on a background thread
            Task.Run(async () => await LoadFavoritesAsync());
        }

        private void LoadTemplates()
        {
            Templates.Add(new RegexTemplate
            {
                Name = "Email",
                Pattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
                Description = "Шаблон для поиска адресов электронной почты.",
                ExampleText = "Примеры адресов:\nhello@world.com\nsupport@google.ru\ntest.user+alias@company.co.uk\ninvalid-email@com (не совпадет)"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "Телефон (Россия)",
                Pattern = @"(?:\+7|8)?\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}",
                Description = "Номера телефонов РФ в различных форматах.",
                ExampleText = "Примеры номеров:\n+7 (999) 123-45-67\n89991234567\n+79991234567\n8-999-123-45-67\n+7 (999)123-4567"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "URL",
                Pattern = @"https?://(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
                Description = "Веб-адреса с протоколами http и https.",
                ExampleText = "Адреса веб-сайтов:\nhttps://github.com/dotnet/wpf\nhttp://example.com/path?query=val&another=1\nhttps://sub.domain.co/test"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "IP-адрес",
                Pattern = @"(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)",
                Description = "IPv4 адрес (от 0.0.0.0 до 255.255.255.255).",
                ExampleText = "Список адресов:\n192.168.1.1\n255.255.255.255\n127.0.0.1\n256.100.0.1 (не совпадет)\n999.999.999.999 (не совпадет)"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "Дата (YYYY-MM-DD)",
                Pattern = @"\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12][0-9]|3[01])",
                Description = "Дата в ISO формате (ГГГГ-ММ-ДД).",
                ExampleText = "Календарь событий:\n2026-07-12\n1999-12-31\n2024-02-29\n2025-13-45 (не совпадет)"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "HEX-цвет",
                Pattern = @"#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})",
                Description = "Цветовые коды HEX (3 или 6 символов).",
                ExampleText = "Цветовая палитра:\n#121212\n#FFD700\n#007ACC\nFFF\n#G12345 (не совпадет)"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "UUID",
                Pattern = @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}",
                Description = "Уникальный идентификатор UUID/GUID.",
                ExampleText = "Примеры UUID:\n95244814-0681-42b3-ab38-9c8bbb375d3d\nf2818bc3-5891-4403-9dbf-615b17e755ee\n00000000-0000-0000-0000-000000000000"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "HTML-тег",
                Pattern = @"<\/?([a-zA-Z0-9]+)(?:\s+[^>]*?)?>",
                Description = "HTML теги различного формата.",
                ExampleText = "Разметка страницы:\n<div class=\"container\">\n<h1>Заголовок</h1>\n<p>Текст абзаца</p>\n<img src=\"logo.png\" />\n</div>"
            });
            Templates.Add(new RegexTemplate
            {
                Name = "Ссылка на GitHub",
                Pattern = @"https?://(?:www\.)?github\.com/[a-zA-Z0-9-]+/[a-zA-Z0-9_.-]+/?",
                Description = "Ссылки на репозитории GitHub.",
                ExampleText = "Наши репозитории:\nhttps://github.com/dotnet/wpf\nhttps://github.com/microsoft/terminal/\nhttp://github.com/user/repo-name"
            });
        }

        public async Task LoadFavoritesAsync()
        {
            try
            {
                await _databaseService.InitializeDatabaseAsync();
                var favorites = await _databaseService.GetFavoritesAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Favorites.Clear();
                    foreach (var fav in favorites)
                    {
                        Favorites.Add(fav);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load favorites: {ex.Message}");
            }
        }

        partial void OnRegexPatternChanged(string value)
        {
            ApplyRegex();
            UpdatePatternDetails();
        }

        partial void OnSampleTextChanged(string value)
        {
            ApplyRegex();
        }

        partial void OnReplacePatternChanged(string value)
        {
            UpdateReplacement();
        }

        partial void OnIgnoreCaseChanged(bool value) => ApplyRegex();
        partial void OnMultilineChanged(bool value) => ApplyRegex();
        partial void OnSinglelineChanged(bool value) => ApplyRegex();
        partial void OnIgnorePatternWhitespaceChanged(bool value) => ApplyRegex();

        public void ApplyRegex()
        {
            RequestMatchHighlighting?.Invoke(this, EventArgs.Empty);
            UpdateReplacement();
        }

        private void UpdateReplacement()
        {
            if (string.IsNullOrEmpty(RegexPattern))
            {
                ReplacementResult = string.Empty;
                return;
            }

            try
            {
                var options = GetRegexOptions();
                var regex = new Regex(RegexPattern, options);
                ReplacementResult = regex.Replace(SampleText, ReplacePattern);
            }
            catch (Exception ex)
            {
                ReplacementResult = $"Ошибка замены: {ex.Message}";
            }
        }

        public RegexOptions GetRegexOptions()
        {
            var options = RegexOptions.None;
            if (IgnoreCase) options |= RegexOptions.IgnoreCase;
            if (Multiline) options |= RegexOptions.Multiline;
            if (Singleline) options |= RegexOptions.Singleline;
            if (IgnorePatternWhitespace) options |= RegexOptions.IgnorePatternWhitespace;
            return options;
        }

        private void UpdatePatternDetails()
        {
            if (string.IsNullOrWhiteSpace(RegexPattern))
            {
                SelectedPatternDetails = Application.Current.TryFindResource("PatternInfoDefault") as string ?? "Введите регулярное выражение для анализа.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            
            string header = Application.Current.TryFindResource("PatternInfoHeader") as string ?? "Анализ паттерна:";
            sb.AppendLine(header);
            
            if (RegexPattern.Contains(@"\d")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoDigits") as string ?? "• \\d — числовые цифры (0-9)");
            
            if (RegexPattern.Contains(@"\D")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoNonDigits") as string ?? "• \\D — любые символы, кроме цифр");
            
            if (RegexPattern.Contains(@"\w")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoWord") as string ?? "• \\w — буквенно-цифровые символы и символ подчеркивания");
            
            if (RegexPattern.Contains(@"\W")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoNonWord") as string ?? "• \\W — любые символы, кроме буквенно-цифровых");
            
            if (RegexPattern.Contains(@"\s")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoSpaces") as string ?? "• \\s — пробельные символы (пробел, табуляция, новая строка)");
            
            if (RegexPattern.Contains(@"\S")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoNonSpaces") as string ?? "• \\S — любые символы, кроме пробельных");
            
            if (RegexPattern.Contains("^")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoStart") as string ?? "• ^ — начало строки/текста");
            
            if (RegexPattern.Contains("$")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoEnd") as string ?? "• $ — конец строки/текста");
            
            if (RegexPattern.Contains("*")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoZeroOrMore") as string ?? "• * — квантификатор: 0 или более повторений");
            
            if (RegexPattern.Contains("+")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoOneOrMore") as string ?? "• + — квантификатор: 1 или более повторений");
            
            if (RegexPattern.Contains("?")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoOptional") as string ?? "• ? — квантификатор: 0 или 1 повторение, либо ленивый режим");
            
            if (RegexPattern.Contains("(")) 
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoGroup") as string ?? "• () — группа захвата или группировка");

            if (sb.Length <= header.Length + 2)
            {
                sb.AppendLine(Application.Current.TryFindResource("PatternInfoCustom") as string ?? "• Пользовательское выражение. Специфических токенов не обнаружено.");
            }

            SelectedPatternDetails = sb.ToString();
        }

        public void RefreshLocalizedProperties()
        {
            UpdatePatternDetails();
            ApplyRegex();
        }

        [RelayCommand]
        private void TriggerApply()
        {
            ApplyRegex();
        }

        [RelayCommand]
        private void CopyRegex()
        {
            if (!string.IsNullOrEmpty(RegexPattern))
            {
                Clipboard.SetText(RegexPattern);
            }
        }

        [RelayCommand]
        private void ToggleSyntaxHelp()
        {
            IsSyntaxHelpOpen = !IsSyntaxHelpOpen;
        }

        [RelayCommand]
        private void OpenSaveDialog()
        {
            if (string.IsNullOrEmpty(RegexPattern)) return;
            
            NewFavoriteName = SelectedTemplate?.Name ?? "Новый шаблон";
            NewFavoriteDescription = SelectedTemplate?.Description ?? string.Empty;
            IsSaveDialogOpen = true;
        }

        [RelayCommand]
        private void CloseSaveDialog()
        {
            IsSaveDialogOpen = false;
        }

        [RelayCommand]
        private async Task SaveFavorite()
        {
            if (string.IsNullOrWhiteSpace(NewFavoriteName) || string.IsNullOrWhiteSpace(RegexPattern))
            {
                return;
            }

            var fav = new FavoritePattern
            {
                Name = NewFavoriteName,
                Pattern = RegexPattern,
                Description = NewFavoriteDescription,
                CreatedAt = DateTime.Now
            };

            await _databaseService.AddFavoriteAsync(fav);
            IsSaveDialogOpen = false;
            await LoadFavoritesAsync();
        }

        [RelayCommand]
        private async Task DeleteFavorite(FavoritePattern? favorite)
        {
            if (favorite == null) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить шаблон \"{favorite.Name}\" из избранного?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _databaseService.DeleteFavoriteAsync(favorite.Id);
                await LoadFavoritesAsync();
            }
        }

        partial void OnSelectedTemplateChanged(RegexTemplate? value)
        {
            if (value == null) return;
            RegexPattern = value.Pattern;
            SampleText = value.ExampleText;
        }

        partial void OnSelectedFavoriteChanged(FavoritePattern? value)
        {
            if (value == null) return;
            RegexPattern = value.Pattern;
            SelectedPatternDetails = value.Description;
        }
    }
}
