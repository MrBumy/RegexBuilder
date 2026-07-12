using System.Windows;

namespace RegexBuilder
{
    public partial class App : Application
    {
        public static void ChangeLanguage(string languageCode)
        {
            var app = (App)Current;
            ResourceDictionary? oldDict = null;

            foreach (var dict in app.Resources.MergedDictionaries)
            {
                if (dict.Source != null && 
                    (dict.Source.OriginalString.Contains("Strings.ru.xaml") || 
                     dict.Source.OriginalString.Contains("Strings.en.xaml")))
                {
                    oldDict = dict;
                    break;
                }
            }

            var newDict = new ResourceDictionary
            {
                Source = new Uri($"Resources/Strings.{languageCode}.xaml", UriKind.Relative)
            };

            if (oldDict != null)
            {
                app.Resources.MergedDictionaries.Remove(oldDict);
            }
            app.Resources.MergedDictionaries.Add(newDict);
        }
    }
}
