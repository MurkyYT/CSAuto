using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace Markdown.Xaml
{
    public class Markdown : DependencyObject
    {
        /// <summary>
        /// maximum nested depth of [] and () supported by the transform; implementation detail
        /// </summary>
        private const int NestDepth = 6;

        /// <summary>
        /// Tabs are automatically converted to spaces as part of the transform  
        /// this constant determines how "wide" those tabs become in spaces  
        /// </summary>
        private const int TabWidth = 4;

        /// <summary>
        /// Default text alignment, equal to the one defined at document level.
        /// </summary>
        private static TextAlignment defaultTextAlignment = TextAlignment.Left;

        #region Style
        public Style DocumentStyle
        {
            get => (Style)GetValue(DocumentStyleProperty);
            set => SetValue(DocumentStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentStyleProperty =
            DependencyProperty.Register(nameof(DocumentStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style NormalParagraphStyle
        {
            get => (Style)GetValue(NormalParagraphStyleProperty);
            set => SetValue(NormalParagraphStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for NormalParagraphStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalParagraphStyleProperty =
            DependencyProperty.Register(nameof(NormalParagraphStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading1Style
        {
            get => (Style)GetValue(Heading1StyleProperty);
            set => SetValue(Heading1StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading1Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading1StyleProperty =
            DependencyProperty.Register(nameof(Heading1Style), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading2Style
        {
            get => (Style)GetValue(Heading2StyleProperty);
            set => SetValue(Heading2StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading2Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading2StyleProperty =
            DependencyProperty.Register(nameof(Heading2Style), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading3Style
        {
            get => (Style)GetValue(Heading3StyleProperty);
            set => SetValue(Heading3StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading3Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading3StyleProperty =
            DependencyProperty.Register(nameof(Heading3Style), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading4Style
        {
            get => (Style)GetValue(Heading4StyleProperty);
            set => SetValue(Heading4StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading4Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading4StyleProperty =
            DependencyProperty.Register(nameof(Heading4Style), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading5Style
        {
            get => (Style)GetValue(Heading5StyleProperty);
            set => SetValue(Heading5StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading5Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading5StyleProperty =
            DependencyProperty.Register(nameof(Heading5Style), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading6Style
        {
            get => (Style)GetValue(Heading6StyleProperty);
            set => SetValue(Heading6StyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Heading6Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading6StyleProperty =
            DependencyProperty.Register(nameof(Heading6Style), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style BlockQuoteStyle
        {
            get => (Style)GetValue(BlockQuoteStyleProperty);
            set => SetValue(BlockQuoteStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for BlockQuoteStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BlockQuoteStyleProperty =
            DependencyProperty.Register(nameof(BlockQuoteStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style NoteStyle
        {
            get => (Style)GetValue(NoteStyleProperty);
            set => SetValue(NoteStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for NoteStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteStyleProperty =
            DependencyProperty.Register(nameof(NoteStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style CodeBlockStyle
        {
            get => (Style)GetValue(CodeBlockStyleProperty);
            set => SetValue(CodeBlockStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for CodeBlockStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeBlockStyleProperty =
            DependencyProperty.Register(nameof(CodeBlockStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style CodeSpanStyle
        {
            get => (Style)GetValue(CodeSpanStyleProperty);
            set => SetValue(CodeSpanStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for CodeSpanStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeSpanStyleProperty =
            DependencyProperty.Register(nameof(CodeSpanStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style LinkStyle
        {
            get => (Style)GetValue(LinkStyleProperty);
            set => SetValue(LinkStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for LinkStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkStyleProperty =
            DependencyProperty.Register(nameof(LinkStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ImageStyle
        {
            get => (Style)GetValue(ImageStyleProperty);
            set => SetValue(ImageStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageStyleProperty =
            DependencyProperty.Register(nameof(ImageStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ImageFailedStyle
        {
            get => (Style)GetValue(ImageFailedStyleProperty);
            set => SetValue(ImageFailedStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageFailedStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageFailedStyleProperty =
            DependencyProperty.Register(nameof(ImageFailedStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ImageDownloadFailedStyle
        {
            get => (Style)GetValue(ImageDownloadFailedStyleProperty);
            set => SetValue(ImageDownloadFailedStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for ImageDownloadFailedStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageDownloadFailedStyleProperty =
            DependencyProperty.Register(nameof(ImageDownloadFailedStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style SeparatorStyle
        {
            get => (Style)GetValue(SeparatorStyleProperty);
            set => SetValue(SeparatorStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for SeparatorStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeparatorStyleProperty =
            DependencyProperty.Register(nameof(SeparatorStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public string AssetPathRoot
        {
            get => (string)GetValue(AssetPathRootProperty);
            set => SetValue(AssetPathRootProperty, value);
        }

        // Using a DependencyProperty as the backing store for AssetPathRoot.
        public static readonly DependencyProperty AssetPathRootProperty =
            DependencyProperty.Register(nameof(AssetPathRoot), typeof(string), typeof(Markdown), new PropertyMetadata(null));

        public Style TableStyle
        {
            get => (Style)GetValue(TableStyleProperty);
            set => SetValue(TableStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for TableStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableStyleProperty =
            DependencyProperty.Register(nameof(TableStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableHeaderStyle
        {
            get => (Style)GetValue(TableHeaderStyleProperty);
            set => SetValue(TableHeaderStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for TableHeaderStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableHeaderStyleProperty =
            DependencyProperty.Register(nameof(TableHeaderStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableBodyStyle
        {
            get => (Style)GetValue(TableBodyStyleProperty);
            set => SetValue(TableBodyStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for TableBodyStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableBodyStyleProperty =
            DependencyProperty.Register(nameof(TableBodyStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style TableRowAltStyle
        {
            get => (Style)GetValue(TableRowAltStyleProperty);
            set => SetValue(TableRowAltStyleProperty, value);
        }
        // Using a DependencyProperty as the backing store for TableRowAltStyle.  This enables animation, styling, binding, etc...

        public static readonly DependencyProperty TableRowAltStyleProperty =
            DependencyProperty.Register(nameof(TableRowAltStyle), typeof(Style), typeof(Markdown), new PropertyMetadata(null));
        #endregion Style

        public Markdown()
        {
            HyperlinkCommand = NavigationCommands.GoToPage;
        }

        public FlowDocument Transform(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            text = Normalize(text);

            defaultTextAlignment = TextAlignment.Left;
            defaultTextAlignment = GetTextAlignment(text);

            text = Regex.Replace(text, Alignment, "");
            text = new Regex(@"^\n+", RegexOptions.Compiled).Replace(text, "");

            var document = Create<FlowDocument, Block>(RunBlockGamut(text));

            if (DocumentStyle != null)
            {
                document.Style = DocumentStyle;
            }
            else
            {
                document.PagePadding = new Thickness(10);
            }
            document.TextAlignment = defaultTextAlignment;

            return document;
        }

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Block> RunBlockGamut(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return DoHeaders(text,
                n => DoNote(n,
                hr => DoHorizontalRules(hr,
                l => DoLists(l,
                t => DoTable(t,
                q => DoBlockQuotes(q, 
                c => DoCodeBlock(c,
                FormParagraphs)))))));

            //// We already ran HashHTMLBlocks() before, in Markdown(), but that
            //// was to escape raw HTML in the original Markdown source. This time,
            //// we're escaping the markup we've just created, so that we don't wrap
            //// <p> tags around block-level tags.
            //text = HashHTMLBlocks(text);

            //text = FormParagraphs(text);

            //return text;
        }

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        private IEnumerable<Inline> RunSpanGamut(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return DoColor(text,
                s1 => DoCodeSpan(s1,
                s2 => DoAnchors(s2,
                s3 => DoImages(s3,
                s4 => DoTextDecorations(s4,
                DoText)))));

            //text = EscapeSpecialCharsWithinTagAttributes(text);
            //text = EscapeBackslashes(text);

            //// Images must come first, because ![foo][f] looks like an anchor.
            //text = DoImages(text);
            //text = DoAnchors(text);

            //// Must come after DoAnchors(), because you can use < and >
            //// delimiters in inline links like [this](<url>).
            //text = DoAutoLinks(text);

            //text = EncodeAmpsAndAngles(text);
            //text = DoItalicsAndBold(text);
            //text = DoHardBreaks(text);

            //return text;
        }

        private static readonly Regex NewlinesLeadingTrailing = new Regex(@"^\n+|\n+\z", RegexOptions.Compiled);
        private static readonly Regex NewlinesMultiple = new Regex(@"\n{2,}", RegexOptions.Compiled);
        private static readonly Regex LeadingWhitespace = new Regex(@"^[ ]*", RegexOptions.Compiled);
        private const string Alignment = @"^\|-\||^\|:-\||^\|-:\||^\|:-:\||^\|=\|";

        /// <summary>
        /// splits on two or more newlines, to form "paragraphs";    
        /// </summary>
        private IEnumerable<Block> FormParagraphs(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // split on two or more newlines
            var grafs = NewlinesMultiple.Split(NewlinesLeadingTrailing.Replace(text, ""));

            foreach (var g in grafs)
            {
                var textAlignment = GetTextAlignment(text);
                var block = Create<Paragraph, Inline>(RunSpanGamut(Regex.Replace(g, Alignment, "")));
                block.Style = NormalParagraphStyle;
                block.TextAlignment = textAlignment;
                yield return block;
            }
        }

        private static string _nestedBracketsPattern;

        /// <summary>
        /// Reusable pattern to match balanced [brackets]. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to _nestDepth
            if(_nestedBracketsPattern == null)
            {
                _nestedBracketsPattern = RepeatString(@"
                    (?>              # Atomic matching
                       [^\[\]]+      # Anything other than brackets
                     |
                       \[
                           ", NestDepth) + RepeatString(
                @" \]
                    )*"
                , NestDepth);
            }
            return _nestedBracketsPattern;
        }

        private static string _nestedParensPattern;

        /// <summary>
        /// Reusable pattern to match balanced (parens). See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if(_nestedParensPattern == null)
            {
                _nestedParensPattern = RepeatString(@"
                    (?>              # Atomic matching
                       [^()\s]+      # Anything other than parens or whitespace
                     |
                       \(
                           ", NestDepth) + RepeatString(
                @" \)
                    )*"
                , NestDepth);
            }
            return _nestedParensPattern;
        }

        private static string _nestedParensPatternWithWhiteSpace;

        /// <summary>
        /// Reusable pattern to match balanced (parens), including whitespace. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        private static string GetNestedParensPatternWithWhiteSpace()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to _nestDepth
            if (_nestedParensPatternWithWhiteSpace == null)
            {
                _nestedParensPatternWithWhiteSpace = RepeatString(@"
                    (?>              # Atomic matching
                       [^()]+        # Anything other than parens
                     |
                       \(
                           ", NestDepth) + RepeatString(
                @" \)
                    )*"
                , NestDepth);
            }
            return _nestedParensPatternWithWhiteSpace;
        }

        #region Image
        private static readonly Regex ImageInline = new Regex(
            string.Format(CultureInfo.InvariantCulture, @"
                (?:
                    !\[
                        (?<alt>{0})         # link text
                    \]
                    \(
                        [ ]*
                        (?<url>{1})         # image URI
                        [ ]*
                        (?:
                            [ ]*
                            (?<scaling>\d*[.]?\d+)%
                            [ ]*
                        )?                  # size is optional
                        (?:
                            (?<quote>['""]) # quote char
                                (?<tag>.*?) # tag
                            \k<quote>       # matching quote
                            [ ]*            # ignore any spaces between closing quote and )
                        )?                  # tag is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPattern()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown images into images
        /// </summary>
        /// <remarks>
        /// ![image alt](url "tag") 
        /// </remarks>
        private IEnumerable<Inline> DoImages(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, ImageInline, ImageInlineEvaluator, defaultHandler);
        }

        private Inline ImageInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var imageAlt = match.Groups["alt"].Value;
            var url = match.Groups["url"].Value;
            var tag = match.Groups["tag"].Value;

            if (!double.TryParse(match.Groups["scaling"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var scaling))
                scaling = 100.0;

            BitmapImage imgSource;
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !Path.IsPathRooted(url))
                {
                    url = Path.Combine(AssetPathRoot ?? string.Empty, url);
                }

                imgSource = CreateBitmapImage(url);
            }
            catch (Exception)
            {
                var img = new Image
                {
                    ToolTip = ToolTipForImageFailed(imageAlt, url),
                    Style = ImageFailedStyle
                };

                // in case ImageFailedStyle provides a Source (e.g. DrawingImage)
                if (img.Source != null) return new InlineUIContainer(img);

                img.Source = CreateBitmapImage(); // load "broken image" icon from resources

                if (img.Source == null) // when even getting a placeholder from resources fails...
                    return new Run("!" + (!string.IsNullOrEmpty(imageAlt) ? imageAlt : url)) { Style = ImageDownloadFailedStyle };

                return new InlineUIContainer(img);
            }

            var image = new Image { Source = imgSource, Tag = tag };
            if (!string.IsNullOrWhiteSpace(imageAlt))
            {
                image.ToolTip = imageAlt;
            }
            if (ImageStyle is null)
            {
                image.Margin = new Thickness(0);
            }
            else
            {
                image.Style = ImageStyle;
            }

            // Bind size so document is updated when image is downloaded
            if (imgSource.IsDownloading)
            {
                //var binding = new Binding(nameof(BitmapImage.Width)) {Source = imgSource, Mode = BindingMode.OneWay};
                //var bindingExpression = BindingOperations.SetBinding(image, FrameworkElement.WidthProperty, binding);

                void DownloadCompletedHandler(object sender, EventArgs e)
                {
                    var source = (BitmapImage)sender;
                    source.DownloadCompleted -= DownloadCompletedHandler;
                    source.DownloadFailed -= DownloadFailedHandler;
                    image.Width = source.Width * scaling / 100.0;
                    //bindingExpression.UpdateTarget();
                }

                void DownloadFailedHandler(object sender, EventArgs e)
                {
                    var source = (BitmapImage)sender;
                    source.DownloadCompleted -= DownloadCompletedHandler;
                    source.DownloadFailed -= DownloadFailedHandler;

                    // default image when download failed
                    image.Source = CreateBitmapImage(); ;
                    image.Width = imgSource.Width;
                    image.ToolTip = ToolTipForImageFailed(imageAlt, url);
                    image.Style = ImageFailedStyle;
                }

                imgSource.DownloadCompleted += DownloadCompletedHandler;
                imgSource.DownloadFailed += DownloadFailedHandler;
            }
            else
            {
                // local resource
                image.Width = imgSource.Width * scaling / 100.0;
            }

            return new InlineUIContainer(image);
        }

        /// <summary>
        /// Return a BitmapImage to add it as a source to the Image.
        /// </summary>
        /// <param name="url">Url of the image to load.</param>
        private static BitmapImage CreateBitmapImage(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                try
                {
                    var stream = new MemoryStream();

                    //MarkdownFlowDocument.Properties.Resources.ImageFailed.Save(stream, ImageFormat.Bmp);

                    stream.Position = 0;
                    var result = new BitmapImage();
                    result.BeginInit();
                    // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                    // Force the bitmap to load right now so we can dispose the stream.
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.StreamSource = stream;
                    result.EndInit();
                    result.Freeze();
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            var imgSource = new BitmapImage();
            imgSource.BeginInit();
            imgSource.CacheOption = BitmapCacheOption.None;
            imgSource.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
            imgSource.CacheOption = BitmapCacheOption.OnLoad;
            imgSource.CreateOptions = BitmapCreateOptions.None;
            imgSource.UriSource = new Uri(url);
            imgSource.EndInit();     // System.IO.FileNotFoundException:
            return imgSource;
        }

        /// <summary>
        /// Create a ToolTip to set on the image when it fails to load.
        /// </summary>
        /// <param name="imageAlt">Image alternative text.</param>
        /// <param name="url">File location.</param>
        private static StackPanel ToolTipForImageFailed(string imageAlt, string url)
        {
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock()
            {
                Text = imageAlt
            });
            stackPanel.Children.Add(new TextBlock()
            {
                Text = url
            });

            return stackPanel;
        }
        #endregion Image

        #region HyperLink
        public ICommand HyperlinkCommand { get; set; }

        private static readonly Regex AnchorInline = new Regex(
             $@"
                (?:^|(?<![!]))                                             # no preceeding ! to separate from image
                (?:                                                        # wrap whole match
                    \[
                        (?<text>{GetNestedBracketsPattern()})              # link text 
                    \]
                    (?:
                        \(                                                 # literal paren
                            (?:
                                [ ]*
                                (?<href>{GetNestedParensPattern()})        # href
                            )?                      # href is optional
                            [ ]*
                            (?:
                                (?<quote>['""])     # quote char
                                (?<title>.*?)       # title
                                \k<quote>           # matching quote
                                [ ]*                # ignore any spaces between closing quote and )
                            )?                      # title is optional
                        \)
                    )?
                )", RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        /// [link text](url "title") 
        /// </remarks>
        private IEnumerable<Inline> DoAnchors(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // Next, inline-style links: [link text](url "optional title")
            return Evaluate(text, AnchorInline, AnchorInlineEvaluator, defaultHandler);
        }

        private Inline AnchorInlineEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var linkText = match.Groups["text"].Value;
            var url = match.Groups["href"].Value;
            var title = match.Groups["title"].Value;

            var result = Create<Hyperlink, Inline>(RunSpanGamut(linkText));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;

            // hyperlink without url: use the link's text as target
            // (exception: hyperlink has image instead of text, then target still remains empty)
            if (string.IsNullOrEmpty(url))
            {
                result.CommandParameter = ImageInline.IsMatch(linkText) ? string.Empty : linkText;
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                result.ToolTip = title;
            }
            result.Style = LinkStyle;

            return result;
        }
        #endregion HyperLink

        #region Header
        private static readonly Regex HeaderSetext = new Regex(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex HeaderAtx = new Regex(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \#*         # optional closing #'s (not counted)
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// 
        /// # Header 1  
        /// ## Header 2  
        /// ## Header 2 with closing hashes ##  
        /// ...  
        /// ###### Header 6
        /// </remarks>
        private IEnumerable<Block> DoHeaders(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, HeaderSetext, SetextHeaderEvaluator,
                s => Evaluate(s, HeaderAtx, AtxHeaderEvaluator, defaultHandler));
        }

        private Block SetextHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var header = match.Groups[1].Value;
            var level = match.Groups[2].Value.StartsWith("=", StringComparison.Ordinal) ? 1 : 2;

            //TODO: Style the paragraph based on the header level
            return CreateHeader(level, RunSpanGamut(header.Trim()));
        }

        private Block AtxHeaderEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var header = match.Groups[2].Value;
            var level = match.Groups[1].Value.Length;
            var textAlignment = GetTextAlignment(header);
            return CreateHeader(level, RunSpanGamut(Regex.Replace(header, Alignment, "")), textAlignment);
        }

        public Block CreateHeader(int level, IEnumerable<Inline> content, TextAlignment textAlignment = TextAlignment.Left)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var block = Create<Paragraph, Inline>(content);

            var s = new StringBuilder();

            for (var position = block.ContentStart; position?.CompareTo(block.ContentEnd) < 0; position = position.GetNextContextPosition(LogicalDirection.Forward))
            {
                var piece = position.GetTextInRun(LogicalDirection.Forward);
                if (piece.Length == 0) continue;
                if (s.Length > 0) s.Append(" ");
                s.Append(piece);
            }

            block.Tag = s.ToString().Trim();

            switch (level)
            {
                case 1:
                    block.Style = Heading1Style;
                    break;
                case 2:
                    block.Style = Heading2Style;
                    break;
                case 3:
                    block.Style = Heading3Style;
                    break;
                case 4:
                    block.Style = Heading4Style;
                    break;
                case 5:
                    block.Style = Heading5Style;
                    break;
                case 6:
                    block.Style = Heading6Style;
                    break;
            }

            block.TextAlignment = textAlignment;

            return block;
        }
        #endregion Header

        #region BlockQuotes
        private static readonly Regex Blockquotes = new Regex(@"
                ^(\>{1,9})  # $1 = string of >'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown into HTML blockquotes.
        /// </summary>
        /// <remarks>
        /// > quote 1
        /// >> quote 2
        /// ...
        /// >>>>>>>>> quote 9
        /// </remarks>
        private IEnumerable<Block> DoBlockQuotes(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, Blockquotes, BlockQuotesEvaluator, defaultHandler);
        }

        private Block BlockQuotesEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var text = match.Groups[2].Value;
            var level = match.Groups[1].Value.Length;
            var textAlignment = GetTextAlignment(text);
            return CreateBlockQuotes(level, RunSpanGamut(Regex.Replace(text, Alignment, "")), textAlignment);
        }

        public Block CreateBlockQuotes(int level, IEnumerable<Inline> content, TextAlignment textAlignment = TextAlignment.Left)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            //var block = Create<Paragraph, Inline>(content);
            //block.Style = CommentStyle;
            //block.TextAlignment = textAlignment;

            //return block;

            return new BlockUIContainer(CreateBorderBlockQuotes(level, content));
        }

        public Border CreateBorderBlockQuotes(int level, IEnumerable<Inline> content)
        {
            if (level > 1)
            {
                return new Border()
                {
                    BorderBrush = Brushes.Silver,
                    BorderThickness = new Thickness(2, 0, 0, 0),
                    Child = CreateBorderBlockQuotes(level - 1, content),
                    Padding = new Thickness(10, 0, 0, 0)
                };
            }

            var commentFDoc = new FlowDocument(Create<Paragraph, Inline>(content));
            //{
            //    Style = DocumentStyle,
            //    TextAlignment = defaultTextAlignment
            //};

            return new Border
            {
                BorderBrush = Brushes.Silver,
                BorderThickness = new Thickness(2, 0, 0, 0),
                Child = new RichTextBox(commentFDoc)
                {
                    Style = BlockQuoteStyle
                },
                Padding = new Thickness(10, 0, 0, 0)
            };
        }
        #endregion BlockQuotes

        #region Note
        private static readonly Regex Note = new Regex(@"
                ^(\<)       # $1 = starting marker <
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \>*         # optional closing >'s (not counted)
                \n+
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown into HTML paragraphs.
        /// </summary>
        /// <remarks>
        /// Note
        /// </remarks>
        private IEnumerable<Block> DoNote(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, Note, NoteEvaluator, defaultHandler);
        }

        private Block NoteEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var text = match.Groups[2].Value;
            var textAlignment = GetTextAlignment(text);
            return NoteComment(RunSpanGamut(Regex.Replace(text, Alignment, "")), textAlignment);
        }

        public Block NoteComment(IEnumerable<Inline> content, TextAlignment textAlignment = TextAlignment.Left)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var block = Create<Paragraph, Inline>(content);
            block.Style = NoteStyle;
            block.TextAlignment = textAlignment;

            return block;
        }
        #endregion Note

        #region Horizontal Rules
        private static readonly Regex HorizontalRules = HorizontalRulesRegex("-");
        private static readonly Regex HorizontalTwoLinesRules = HorizontalRulesRegex("=");
        private static readonly Regex HorizontalBoldRules = HorizontalRulesRegex("*");
        private static readonly Regex HorizontalBoldWithSingleRules = HorizontalRulesRegex("_");

        /// <summary>
        /// Create regex expression for horizontal rules.
        /// </summary>
        /// <param name="markers">String of markers (e.g.: - or -*_).</param>
        /// <returns>Regex expression.</returns>
        /// <remarks>
        /// e.g.
        /// ---
        /// - - -
        /// -  -  -
        /// </remarks>
        private static Regex HorizontalRulesRegex(string markers)
        {
            return new Regex(@"
                ^[ ]{0,3}                   # Leading space
                    ([" + markers + @"])    # $1: First marker ([markers])
                    (?>                     # Repeated marker group
                        [ ]{0,2}            # Zero, one, or two spaces.
                        \1                  # Marker character
                    ){2,}                   # Group repeated at least twice
                    [ ]*                    # Trailing spaces
                    $                       # End of line.
                ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        }

        /// <summary>
        /// Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        private IEnumerable<Block> DoHorizontalRules(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, HorizontalRules, RuleEvaluator,
                s1 => Evaluate(s1, HorizontalTwoLinesRules, TwoLinesRuleEvaluator,
                s2 => Evaluate(s2, HorizontalBoldRules, BoldRuleEvaluator,
                s3 => Evaluate(s3, HorizontalBoldWithSingleRules, BoldWithSingleRuleEvaluator, defaultHandler))));
        }

        /// <summary>
        /// Single line separator.
        /// </summary>
        private Block RuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            return new BlockUIContainer(new Separator()
            {
                Style = SeparatorStyle
            });
        }

        /// <summary>
        /// Two lines separator.
        /// </summary>
        private Block TwoLinesRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var stackPanel = new StackPanel();
            for (var i = 0; i < 2; i++)
            {
                stackPanel.Children.Add(new Separator()
                {
                    Style = SeparatorStyle
                });
            }

            return new BlockUIContainer(stackPanel);
        }

        /// <summary>
        /// Double line separator.
        /// </summary>
        private Block BoldRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var stackPanel = new StackPanel();
            for (var i = 0; i < 2; i++)
            {
                stackPanel.Children.Add(new Separator()
                {
                    Style = SeparatorStyle,
                    Margin = new Thickness(0)
                });
            }

            return new BlockUIContainer(stackPanel);
        }

        /// <summary>
        /// Two lines separator consisting of a double line and a single line.
        /// </summary>
        private Block BoldWithSingleRuleEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var stackPanel = new StackPanel();
            for (var i = 0; i < 2; i++)
            {
                stackPanel.Children.Add(new Separator()
                {
                    Style = SeparatorStyle,
                    Margin = new Thickness(0)
                });
            }
            stackPanel.Children.Add(new Separator()
            {
                Style = SeparatorStyle
            });

            return new BlockUIContainer(stackPanel);
        }
        #endregion Horizontal Rules

        #region List
        private const string MarkerUl = @"[*+=-]";
        private const string MarkerOl = @"\d+[.]|\p{L}+[.,]";

        // Unordered List
        private const string MarkerUlDisc = @"[*]";
        private const string MarkerUlBox = @"[+]";
        private const string MarkerUlCircle = @"[-]";
        private const string MarkerUlSquare = @"[=]";

        // Ordered List
        private const string MarkerOlNumber = @"\d+[.]";
        private const string MarkerOlLetterLower = @"\p{Ll}+[.]";
        private const string MarkerOlLetterUpper = @"\p{Lu}+[.]";
        private const string MarkerOlRomanLower = @"\p{Ll}+[,]";
        private const string MarkerOlRomanUpper = @"\p{Lu}+[,]";

        private int _listLevel;

        /// <summary>
        /// Maximum number of levels a single list can have.
        /// In other words, _listDepth - 1 is the maximum number of nested lists.
        /// </summary>
        private const int ListDepth = 6;

        private static readonly string WholeList
            = string.Format(CultureInfo.InvariantCulture, @"
            (                               # $1 = whole list
              (                             # $2
                [ ]{{0,{1}}}
                ({0})                       # $3 = first list item marker
                [ ]+
              )
              (?s:.+?)
              (                             # $4
                  \z
                |
                  \n{{2,}}
                  (?=\S)
                  (?!                       # Negative lookahead for another list item marker
                    [ ]*
                    {0}[ ]+
                  )
              )
            )", string.Format(CultureInfo.InvariantCulture, "(?:{0}|{1})", MarkerUl, MarkerOl), ListDepth - 1);

        private static readonly Regex ListNested = new Regex(@"^" + WholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex ListTopLevel = new Regex(@"(?:(?<=\n\n)|\A\n?)" + WholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        private IEnumerable<Block> DoLists(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // We use a different prefix before nested lists than top-level lists.
            // See extended comment in _ProcessListItems().
            if (_listLevel > 0)
                return Evaluate(text, ListNested, ListEvaluator, defaultHandler);
            else
                return Evaluate(text, ListTopLevel, ListEvaluator, defaultHandler);
        }

        private Block ListEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var list = match.Groups[1].Value;
            var listType = Regex.IsMatch(match.Groups[3].Value, MarkerUl) ? "ul" : "ol";

            // Set text marker style.
            var textMarker = GetTextMarkerStyle(listType, match);

            // Turn double returns into triple returns, so that we can make a
            // paragraph for the last item in a list, if necessary:
            //list = Regex.Replace(list, @"\n{2,}", "\n\n\n");

            var resultList = Create<List, ListItem>(ProcessListItems(list, listType == "ul" ? MarkerUl : MarkerOl));

            resultList.MarkerStyle = textMarker;

            return resultList;
        }

        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        private IEnumerable<ListItem> ProcessListItems(string list, string marker)
        {
            // The listLevel global keeps track of when we're inside a list.
            // Each time we enter a list, we increment it; when we leave a list,
            // we decrement. If it's zero, we're not in a list anymore.

            // We do this because when we're not inside a list, we want to treat
            // something like this:

            //    I recommend upgrading to version
            //    8. Oops, now this line is treated
            //    as a sub-list.

            // As a single paragraph, despite the fact that the second line starts
            // with a digit-period-space sequence.

            // Whereas when we're inside a list (or sub-list), that line will be
            // treated as the start of a sub-list. What a kludge, huh? This is
            // an aspect of Markdown's syntax that's hard to parse perfectly
            // without resorting to mind-reading. Perhaps the solution is to
            // change the syntax rules such that sub-lists must start with a
            // starting cardinal number; e.g. "1." or "a.".

            _listLevel++;
            try
            {
                // Trim trailing blank lines:
                list = Regex.Replace(list, @"\n{2,}\z", "\n");

                var pattern = string.Format(CultureInfo.InvariantCulture,
                  @"(\n)?                  # leading line = $1
                (^[ ]*)                    # leading whitespace = $2
                ({0}) [ ]+                 # list marker = $3
                ((?s:.+?)                  # list item text = $4
                (\n{{1,2}}))      
                (?= \n* (\z | \2 ({0}) [ ]+))", marker);

                var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                var matches = regex.Matches(list);
                foreach (Match m in matches)
                {
                    yield return ListItemEvaluator(m);
                }
            }
            finally
            {
                _listLevel--;
            }
        }

        private ListItem ListItemEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var item = match.Groups[4].Value;
            var leadingLine = match.Groups[1].Value;

            if (!String.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..
                return Create<ListItem, Block>(RunBlockGamut(item));
            else
            {
                // recursion for sub-lists
                return Create<ListItem, Block>(RunBlockGamut(item));
            }
        }

        /// <summary>
        /// Get the text marker style based on a specific regex.
        /// </summary>
        /// <param name="listType">Specify what kind of list: ul, ol.</param>
        /// <param name="match"></param>
        private static TextMarkerStyle GetTextMarkerStyle(string listType, Match match)
        {
            switch (listType)
            {
                case "ul":
                    if (Regex.IsMatch(match.Groups[3].Value, MarkerUlDisc))
                    {
                        return TextMarkerStyle.Disc;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerUlBox))
                    {
                        return TextMarkerStyle.Box;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerUlCircle))
                    {
                        return TextMarkerStyle.Circle;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerUlSquare))
                    {
                        return TextMarkerStyle.Square;
                    }
                    break;
                case "ol":
                    if (Regex.IsMatch(match.Groups[3].Value, MarkerOlNumber))
                    {
                        return TextMarkerStyle.Decimal;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerOlLetterLower))
                    {
                        return TextMarkerStyle.LowerLatin;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerOlLetterUpper))
                    {
                        return TextMarkerStyle.UpperLatin;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerOlRomanLower))
                    {
                        return TextMarkerStyle.LowerRoman;
                    }
                    else if (Regex.IsMatch(match.Groups[3].Value, MarkerOlRomanUpper))
                    {
                        return TextMarkerStyle.UpperRoman;
                    }
                    break;
            }
            return TextMarkerStyle.None;
        }
        #endregion List

        #region Table
        private static readonly Regex Table = new Regex(@"
            (                               # $1 = whole table
                [ \r\n]*
                (                           # $2 = table header
                    \|([^|\r\n]*\|)+        # $3
                )
                [ ]*\r?\n[ ]*
                (                           # $4 = column style
                    =?\|(:?-+:?\|)+         # $5
                )
                (                           # $6 = table row
                    (                       # $7
                        [ ]*\r?\n[ ]*
                        \|([^|\r\n]*\|)+    # $8
                    )+
                )
            )",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public IEnumerable<Block> DoTable(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, Table, TableEvalutor, defaultHandler);
        }

        private Block TableEvalutor(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var wholeTable = match.Groups[1].Value;
            var header = match.Groups[2].Value.Trim();
            var style = match.Groups[4].Value.Trim();
            var row = match.Groups[6].Value.Trim();
            var rowAlt = style.Substring(0, 1) == "=";

            var styles = style.Substring(1 + Convert.ToInt32(rowAlt), style.Length - (2 + Convert.ToInt32(rowAlt))).Split('|');
            var headers = header.Substring(1, header.Length - 2).Split('|');
            var rowList = row.Split('\n').Select(ritm =>
            {
                var trimRitm = ritm.Trim();
                return trimRitm.Substring(1, trimRitm.Length - 2).Split('|');
            }).ToList();

            var maxColCount =
                Math.Max(
                    Math.Max(styles.Length, headers.Length),
                    rowList.Select(ritm => ritm.Length).Max()
                );

            // table style
            var aligns = new List<TextAlignment?>();
            foreach (var colStyleTxt in styles)
            {
                var firstChar = colStyleTxt.First();
                var lastChar = colStyleTxt.Last();
                // center
                if (firstChar == ':' && lastChar == ':')
                {
                    aligns.Add(TextAlignment.Center);
                }
                // right
                else if (lastChar == ':')
                {
                    aligns.Add(TextAlignment.Right);
                }
                // left
                else if (firstChar == ':')
                {
                    aligns.Add(TextAlignment.Left);
                }
                // default
                else
                {
                    aligns.Add(null);
                }
            }
            while (aligns.Count < maxColCount)
            {
                aligns.Add(null);
            }

            // table
            var table = new Table()
            {
                Style = TableStyle
            };

            // table columns
            while (table.Columns.Count < maxColCount)
            {
                table.Columns.Add(new TableColumn());
            }

            // table header
            var tableHeaderRg = new TableRowGroup()
            {
                Style = TableHeaderStyle
            };

            var tableHeader = CreateTableRow(headers, aligns);
            tableHeaderRg.Rows.Add(tableHeader);
            table.RowGroups.Add(tableHeaderRg);

            // row
            var tableBodyRg = new TableRowGroup()
            {
                Style = TableBodyStyle
            };
            foreach (var rowAry in rowList)
            {
                var tableBody = CreateTableRow(rowAry, aligns, rowAlt && (rowList.IndexOf(rowAry) % 2) == 1 );
                tableBodyRg.Rows.Add(tableBody);
            }
            table.RowGroups.Add(tableBodyRg);

            return table;
        }

        private TableRow CreateTableRow(string[] txts, List<TextAlignment?> aligns, bool isOddRow = false)
        {
            var tableRow = new TableRow()
            {
                Style = isOddRow ? TableRowAltStyle : null
            };

            foreach (var idx in Enumerable.Range(0, txts.Length))
            {
                var txt = txts[idx];
                var align = aligns[idx];

                var paragraph = Create<Paragraph, Inline>(RunSpanGamut(txt));
                var cell = new TableCell(paragraph);

                if (align.HasValue)
                {
                    cell.TextAlignment = align.Value;
                }

                tableRow.Cells.Add(cell);
            }

            while (tableRow.Cells.Count < aligns.Count)
            {
                tableRow.Cells.Add(new TableCell());
            }

            return tableRow;
        }
        #endregion Table

        #region CodeBlock
        private static readonly Regex CodeBlock = new Regex(@"
                \n
                \s*```               # starting marker ```
                (?<lang>\w+?)?       # language (for syntax highlighting)
                \s*\n
                (?<code>.+?)         # code text
                \s*```\s*            # closing marker ```
                \n+
            ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown « code block » into HTML code tags
        /// </summary>
        private IEnumerable<Block> DoCodeBlock(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return Evaluate(text, CodeBlock, CodeBlockEvaluator, defaultHandler);
        }

        private Block CodeBlockEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var text = match.Groups["code"].Value;

            var stack = new StackPanel();

            foreach (var line in text.Split('\r', '\n'))
            {
                var t = Eoln.Replace(line, " ");
                stack.Children.Add(new TextBlock { Style = CodeBlockStyle, Text = t, TextWrapping = TextWrapping.NoWrap});
            }

            var scroller = new ScrollViewer 
            {
                Content = stack, 
                HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto, 
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var container = new BlockUIContainer {Child = scroller};
            return container;
        }
        #endregion CodeBlock

        #region CodeSpan
        private static readonly Regex CodeSpan = new Regex(@"
                (?<!\\)   # Character before opening ` can't be a backslash
                (`+)      # $1 = Opening run of `
                (.+?)     # $2 = The code block
                (?<!`)
                \1
                (?!`)
            ", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown `code spans` into HTML code tags
        /// </summary>
        private IEnumerable<Inline> DoCodeSpan(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            //    * You can use multiple backticks as the delimiters if you want to
            //        include literal backticks in the code span. So, this input:
            //
            //        Just type ``foo `bar` baz`` at the prompt.
            //
            //        Will translate to:
            //
            //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
            //
            //        There's no arbitrary limit to the number of backticks you
            //        can use as delimters. If you need three consecutive backticks
            //        in your code, use four for delimiters, etc.
            //
            //    * You can use spaces to get literal backticks at the edges:
            //
            //          ... type `` `bar` `` ...
            //
            //        Turns to:
            //
            //          ... type <code>`bar`</code> ...         
            //

            return Evaluate(text, CodeSpan, CodeSpanEvaluator, defaultHandler);
        }

        private Inline CodeSpanEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            return new Run(span) 
            {
                Style = CodeSpanStyle
            };
        }
        #endregion CodeSpan

        #region Text Decorations
        private static readonly Regex Bold = new Regex(@"(\*\*) (?=\S) (.+?[*_]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex Italic = new Regex(@"(\*) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex Strikethrough = new Regex(@"(__) (?=\S) (.+?[*_]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex Underline = new Regex(@"(_) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        private IEnumerable<Inline> DoTextDecorations(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            // <strong> must go first, then <em>
            return Evaluate(text, Bold, m => BoldEvaluator(m, 2),
                s1 => Evaluate(s1, Italic, m => ItalicEvaluator(m, 2),
                s2 => Evaluate(s2, Strikethrough, m => StrikethroughEvaluator(m, 2),
                s3 => Evaluate(s3, Underline, m => UnderlineEvaluator(m, 2),
                defaultHandler))));
        }

        private Inline BoldEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            return Create<Bold, Inline>(RunSpanGamut(content));
        }

        private Inline ItalicEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            return Create<Italic, Inline>(RunSpanGamut(content));
        }

        private Inline StrikethroughEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            return Create<Strikethrough, Inline>(RunSpanGamut(content));
        }

        private Inline UnderlineEvaluator(Match match, int contentGroup)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var content = match.Groups[contentGroup].Value;
            return Create<Underline, Inline>(RunSpanGamut(content));
        }
        #endregion Text Decorations

        #region Color
        private static readonly Regex Color = new Regex(
            string.Format(CultureInfo.InvariantCulture, @"
                (                           # wrap whole match in $1
                    color\/\[
                        ({0})               # color brush name = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # text = $3
                        [ ]*
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPatternWithWhiteSpace()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Color spans.
        /// </summary>
        /// <remarks>
        /// color/[ColorBrushName or HexColor](text to color) 
        /// ColorBrushName is not case sensitive
        /// HexColor e.g. #29F, #FFFFFF, #88000000
        /// </remarks>
        private IEnumerable<Inline> DoColor(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return Evaluate(text, Color, ColorEvaluator, defaultHandler);
        }

        private Inline ColorEvaluator(Match match)
        {
            if (match is null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            var text = match.Groups[3].Value;
            text = Regex.Replace(text, @"^[ ]*", ""); // leading whitespace
            text = Regex.Replace(text, @"[ ]*$", ""); // trailing whitespace

            var span = Create<Span, Inline>(RunSpanGamut(text));

            try
            {
                if (new Regex(@"^\#").IsMatch(match.Groups[2].Value))
                {
                    span.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(match.Groups[2].Value));
                }
                else
                {
                    span.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(match.Groups[2].Value);
                }
            }
            catch
            {
                span.Foreground = Brushes.Black;
            }

            return span;
        }
        #endregion Color

        private static readonly Regex OutDent = new Regex(@"^[ ]{1," + TabWidth + @"}", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Remove one level of line-leading spaces
        /// </summary>
        private string Outdent(string block)
        {
            return OutDent.Replace(block, "");
        }

        /// <summary>
        /// convert all tabs to _tabWidth spaces; 
        /// standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
        /// makes sure text ends with a couple of newlines; 
        /// removes any blank lines (only spaces) in the text
        /// </summary>
        private static string Normalize(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var output = new StringBuilder(text.Length);
            var line = new StringBuilder();
            var valid = false;

            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        if (valid)
                            output.Append(line);
                        output.Append('\n');
                        line.Length = 0;
                        valid = false;
                        break;
                    case '\r':
                        if ((i < text.Length - 1) && (text[i + 1] != '\n'))
                        {
                            if (valid)
                                output.Append(line);
                            output.Append('\n');
                            line.Length = 0;
                            valid = false;
                        }
                        break;
                    case '\t':
                        var width = (TabWidth - line.Length % TabWidth);
                        for (var k = 0; k < width; k++)
                            line.Append(' ');
                        break;
                    case '\x1A':
                        break;
                    default:
                        if (!valid && text[i] != ' ')
                            valid = true;
                        line.Append(text[i]);
                        break;
                }
            }

            if (valid)
                output.Append(line);
            output.Append('\n');

            // add two newlines to the end before return
            return output.Append("\n\n").ToString();
        }

        /// <summary>
        /// Return text alignment for the document based on a specific marker.
        /// </summary>
        private static TextAlignment GetTextAlignment(string text)
        {
            if (new Regex(@"^\|-\|").IsMatch(text) ||
                new Regex(@"^\|:-\|").IsMatch(text))
            {
                return TextAlignment.Left;
            }
            if (new Regex(@"^\|-:\|").IsMatch(text))
            {
                return TextAlignment.Right;
            }
            if (new Regex(@"^\|:-:\|").IsMatch(text))
            {
                return TextAlignment.Center;
            }
            if (new Regex(@"^\|=\|").IsMatch(text))
            {
                return TextAlignment.Justify;
            }
            return defaultTextAlignment;
        }

        /// <summary>
        /// this is to emulate what's available in PHP
        /// </summary>
        private static string RepeatString(string value, int count)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var sb = new StringBuilder(value.Length * count);
            for (var i = 0; i < count; i++)
                sb.Append(value);
            return sb.ToString();
        }

        private static TResult Create<TResult, TContent>(IEnumerable<TContent> content)
            where TResult : IAddChild, new()
        {
            var result = new TResult();
            foreach (var c in content)
            {
                try
                {
                    result.AddChild(c);
                }
                catch (InvalidOperationException)
                {
                    result.AddText(c.ToString());
                }
            }

            return result;
        }

        private static IEnumerable<T> Evaluate<T>(string text, Regex expression, Func<Match, T> build, Func<string, IEnumerable<T>> rest)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var matches = expression.Matches(text);
            var index = 0;
            foreach (Match m in matches)
            {
                if (m.Index > index)
                {
                    var prefix = text.Substring(index, m.Index - index);
                    foreach (var t in rest(prefix))
                    {
                        yield return t;
                    }
                }

                yield return build(m);

                index = m.Index + m.Length;
            }

            if (index < text.Length)
            {
                var suffix = text.Substring(index, text.Length - index);
                foreach (var t in rest(suffix))
                {
                    yield return t;
                }
            }
        }

        private static readonly Regex Eoln = new Regex("\\s+");
        private static readonly Regex Lbrk = new Regex(@"\ {2,}\n");

        public static IEnumerable<Inline> DoText(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var lines = Lbrk.Split(text);
            var first = true;
            foreach (var line in lines)
            {
                if (first)
                    first = false;
                else
                    yield return new LineBreak();
                var t = Eoln.Replace(line, " ");
                yield return new Run(t);
            }
        }
    }



    class Strikethrough : Span
    {
        public Strikethrough()
        {
            TextDecorations = System.Windows.TextDecorations.Strikethrough;
        }
    }
}
