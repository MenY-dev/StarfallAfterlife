using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class MarkdownView : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<MarkdownView, string>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

        protected virtual List<MdParser> Parsers { get; } = new()
        {
            new UnderlineTagParser() { Priority = 0 },
            new StrikethroughTagParser() { Priority = 0 },
            new ItalicTagParser() { Priority = 0 },
            new BoldTagParser() { Priority = 1 },
            new BoldItalicTagParser() { Priority = 2 },
            new MonospaceTagParser() { Priority = 0 },
            new HeaderTagParser() { Priority = 0 },
            new ListTagParser() { Priority = 0 },
            new NumberListTagParser() { Priority = 0 },
        };

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public InlineCollection Inlines { get; } = new();

        public MarkdownView()
        {
            InitializeComponent();
        }

        public void UpdateLines()
        {
            var text = Text;
            
            Inlines.Clear();

            if (text is null)
                return;

            var span = new Span();
            span.Inlines.Add(new Run(text));
            GenerateInlines(span);
            Inlines.Add(span);
        }

        protected void GenerateInlines(Span root)
        {
            var queue = new Queue<Inline>();

            queue.Enqueue(root);

            do
            {
                var inline = queue.Dequeue();

                if (inline is null)
                    continue;

                if (inline is Run run)
                {
                    var parent = inline.Parent as Span;

                    if (parent is null)
                        continue;

                    var index = parent.Inlines.IndexOf(run);
                    var parseResult = ProcessTextRun(run)
                        .Where(i => i is not null && i.IsSucces == true)
                        .ToList()
                        .OrderBy(i => i.Index)
                        .ThenBy(i => -i.Priority)
                        .FirstOrDefault();

                    if (index > -1 &&
                        parseResult is not null &&
                        parseResult.CreateInlines() is List<Inline> newInlines &&
                        newInlines.Count > 0)
                    {
                        parent.Inlines.Remove(run);
                        newInlines.Reverse();

                        foreach (var item in newInlines)
                        {
                            parent.Inlines.Insert(index, item);
                            queue.Enqueue(item);
                        }
                    }
                }
                else if (inline is Span span)
                {
                    foreach (var item in span.Inlines ?? new())
                        queue.Enqueue(item);
                }
            }
            while (queue.Count > 0);
        }

        protected IEnumerable<ParseResult> ProcessTextRun(Run run)
        {
            var text = run.Text;

            if (text is null || Parsers is null)
                return Enumerable.Empty<ParseResult>();

            return Parsers.Select(p => p.Parse(run.Text));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                UpdateLines();
            }
        }

        protected class ParseResult
        {
            public bool IsSucces;
            public int Index;
            public string BeforeMatch;
            public string OpenTag;
            public string Match;
            public string CloseTag;
            public string AfterMatch;
            public int Priority;
            private Func<ParseResult, List<Inline>> _containerCreator;

            public ParseResult(Func<ParseResult, List<Inline>> containerCreator)
            {
                _containerCreator = containerCreator;
            }

            public List<Inline> CreateInlines()
            {
                return _containerCreator?.Invoke(this);
            }
        }

        protected class MdParser
        {
            protected virtual string Pattern { get; }

            protected virtual RegexOptions Options { get; } = RegexOptions.Multiline;

            public virtual List<Inline> CreateInlines(ParseResult parseResult) => null;

            public int Priority { get; set; }

            public ParseResult Parse(string text)
            {
                Match match = null;

                if (text is null || Pattern is null)
                    return null;

                try
                {
                    match = Regex.Match(text, Pattern, Options);
                }
                catch
                {
                    return null;
                }
                
                if (match is null ||
                    match.Success == false)
                    return null;

                var textMatch = match.Groups?.GetValueOrDefault("m");
                var OpenTagMatch = match.Groups?.GetValueOrDefault("t1");
                var CloseTagMatch = match.Groups?.GetValueOrDefault("t2");

                if (textMatch is null ||
                    textMatch.Success == false ||
                    textMatch.Length < 1 ||
                    OpenTagMatch?.Value.Contains('\\') == true)
                    return null;

                return new ParseResult(CreateInlines)
                {
                    IsSucces = true,
                    Index = match.Index,
                    BeforeMatch = text[0..match.Index],
                    OpenTag = OpenTagMatch?.Value,
                    Match = textMatch.Value,
                    CloseTag = CloseTagMatch?.Value,
                    AfterMatch = text[(match.Index + match.Length)..],
                    Priority = Priority,
                };
            }
        }

        protected class SimpleTagParser : MdParser
        {
            public override List<Inline> CreateInlines(ParseResult parseResult)
            {
                if (parseResult is null ||
                    parseResult.Match is null ||
                    parseResult.IsSucces == false)
                    return null;

                var container = CreateMatchContainer(parseResult);

                if (container is null)
                    return null;

                var result = new List<Inline>();

                if (parseResult.BeforeMatch is string beforeMatch &&
                    beforeMatch.Length > 0)
                    result.Add(new Run(beforeMatch));

                if (CreatePrefix(parseResult) is Inline prefix)
                    container.Inlines.Add(prefix);

                container.Inlines.Add(new Run(parseResult.Match));
                result.Add(container);

                if (CreatePostfix(parseResult) is Inline postfix)
                    container.Inlines.Add(postfix);

                if (parseResult.AfterMatch is string afterMatch &&
                    afterMatch.Length > 0)
                    result.Add(new Run(afterMatch));

                return result;
            }

            protected virtual Span CreateMatchContainer(ParseResult parseResult) => null;

            protected virtual Inline CreatePrefix(ParseResult parseResult) => null;

            protected virtual Inline CreatePostfix(ParseResult parseResult) => null;
        }

        protected class UnderlineTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>_{2})(?<m>.+?)(?<t2>_{2})";

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { TextDecorations = TextDecorations.Underline };
        }

        protected class StrikethroughTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>~{2})(?<m>.+?)(?<t2>~{2})";

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { TextDecorations = TextDecorations.Strikethrough };
        }

        protected class BoldTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>\*{2})(?<m>.+?)(?<t2>\*{2})";

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { FontWeight = FontWeight.Bold };
        }

        protected class ItalicTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>\*{1})(?<m>.+?)(?<t2>\*{1})";

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { FontStyle = FontStyle.Italic };
        }

        protected class BoldItalicTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>\*{3})(?<m>.+?)(?<t2>\*{3})";

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { FontWeight = FontWeight.Bold, FontStyle = FontStyle.Italic };
        }

        protected class MonospaceTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>`{1})(?<m>.+?)(?<t2>\`{1})";

            protected static readonly SolidColorBrush Background = new(new HslColor(0.25, 0, 0, 0).ToRgb());

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new()
                {
                    Background = Background,
                    FontFamily = new FontFamily(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                        "Courier New" : "monospace"),
                };
        }


        protected class HeaderTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>^#{1,3} )(?<m>.+$(?>\r\n|\r|\n)?)";

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new()
                {
                    FontWeight = FontWeight.Bold,
                    FontSize = parseResult?.OpenTag?.Count(c => c == '#') switch
                    {
                        1 => 24,
                        2 => 20,
                        3 => 16,
                        _ => 12
                    },
                };
        }

        protected class ListTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>^(?:(?!\n|\r)\s)*(?>\*|\+|-) )(?<m>.*$(?>\r\n|\r|\n)?)";

            protected override Inline CreatePrefix(ParseResult parseResult)
            {
                var span = new Span() { FontWeight = FontWeight.Bold };
                span.Inlines.Add(parseResult?.OpenTag?
                    .Replace('*', '•')
                    .Replace('+', '•')
                    .Replace('-', '•')
                    ?? "• ");
                return span;
            }

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { };
        }

        protected class NumberListTagParser : SimpleTagParser
        {
            protected override string Pattern => @"(?<t1>^(?:(?!\n|\r)\s)*\\?(?>\d+\.)+(?>\d+)? )(?<m>.*$(?>\r\n|\r|\n)?)";

            protected override Inline CreatePrefix(ParseResult parseResult)
            {
                var span = new Span() { FontWeight = FontWeight.Bold };
                var number = parseResult?.OpenTag?.TrimEnd().TrimEnd('.') ?? "• ";
                span.Inlines.Add($"\t{number} ");
                return span;
            }

            protected override Span CreateMatchContainer(ParseResult parseResult) =>
                new() { };
        }
    }
}
