// Copyright (c) Microsoft Corporation
// All rights reserved

namespace ClipsLanguage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.VisualStudio.Language.StandardClassification;

    [Export(typeof(ITaggerProvider))]
    [ContentType("clips")]
    [TagType(typeof(ClipsTokenTag))]
    internal sealed class ClipsTokenTagProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new ClipsTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class ClipsTokenTag : ITag 
    {
        public string type { get; private set; }

        public ClipsTokenTag(string type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format("Tag: {0}", type);
        }
    }

    internal sealed class ClipsTokenTagger : ITagger<ClipsTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, string> _ClipsTypes;

        internal ClipsTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _ClipsTypes = new Dictionary<string, string>
            {
                { "(", PredefinedClassificationTypeNames.Operator },
                { ")", PredefinedClassificationTypeNames.Operator },
                { "&", PredefinedClassificationTypeNames.Operator },
                { "|", PredefinedClassificationTypeNames.Operator },
                { "~", PredefinedClassificationTypeNames.Operator },
                { ":", PredefinedClassificationTypeNames.Operator },
                { "=", PredefinedClassificationTypeNames.Operator },
                { "<-", PredefinedClassificationTypeNames.Operator },
                { "=>", PredefinedClassificationTypeNames.Operator },

                { "defclass", PredefinedClassificationTypeNames.Keyword },
                { "defrule", PredefinedClassificationTypeNames.Keyword },
                { "deftemplate", PredefinedClassificationTypeNames.Keyword },

                { "slot", PredefinedClassificationTypeNames.Keyword },
                { "multislot", PredefinedClassificationTypeNames.Keyword },
                { "role", PredefinedClassificationTypeNames.Keyword },
                { "pattern-match", PredefinedClassificationTypeNames.Keyword },

                { "object", PredefinedClassificationTypeNames.Keyword },
                { "is-a", PredefinedClassificationTypeNames.Keyword },
                { "name", PredefinedClassificationTypeNames.Keyword },
            };
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        internal IEnumerable<string> Tokenize(string text)
        {
            var position = 0;
            var length = text.Length;
            while (position < length)
            {
                var start = position;
                var c = text[position];

                if ("()&|~".IndexOf(text[position]) != -1)
                {
                    ++position;
                }
                else if (text[position] == '"')
                {
                    ++position;
                    while (position < length)
                    {
                        if (text[position] == '"')
                            break;
                        if (text[position] == '\\')
                        {
                            ++position;
                            if (position < length)
                                ++position;
                        }
                        else
                        {
                            ++position;
                        }
                    }
                }
                else if (char.IsWhiteSpace(text[position]))
                {
                    ++position;
                    while (position < length)
                    {
                        if (!char.IsWhiteSpace(text[position]))
                            break;
                        ++position;
                    }
                }
                else if (text[position] == ';')
                {
                    ++position;
                    while (position < length)
                        ++position;
                }
                else if (!char.IsWhiteSpace(text[position]))
                {
                    ++position;
                    while (position < length)
                    {
                        if (char.IsWhiteSpace(text[position]) ||
                            "()&|<~".IndexOf(text[position]) != -1)
                            break;
                        ++position;
                    }
                }
                yield return text.Substring(start, position - start);
            }
        }

        public IEnumerable<ITagSpan<ClipsTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //Debug.WriteLine("GetTags");
            foreach (SnapshotSpan curSpan in spans)
            {
                //Debug.WriteLine("curSpan = {0}", curSpan);
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                var lineStart = containingLine.Start.Position;
                var position = lineStart;
                var line = containingLine.GetText();

                foreach (var token in Tokenize(line))
                {
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot,
                        new Span(position, token.Length));
                    if (tokenSpan.IntersectsWith(curSpan))
                    {
                        var type = (string)null;
                        if (_ClipsTypes.ContainsKey(token))
                            type = _ClipsTypes[token];
                        else if (char.IsWhiteSpace(token[0]))
                            type = PredefinedClassificationTypeNames.WhiteSpace;
                        else if (char.IsWhiteSpace(token[0]))
                            type = PredefinedClassificationTypeNames.WhiteSpace;
                        else if (token[0] == ';')
                            type = PredefinedClassificationTypeNames.Comment;
                        else if (token[0] == '"')
                            type = PredefinedClassificationTypeNames.String;
                        else if (token[0] == '?' || token[0] == '$' && token[1] == '?')
                            type = PredefinedClassificationTypeNames.Identifier;
                        else
                            type = PredefinedClassificationTypeNames.SymbolReference;
                        if (type != null)
                        {
                            var tag = new ClipsTokenTag(type);
                            var span = new TagSpan<ClipsTokenTag>(tokenSpan, tag);
                            Debug.WriteLine("{0}", tag);
                            yield return span;
                        }
                    }
                    position += token.Length;
                }
            }
        }
    }
}
