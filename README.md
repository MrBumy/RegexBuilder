# RegexBuilder

[![Language Switcher](https://img.shields.type/badge/Language-English%20%7C%20%D0%A0%D1%83%D1%81%D1%81%D0%BA%D0%B8%D0%B9-gold)](#table-of-contents)

RegexBuilder — visual regular expression constructor, tester, and analyzer for .NET 8 WPF, styled with a premium dark mode (Gold/Blue accents).
RegexBuilder — визуальный конструктор, тестер и анализатор регулярных выражений на .NET 8 WPF в стильном тёмном оформлении с золотыми и синими акцентами.

---

## Table of Contents / Содержание

*   [English Version](#english-version)
    *   [Features](#features)
    *   [Architecture](#architecture)
    *   [Build & Setup](#build--setup)
    *   [Keyboard Shortcuts](#keyboard-shortcuts)
*   [Русская версия](#russian-version)
    *   [Особенности](#особенности)
    *   [Архитектура](#архитектура)
    *   [Сборка и Запуск](#сборка-и-запуск)
    *   [Горячие клавиши](#горячие-клавиши)

---

<a name="english-version"></a>
## English Version

RegexBuilder is a visual developer utility built to construct, test, and analyze regular expressions in real-time.

### Features
*   **Dynamic Highlighting**: Matches are colored instantly in the target text area as you type or modify the regex pattern.
*   **Visual Token Bricks**: Insert standard blocks (`\d`, `\w`, `\s`, groups, anchors) at the caret position with automated smart cursor placement.
*   **Smart Selection Heuristics ("Make Regex" button)**: Highlight a fragment of text (e.g. email or digits) and auto-generate the matching regex pattern instantly.
*   **Search-and-Replace Test Runner**: Swap to the "Testing" tab to test regex replacements (substitutions) and view outcomes in real-time.
*   **Favorites Library**: Backed by a SQLite database to save, reload, and delete custom regex configurations.
*   **Built-in Template Library**: Quick presets for Email, Russian Phone numbers, IP-address, dates, URLs, UUIDs, and HTML tags.
*   **Real-time Syntax Parser**: Displays human-readable breakdowns of used regex tokens in the details section.
*   **Language Selector**: Supports real-time switching between Russian (RU) and English (EN) dynamically.

### Architecture
The project utilizes the classic **MVVM** pattern powered by standard .NET 8 WPF and libraries:
*   **Framework**: .NET 8.0 WPF (targets `net8.0-windows`).
*   **Mvvm toolkit**: `CommunityToolkit.Mvvm` (using observable source generators).
*   **Micro-ORM & Database**: `Dapper` + `Microsoft.Data.Sqlite` for local favorites storage.
*   **Database Location**: `%LOCALAPPDATA%\RegexBuilder\favorites.db`.

```
RegexBuilder/
├── Models/
│   ├── FavoritePattern.cs        # Favorite database record model
│   └── RegexTemplate.cs         # Built-in library template preset
├── Services/
│   ├── DatabaseService.cs        # SQLite database connection and CRUD
│   └── RegexGeneratorService.cs  # Regex generator selection heuristics
├── ViewModels/
│   └── MainViewModel.cs          # State container, commands, and options
├── Styles/
│   └── Theme.xaml                # Premium dark mode theme resources
├── Resources/
│   ├── Strings.ru.xaml           # Russian localization dictionary
│   └── Strings.en.xaml           # English localization dictionary
├── App.xaml / App.xaml.cs        # Entry point and dynamic localization toggles
└── MainWindow.xaml / .xaml.cs    # Core window layout and match highlighting
```

### Build & Setup

#### Prerequisites
*   .NET 8.0 SDK (or later)

#### Building and Running
1. Clone the project.
2. Navigate to the project directory:
   ```bash
   cd RegexBuilder
   ```
3. Restore packages and compile:
   ```bash
   dotnet build
   ```
4. Run the executable:
   ```bash
   dotnet run
   ```

### Keyboard Shortcuts
*   `F1` — Toggle the visual syntax cheatsheet helper.
*   `Ctrl+F` — Focus and highlight the regex input textbox.
*   `Ctrl+Enter` — Compile expression and apply highlights immediately.
*   `Ctrl+S` — Trigger the "Save to Favorites" overlay dialog.
*   `Ctrl+C` — Copy current regex string to clipboard (unless copying highlighted text in focused inputs).
*   `Esc` — Close open modals (cheatsheet, save overlay).

---

<a name="russian-version"></a>
## Русская версия

RegexBuilder — это визуальный инструмент разработчика, предназначенный для создания, тестирования и анализа регулярных выражений в режиме реального времени.

### Особенности
*   **Подсветка совпадений**: Совпадения с паттерном выделяются золотым цветом в центральном поле ввода прямо во время набора или редактирования регулярного выражения.
*   **Панель кирпичиков (Конструктор)**: Вставка токенов (`\d`, `\w`, `\s`, групп, границ) по клику в позицию курсора с авто-фокусировкой и позиционированием каретки (например, каретка автоматически ставится внутрь круглых скобок).
*   **Волшебная кнопка ("Сделать Regex")**: Автоматическая генерация регулярного выражения на основе выделенного пользователем фрагмента текста по умным эвристическим правилам.
*   **Полноценное тестирование замены**: Вкладка "Тестирование" позволяет вводить паттерны замены и просматривать результат автозамены в реальном времени.
*   **База данных Избранного**: Локальная база данных SQLite для сохранения, загрузки и удаления часто используемых выражений.
*   **Библиотека шаблонов**: Готовые шаблоны для поиска Email, номеров телефонов РФ, URL, IP-адресов, дат YYYY-MM-DD, цветов HEX, UUID и тегов HTML.
*   **Динамический анализ синтаксиса**: Анализатор структуры выражений, выводящий текстовую расшифровку для каждого встреченного токена.
*   **Локализация (RU/EN)**: Мгновенное переключение языка интерфейса кнопками в шапке окна.

### Архитектура
Приложение спроектировано по архитектуре **MVVM** с использованием современных библиотек:
*   **Платформа**: .NET 8.0 WPF (`net8.0-windows`).
*   **Разработка MVVM**: `CommunityToolkit.Mvvm` (с генераторами исходного кода).
*   **Доступ к данным**: `Dapper` + `Microsoft.Data.Sqlite`.
*   **Размещение БД**: `%LOCALAPPDATA%\RegexBuilder\favorites.db` (база данных создается автоматически при первом запуске).

### Сборка и Запуск

#### Системные требования
*   Установленный .NET 8.0 SDK (или новее)

#### Сборка из терминала
1. Перейдите в корневую папку проекта:
   ```bash
   cd RegexBuilder
   ```
2. Восстановите зависимости и соберите проект:
   ```bash
   dotnet build
   ```
3. Запустите приложение:
   ```bash
   dotnet run
   ```

### Горячие клавиши
*   `F1` — Показать / скрыть справку по синтаксису регулярных выражений.
*   `Ctrl+F` — Перевести фокус на поле ввода регулярного выражения.
*   `Ctrl+Enter` — Принудительно применить регулярное выражение к тексту.
*   `Ctrl+S` — Сохранить текущее выражение в базу данных избранного.
*   `Ctrl+C` — Скопировать регулярное выражение в буфер обмена (если фокус не находится на выделенном тексте внутри других полей).
*   `Esc` — Закрыть открытые всплывающие панели (справку, окно сохранения).
