using MdXaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CSAuto
{
    public static class MarkdownHelper
    {
        /// <summary>
        /// Creates a Markdown instance with GitHub-style formatting using MahApps colors
        /// </summary>
        public static async Task<FlowDocument> GetDocumentAsync(string text)
        {
            // Well MdXaml cant use only 2 spaces to add indetation so replace "  " with "    "
            text = text.Replace("  ", "    ");

            var codeLinksPattern = @"\[`+([^`\]]+)`+\]\(([^)]+)\)";
            var codeLinks = new HashSet<string>();

            var matches = System.Text.RegularExpressions.Regex.Matches(text, codeLinksPattern);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                codeLinks.Add(match.Groups[1].Value);
            }

            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                codeLinksPattern,
                "[$1]($2)"
            );

            var document = await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var markdown = CreateStyledMarkdown();
                var doc = markdown.Transform(text);
                ApplyCodeStyleToSpecificLinks(doc, codeLinks);
                return doc;
            });

            return document;
        }

        static void ApplyCodeStyleToSpecificLinks(FlowDocument document, HashSet<string> codeLinks)
        {
            FindAndStyleHyperlinks(document.Blocks, codeLinks);
        }

        static void FindAndStyleHyperlinks(IEnumerable<Block> blocks, HashSet<string> codeLinks)
        {
            foreach (var block in blocks)
            {
                if (block is Paragraph paragraph)
                {
                    StyleHyperlinksInInlines(paragraph.Inlines, codeLinks);
                }
                else if (block is Section section)
                {
                    FindAndStyleHyperlinks(section.Blocks, codeLinks);
                }
                else if (block is List list)
                {
                    foreach (var listItem in list.ListItems)
                    {
                        FindAndStyleHyperlinks(listItem.Blocks, codeLinks);
                    }
                }
            }
        }

        static void StyleHyperlinksInInlines(InlineCollection inlines, HashSet<string> codeLinks)
        {
            foreach (var inline in inlines)
            {
                if (inline is Hyperlink hyperlink)
                {
                    var text = new TextRange(hyperlink.ContentStart, hyperlink.ContentEnd).Text;

                    if (codeLinks.Contains(text))
                    {
                        hyperlink.FontFamily = new FontFamily("Consolas");
                        hyperlink.FontSize = 13.6;
                        hyperlink.Background = (Brush)Application.Current.Resources["MahApps.Brushes.Gray8"];
                    }
                }
                else if (inline is Span span)
                {
                    StyleHyperlinksInInlines(span.Inlines, codeLinks);
                }
            }
        }
        public static Markdown CreateStyledMarkdown()
        {
            var markdown = new Markdown
            {
                HyperlinkCommand = CreateHyperlinkCommand(),
                DocumentStyle = CreateDocumentStyle(),
                Heading1Style = CreateHeading1Style(),
                Heading2Style = CreateHeading2Style(),
                Heading3Style = CreateHeading3Style(),
                Heading4Style = CreateHeading4Style(),
                Heading5Style = CreateHeading5Style(),
                Heading6Style = CreateHeading6Style(),
                CodeStyle = CreateCodeStyle(),
                CodeBlockStyle = CreateCodeBlockStyle(),
                BlockquoteStyle = CreateQuoteStyle(),
                LinkStyle = CreateHyperlinkStyle(),
                TableStyle = CreateTableStyle(),
                TableHeaderStyle = CreateTableHeaderStyle(),
            };

            RegisterHyperlinkCommandBinding(markdown);
            return markdown;
        }

        private static ICommand CreateHyperlinkCommand()
        {
            return new RoutedCommand();
        }

        private static void RegisterHyperlinkCommandBinding(Markdown markdown)
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(FlowDocument),
                new CommandBinding(markdown.HyperlinkCommand, (sender, e) =>
                {
                    string url = e.Parameter?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                })
            );
        }

        #region Document Style

        private static Style CreateDocumentStyle()
        {
            var style = new Style(typeof(FlowDocument));
            style.Setters.Add(new Setter(FlowDocument.FontFamilyProperty,
                new FontFamily("-apple-system,BlinkMacSystemFont,Segoe UI,Noto Sans,Helvetica,Arial,sans-serif")));
            style.Setters.Add(new Setter(FlowDocument.FontSizeProperty, 16.0));
            style.Setters.Add(new Setter(FlowDocument.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(FlowDocument.BackgroundProperty,
                GetResource("MahApps.Brushes.Control.Background")));
            style.Setters.Add(new Setter(FlowDocument.PagePaddingProperty, new Thickness(16)));
            style.Setters.Add(new Setter(FlowDocument.LineHeightProperty, 24.0));
            return style;
        }

        #endregion

        #region Heading Styles

        private static Style CreateHeading1Style()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 32.0));
            style.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 0, 0, 16)));
            style.Setters.Add(new Setter(Paragraph.PaddingProperty, new Thickness(0, 0, 0, 10)));
            style.Setters.Add(new Setter(Paragraph.BorderThicknessProperty, new Thickness(0, 0, 0, 1)));
            style.Setters.Add(new Setter(Paragraph.BorderBrushProperty,
                GetResource("MahApps.Brushes.Gray5")));
            return style;
        }

        private static Style CreateHeading2Style()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 24.0));
            style.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 24, 0, 16)));
            style.Setters.Add(new Setter(Paragraph.PaddingProperty, new Thickness(0, 0, 0, 10)));
            style.Setters.Add(new Setter(Paragraph.BorderThicknessProperty, new Thickness(0, 0, 0, 1)));
            style.Setters.Add(new Setter(Paragraph.BorderBrushProperty,
                GetResource("MahApps.Brushes.Gray5")));
            return style;
        }

        private static Style CreateHeading3Style()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 20.0));
            style.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 24, 0, 16)));
            return style;
        }

        private static Style CreateHeading4Style()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 16.0));
            style.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 24, 0, 16)));
            return style;
        }

        private static Style CreateHeading5Style()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 14.0));
            style.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 24, 0, 16)));
            return style;
        }

        private static Style CreateHeading6Style()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 13.6));
            style.Setters.Add(new Setter(Paragraph.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Gray1")));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 24, 0, 16)));
            return style;
        }

        #endregion

        #region Code Styles

        private static Style CreateCodeStyle()
        {
            var style = new Style(typeof(Run));
            style.Setters.Add(new Setter(Run.FontFamilyProperty,
                new FontFamily("ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace")));
            style.Setters.Add(new Setter(Run.FontSizeProperty, 13.6));
            style.Setters.Add(new Setter(Run.BackgroundProperty,
                GetResource("MahApps.Brushes.Gray8")));
            style.Setters.Add(new Setter(Run.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            return style;
        }

        private static Style CreateCodeBlockStyle()
        {
            var style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Paragraph.FontFamilyProperty,
                new FontFamily("ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace")));
            style.Setters.Add(new Setter(Paragraph.FontSizeProperty, 13.6));
            style.Setters.Add(new Setter(Paragraph.BackgroundProperty,
                GetResource("MahApps.Brushes.Gray10")));
            style.Setters.Add(new Setter(Paragraph.ForegroundProperty,
                GetResource("MahApps.Brushes.Text")));
            style.Setters.Add(new Setter(Paragraph.PaddingProperty, new Thickness(16)));
            style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0, 0, 0, 16)));
            style.Setters.Add(new Setter(Paragraph.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Paragraph.BorderBrushProperty,
                GetResource("MahApps.Brushes.Gray5")));
            style.Setters.Add(new Setter(Paragraph.LineHeightProperty, 20.0));
            return style;
        }

        #endregion

        #region Quote Style

        private static Style CreateQuoteStyle()
        {
            var style = new Style(typeof(Section));
            style.Setters.Add(new Setter(Section.PaddingProperty, new Thickness(16, 0, 16, 0)));
            style.Setters.Add(new Setter(Section.MarginProperty, new Thickness(0, 0, 0, 16)));
            style.Setters.Add(new Setter(Section.BorderThicknessProperty, new Thickness(4, 0, 0, 0)));
            style.Setters.Add(new Setter(Section.BorderBrushProperty,
                GetResource("MahApps.Brushes.Gray5")));
            style.Setters.Add(new Setter(Section.ForegroundProperty,
                GetResource("MahApps.Brushes.Gray1")));
            return style;
        }

        #endregion

        #region Hyperlink Style

        private static Style CreateHyperlinkStyle()
        {
            var style = new Style(typeof(Hyperlink));
            style.Setters.Add(new Setter(Hyperlink.ForegroundProperty,
                GetResource("MahApps.Brushes.Accent")));
            style.Setters.Add(new Setter(Hyperlink.TextDecorationsProperty, null));

            var trigger = new Trigger { Property = Hyperlink.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Hyperlink.TextDecorationsProperty, TextDecorations.Underline));
            style.Triggers.Add(trigger);

            return style;
        }

        #endregion

        #region Table Styles

        private static Style CreateTableStyle()
        {
            var style = new Style(typeof(Table));
            style.Setters.Add(new Setter(Table.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Table.BorderBrushProperty,
                GetResource("MahApps.Brushes.Gray5")));
            style.Setters.Add(new Setter(Table.CellSpacingProperty, 0.0));
            style.Setters.Add(new Setter(Table.MarginProperty, new Thickness(0, 0, 0, 16)));
            return style;
        }

        private static Style CreateTableHeaderStyle()
        {
            var style = new Style(typeof(TableRowGroup));
            style.Setters.Add(new Setter(TableRowGroup.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(TableRowGroup.BackgroundProperty,
                GetResource("MahApps.Brushes.Gray10")));
            return style;
        }

        #endregion

        #region Helper Methods

        private static object GetResource(string resourceKey)
        {
            try
            {
                return Application.Current.Resources[resourceKey];
            }
            catch
            {
                return GetFallbackBrush(resourceKey);
            }
        }

        private static Brush GetFallbackBrush(string resourceKey)
        {
            switch (resourceKey)
            {
                case "MahApps.Brushes.Text": return new SolidColorBrush(Color.FromRgb(0x1F, 0x23, 0x28));
                case "MahApps.Brushes.Gray1": return new SolidColorBrush(Color.FromRgb(0x65, 0x6D, 0x76));
                case "MahApps.Brushes.Gray5": return new SolidColorBrush(Color.FromRgb(0xD1, 0xD9, 0xE0));
                case "MahApps.Brushes.Gray8": return new SolidColorBrush(Color.FromRgb(0xF6, 0xF8, 0xFA));
                case "MahApps.Brushes.Accent": return new SolidColorBrush(Color.FromRgb(0x09, 0x69, 0xDA));
                case "MahApps.Brushes.Control.Background": return new SolidColorBrush(Colors.White);
                default: return new SolidColorBrush(Colors.Black);
            }
        }

        #endregion
    }
}