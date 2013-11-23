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
    [ContentType("Clips")]
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
        public ClipsTokenTypes type { get; private set; }

        public ClipsTokenTag(ClipsTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class ClipsTokenTagger : ITagger<ClipsTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, ClipsTokenTypes> _ClipsTypes;

        internal ClipsTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _ClipsTypes = new Dictionary<string, ClipsTokenTypes>();
            _ClipsTypes["defclass"] = ClipsTokenTypes.ClipsKeyword;
            _ClipsTypes["defrule"] = ClipsTokenTypes.ClipsKeyword;
            _ClipsTypes["deftemplate"] = ClipsTokenTypes.ClipsKeyword;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        internal IEnumerable<string> Tokenize(string text)
        {
            int position = 0;
            int length = text.Length;
            while (position < length)
            {
                var c = text[position];
                var buffer = new StringBuilder();

                if ("()&|~".IndexOf(c) != -1)
                {
                    yield return c.ToString();
                    ++position;
                    continue;
                }

                if (c == '"')
                {
                    ++position;
                    while (position < length)
                    {
                        c = text[position];
                        if (c == '"')
                            break;
                        if (c == '\\')
                        {
                            buffer.Append(c);
                            ++position;
                            if (position < length)
                            {
                                c = text[position];
                                buffer.Append(c);
                                ++position;
                            }
                        }
                        else
                        {
                            buffer.Append(c);
                            ++position;
                        }
                    }
                    yield return buffer.ToString();
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    buffer.Append(c);
                    ++position;
                    while (position < length)
                    {
                        c = text[position];
                        if (!char.IsWhiteSpace(c))
                            break;
                        buffer.Append(c);
                        ++position;
                    }
                    yield return buffer.ToString();
                    continue;

                }

                if (c == ';')
                {
                    buffer.Append(c);
                    ++position;
                    while (position < length)
                    {
                        c = text[position];
                        buffer.Append(c);
                        ++position;
                    }
                    yield return buffer.ToString();
                    continue;

                }

                if (!char.IsWhiteSpace(c))
                {
                    buffer.Append(c);
                    ++position;
                    while (position < length)
                    {
                        c = text[position];
                        if (char.IsWhiteSpace(c) || "()&|<~".IndexOf(c) != -1)
                            break;
                        buffer.Append(c);
                        ++position;
                    }
                    yield return buffer.ToString();
                    continue;
                }
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
                var commentStart = line.IndexOf(';');
                var code = line;
                if (commentStart != -1)
                    code = code.Substring(0, commentStart);
                var tokens = Tokenize(code);

                foreach (var token in Tokenize(code))
                {
                    if (_ClipsTypes.ContainsKey(token))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot,
                            new Span(position, token.Length));
                        if (tokenSpan.IntersectsWith(curSpan)) 
                            yield return new TagSpan<ClipsTokenTag>(tokenSpan, 
                                new ClipsTokenTag(_ClipsTypes[token]));
                    }
                    position += token.Length;
                }

                if (commentStart != -1)
                {
                    var start = lineStart + commentStart;
                    var end = line.Length - commentStart;
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot,
                        new Span(start, end));
                    if (tokenSpan.IntersectsWith(curSpan)) 
                        yield return new TagSpan<ClipsTokenTag>(tokenSpan,
                            new ClipsTokenTag(ClipsTokenTypes.ClipsComment));
                }
            }
        }
    }
}
