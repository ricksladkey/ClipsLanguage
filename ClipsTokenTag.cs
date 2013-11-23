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

        public IEnumerable<ITagSpan<ClipsTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //Debug.WriteLine("GetTags");
            foreach (SnapshotSpan curSpan in spans)
            {
                //Debug.WriteLine("curSpan = {0}", curSpan);
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int lineStart = containingLine.Start.Position;
                int position = lineStart;
                string line = containingLine.GetText();
                int commentStart = line.IndexOf(';');
                string code = line;
                if (commentStart != -1)
                    code = code.Substring(0, commentStart);
                string[] tokens = code.Split(' ');

                foreach (string ClipsToken in tokens)
                {
                    if (_ClipsTypes.ContainsKey(ClipsToken))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot,
                            new Span(position, ClipsToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan)) 
                            yield return new TagSpan<ClipsTokenTag>(tokenSpan, 
                                new ClipsTokenTag(_ClipsTypes[ClipsToken]));
                    }

                    //add an extra char location because of the space
                    position += ClipsToken.Length + 1;
                }

                if (commentStart != -1)
                {
                    int start = lineStart + commentStart;
                    int end = line.Length - commentStart;
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
