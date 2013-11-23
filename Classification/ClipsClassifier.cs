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

    [Export(typeof(ITaggerProvider))]
    [ContentType("Clips")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class ClipsClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("Clips")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition ClipsContentType = null;

        [Export]
        [FileExtension(".clp")]
        [ContentType("Clips")]
        internal static FileExtensionToContentTypeDefinition ClipsFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<ClipsTokenTag> ClipsTagAggregator = 
                                            aggregatorFactory.CreateTagAggregator<ClipsTokenTag>(buffer);

            return new ClipsClassifier(buffer, ClipsTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class ClipsClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<ClipsTokenTag> _aggregator;
        IDictionary<ClipsTokenTypes, IClassificationType> _ClipsTypes;

        internal ClipsClassifier(ITextBuffer buffer, 
                               ITagAggregator<ClipsTokenTag> ClipsTagAggregator, 
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = ClipsTagAggregator;
            _ClipsTypes = new Dictionary<ClipsTokenTypes, IClassificationType>();
            _ClipsTypes[ClipsTokenTypes.ClipsComment] = typeService.GetClassificationType("ClipsComment");
            _ClipsTypes[ClipsTokenTypes.ClipsKeyword] = typeService.GetClassificationType("ClipsKeyword");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return 
                    new TagSpan<ClassificationTag>(tagSpans[0], 
                        new ClassificationTag(_ClipsTypes[tagSpan.Tag.type]));
            }
        }
    }
}
