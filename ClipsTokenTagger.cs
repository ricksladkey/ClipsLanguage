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

namespace ClipsLanguage
{
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

        public static string Operator = PredefinedClassificationTypeNames.Operator;
        public static string Literal = PredefinedClassificationTypeNames.Literal;
        public static string LanguageKeyword = PredefinedClassificationTypeNames.Keyword;
        public static string OperatorKeyword = PredefinedClassificationTypeNames.Keyword;
        public static string DefinitionKeyword = PredefinedClassificationTypeNames.Keyword;
        public static string EnvironmentCommand = PredefinedClassificationTypeNames.Keyword;
        public static string ConditionalKeyword = PredefinedClassificationTypeNames.SymbolReference;
        public static string AttributeConstraint = PredefinedClassificationTypeNames.SymbolReference;
        public static string BuiltinFunction = PredefinedClassificationTypeNames.PreprocessorKeyword;

        internal ClipsTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _ClipsTypes = new Dictionary<string, string>
            {
                // Operators.
                { "(", Operator },
                { ")", Operator },
                { "&", Operator },
                { "|", Operator },
                { "~", Operator },
                { ":", Operator },

                // Literals.
                { "TRUE", Literal },
                { "FALSE", Literal },
                { "crlf", Literal },
                { "t", Literal },
                { "nil", Literal },

                // Language keywords.
                { "if", LanguageKeyword },
                { "then", LanguageKeyword },
                { "else", LanguageKeyword },
                { "loop-for-count", LanguageKeyword },
                { "while", LanguageKeyword },
                { "return", LanguageKeyword },
                { "slot", LanguageKeyword },
                { "multislot", LanguageKeyword },
                { "role", LanguageKeyword },
                { "pattern-match", LanguageKeyword },
                { "bind", LanguageKeyword },
                { "progn", LanguageKeyword },
                { "progn$", LanguageKeyword },
                { "break", LanguageKeyword },
                { "switch", LanguageKeyword },
                { "is", LanguageKeyword },

                // Definition keywords.
                { "defgeneric", DefinitionKeyword },
                { "defmethod", DefinitionKeyword },
                { "defglobal", DefinitionKeyword },
                { "defmodule", DefinitionKeyword },
                { "defclass", DefinitionKeyword },
                { "defrule", DefinitionKeyword },
                { "deftemplate", DefinitionKeyword },
                { "deffunction", DefinitionKeyword },
                { "defmessage-handler", DefinitionKeyword },
                { "definstances", DefinitionKeyword },
                { "deffacts", DefinitionKeyword },
                { "defmodules", DefinitionKeyword },

                // Operator-like keywords.
                { "<-", OperatorKeyword },
                { "=>", OperatorKeyword },

                // Environment commands.
                { "clear", EnvironmentCommand },
                { "exit", EnvironmentCommand },
                { "facts", EnvironmentCommand },
                { "assert", EnvironmentCommand },
                { "retract", EnvironmentCommand },
                { "focus", EnvironmentCommand },
                { "batch", EnvironmentCommand },
                { "batch*", EnvironmentCommand },
                { "run", EnvironmentCommand },
                { "reset", EnvironmentCommand },
                { "make-instance", EnvironmentCommand },
                { "set-strategy", EnvironmentCommand },
                { "watch", EnvironmentCommand },

                // Conditional keywords.
                { "test", ConditionalKeyword },
                { "and", ConditionalKeyword },
                { "or", ConditionalKeyword },
                { "not", ConditionalKeyword },
                { "declare", ConditionalKeyword },
                { "logical", ConditionalKeyword },
                { "object", ConditionalKeyword },
                { "exists", ConditionalKeyword },
                { "forall", ConditionalKeyword },

                // Attribute constraints.
                { "is-a", AttributeConstraint },
                { "name", AttributeConstraint },

                // Common built-in functions.
                { "=", BuiltinFunction },
                { "eq", BuiltinFunction },
                { "neq", BuiltinFunction },
                { "send", BuiltinFunction },
                { "instance-address", BuiltinFunction },
                { "str-cat", BuiltinFunction },
                { "format", BuiltinFunction },
                { "open", BuiltinFunction },
                { "close", BuiltinFunction },
                { "nth$", BuiltinFunction },
                { "member$", BuiltinFunction },
                { "create$", BuiltinFunction },
                { "length$", BuiltinFunction },
                { "printout", BuiltinFunction },
                { "find-instance", BuiltinFunction },
                { "find-all-instances", BuiltinFunction },
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
                return PredefinedClassificationTypeNames.Other;
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
