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
        public TokenTypes type { get; private set; }

        public ClipsTokenTag(TokenTypes type)
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
        IDictionary<string, TokenTypes> _ClipsTypes;

        internal ClipsTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _ClipsTypes = new Dictionary<string, TokenTypes>
            {
                { "(", TokenTypes.Operator },
                { ")", TokenTypes.Operator },
                { "&", TokenTypes.Operator },
                { "|", TokenTypes.Operator },
                { "~", TokenTypes.Operator },
                { ":", TokenTypes.Operator },
                { "=", TokenTypes.Operator },
                { "<-", TokenTypes.Operator },
                { "=>", TokenTypes.Operator },

                { "defclass", TokenTypes.Keyword },
                { "defrule", TokenTypes.Keyword },
                { "deftemplate", TokenTypes.Keyword },

                { "slot", TokenTypes.Keyword },
                { "multislot", TokenTypes.Keyword },
                { "role", TokenTypes.Keyword },
                { "pattern-match", TokenTypes.Keyword },

                { "object", TokenTypes.Keyword },
                { "is-a", TokenTypes.Keyword },
                { "name", TokenTypes.Keyword },
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
                        var type = TokenTypes.None;
                        if (_ClipsTypes.ContainsKey(token))
                            type = _ClipsTypes[token];
                        else if (char.IsWhiteSpace(token[0]))
                            type = TokenTypes.Whitespace;
                        else if (char.IsWhiteSpace(token[0]))
                            type = TokenTypes.Whitespace;
                        else if (token[0] == '?' || token[0] == '$' && token[1] == '?')
                            type = TokenTypes.Variable;
                        else if (token[0] == ';')
                            type = TokenTypes.Comment;
                        else if (token[0] == '"')
                            type = TokenTypes.String;
                        if (type != TokenTypes.None)
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
