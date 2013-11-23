using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ClipsLanguage
{
    
    [Export(typeof(IQuickInfoSourceProvider))]
    [ContentType("Clips")]
    [Name("ClipsQuickInfo")]
    class ClipsQuickInfoSourceProvider : IQuickInfoSourceProvider
    {

        [Import]
        IBufferTagAggregatorFactoryService aggService = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new ClipsQuickInfoSource(textBuffer, aggService.CreateTagAggregator<ClipsTokenTag>(textBuffer));
        }
    }

    class ClipsQuickInfoSource : IQuickInfoSource
    {
        private ITagAggregator<ClipsTokenTag> _aggregator;
        private ITextBuffer _buffer;
        private bool _disposed = false;


        public ClipsQuickInfoSource(ITextBuffer buffer, ITagAggregator<ClipsTokenTag> aggregator)
        {
            _aggregator = aggregator;
            _buffer = buffer;
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            if (_disposed)
                throw new ObjectDisposedException("TestQuickInfoSource");

            var triggerPoint = (SnapshotPoint) session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (triggerPoint == null)
                return;

            foreach (IMappingTagSpan<ClipsTokenTag> curTag in _aggregator.GetTags(new SnapshotSpan(triggerPoint, triggerPoint)))
            {
#if false
                if (curTag.Tag.type == ClipsTokenTypes.ClipsExclaimation)
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    quickInfoContent.Add("Exclaimed Clips");
                }
#endif
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}

