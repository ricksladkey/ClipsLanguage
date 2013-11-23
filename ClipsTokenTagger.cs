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
                // Operators.
                { "(", PredefinedClassificationTypeNames.Operator },
                { ")", PredefinedClassificationTypeNames.Operator },
                { "&", PredefinedClassificationTypeNames.Operator },
                { "|", PredefinedClassificationTypeNames.Operator },
                { "~", PredefinedClassificationTypeNames.Operator },
                { ":", PredefinedClassificationTypeNames.Operator },

                // Literals.
                { "TRUE", PredefinedClassificationTypeNames.Literal },
                { "FALSE", PredefinedClassificationTypeNames.Literal },
                { "crlf", PredefinedClassificationTypeNames.Literal },
                { "t", PredefinedClassificationTypeNames.Literal },

                // Language keywords.
                { "if", PredefinedClassificationTypeNames.Keyword },
                { "then", PredefinedClassificationTypeNames.Keyword },
                { "else", PredefinedClassificationTypeNames.Keyword },
                { "loop-for-count", PredefinedClassificationTypeNames.Keyword },
                { "while", PredefinedClassificationTypeNames.Keyword },
                { "return", PredefinedClassificationTypeNames.Keyword },
                { "slot", PredefinedClassificationTypeNames.Keyword },
                { "multislot", PredefinedClassificationTypeNames.Keyword },
                { "role", PredefinedClassificationTypeNames.Keyword },
                { "pattern-match", PredefinedClassificationTypeNames.Keyword },
                { "bind", PredefinedClassificationTypeNames.Keyword },

                // Common language support methods.
                { "assert", PredefinedClassificationTypeNames.Keyword },
                { "retract", PredefinedClassificationTypeNames.Keyword },
                { "focus", PredefinedClassificationTypeNames.Keyword },
                { "batch", PredefinedClassificationTypeNames.Keyword },
                { "run", PredefinedClassificationTypeNames.Keyword },
                { "reset", PredefinedClassificationTypeNames.Keyword },
                { "make-instance", PredefinedClassificationTypeNames.Keyword },
                { "set-strategy", PredefinedClassificationTypeNames.Keyword },

                // Conditional elements.
                { "test", PredefinedClassificationTypeNames.Keyword },
                { "and", PredefinedClassificationTypeNames.Keyword },
                { "or", PredefinedClassificationTypeNames.Keyword },
                { "not", PredefinedClassificationTypeNames.Keyword },
                { "declare", PredefinedClassificationTypeNames.Keyword },
                { "logical", PredefinedClassificationTypeNames.Keyword },
                { "object", PredefinedClassificationTypeNames.Keyword },
                { "exists", PredefinedClassificationTypeNames.Keyword },
                { "forall", PredefinedClassificationTypeNames.Keyword },

                // Operator-like keywords.
                { "=", PredefinedClassificationTypeNames.Keyword },
                { "<-", PredefinedClassificationTypeNames.Keyword },
                { "=>", PredefinedClassificationTypeNames.Keyword },

                // Definition keywords.
                { "defgeneric", PredefinedClassificationTypeNames.Keyword },
                { "defmethod", PredefinedClassificationTypeNames.Keyword },
                { "defglobal", PredefinedClassificationTypeNames.Keyword },
                { "defmodule", PredefinedClassificationTypeNames.Keyword },
                { "defclass", PredefinedClassificationTypeNames.Keyword },
                { "defrule", PredefinedClassificationTypeNames.Keyword },
                { "deftemplate", PredefinedClassificationTypeNames.Keyword },
                { "deffunction", PredefinedClassificationTypeNames.Keyword },
                { "defmessage-handler", PredefinedClassificationTypeNames.Keyword },
                { "definstances", PredefinedClassificationTypeNames.Keyword },
                { "deffacts", PredefinedClassificationTypeNames.Keyword },
                { "defmodules", PredefinedClassificationTypeNames.Keyword },

                // Attribute constraints.
                { "is-a", PredefinedClassificationTypeNames.Keyword },
                { "name", PredefinedClassificationTypeNames.Keyword },

                // Common built-in methods.
                { "eq", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "neq", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "send", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "instance-address", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "str-cat", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "format", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "open", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "close", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "exit", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "nth$", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "progn$", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "member$", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "create$", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "length$", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "printout", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "find-instance", PredefinedClassificationTypeNames.PreprocessorKeyword },
                { "find-all-instances", PredefinedClassificationTypeNames.PreprocessorKeyword },
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
                        {
                            ++position;
                            break;
                        }
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

        internal string GetTokenType(string token, string lastToken)
        {
            var char0 = token[0];
            var char1 = token.Length >= 2 ? token[1] : (char)0;
            if (_ClipsTypes.ContainsKey(token))
                return _ClipsTypes[token];
            else if (char.IsWhiteSpace(char0))
                return PredefinedClassificationTypeNames.WhiteSpace;
            else if (char0 == ';')
                return PredefinedClassificationTypeNames.Comment;
            else if (char0 == '"')
                return PredefinedClassificationTypeNames.String;
            else if (char.IsDigit(char0) || char0 == '-' && char.IsDigit(char1))
                return PredefinedClassificationTypeNames.Number;
            else if (char0 == '?' || char0 == '$' && char1 == '?')
                return PredefinedClassificationTypeNames.Identifier;
            else if (lastToken != null &&
                lastToken.StartsWith("def") &&
                _ClipsTypes.ContainsKey(lastToken) &&
                _ClipsTypes[lastToken] == PredefinedClassificationTypeNames.Keyword)
                return PredefinedClassificationTypeNames.SymbolDefinition;
            else
                return PredefinedClassificationTypeNames.SymbolReference;
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

                var lastToken = (string)null;
                foreach (var token in Tokenize(line))
                {
                    var type = GetTokenType(token, lastToken);
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot,
                        new Span(position, token.Length));
                    if (type != null && tokenSpan.IntersectsWith(curSpan))
                    {
                        var tag = new ClipsTokenTag(type);
                        var span = new TagSpan<ClipsTokenTag>(tokenSpan, tag);
                        //Debug.WriteLine("{0}", tag);
                        yield return span;
                    }
                    if (type != PredefinedClassificationTypeNames.WhiteSpace)
                        lastToken = token;
                    position += token.Length;
                }
            }
        }
    }
}
