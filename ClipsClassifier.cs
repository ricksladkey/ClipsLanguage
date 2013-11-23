using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace ClipsLanguage
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("clips")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class ClipsClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("clips")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition ClipsContentType = null;

        [Export]
        [FileExtension(".clp")]
        [ContentType("clips")]
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
        internal static string[] PredefinedTypes = new string[]
        {
            PredefinedClassificationTypeNames.Character,
            PredefinedClassificationTypeNames.Comment,
            PredefinedClassificationTypeNames.ExcludedCode,
            PredefinedClassificationTypeNames.FormalLanguage,
            PredefinedClassificationTypeNames.Identifier,
            PredefinedClassificationTypeNames.Keyword,
            PredefinedClassificationTypeNames.Literal,
            PredefinedClassificationTypeNames.NaturalLanguage,
            PredefinedClassificationTypeNames.Number,
            PredefinedClassificationTypeNames.Operator,
            PredefinedClassificationTypeNames.Other,
            PredefinedClassificationTypeNames.PreprocessorKeyword,
            PredefinedClassificationTypeNames.String,
            PredefinedClassificationTypeNames.SymbolDefinition,
            PredefinedClassificationTypeNames.SymbolReference,
            PredefinedClassificationTypeNames.WhiteSpace,
        };

        ITextBuffer _buffer;
        ITagAggregator<ClipsTokenTag> _aggregator;
        IDictionary<string, IClassificationType> _ClipsTypes;

        internal ClipsClassifier(ITextBuffer buffer, 
                               ITagAggregator<ClipsTokenTag> ClipsTagAggregator, 
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = ClipsTagAggregator;
            _ClipsTypes = PredefinedTypes.Select(
                type => Tuple.Create(type, typeService.GetClassificationType(type))
            ).ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
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
