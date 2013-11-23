using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ClipsLanguage
{
    #region Format definition
    /// <summary>
    /// Defines an editor format for the ClipsComment type
    /// that has a green foreground.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ClipsComment")]
    [Name("ClipsComment")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class ClipsComment : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "ordinary" classification type
        /// </summary>
        public ClipsComment()
        {
            this.DisplayName = "ClipsComment"; //human readable version of the name
            this.ForegroundColor = Colors.DarkGreen;
        }
    }

    /// <summary>
    /// Defines an editor format for the ClipsKeyword type
    /// that has a cyan foreground.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ClipsKeyword")]
    [Name("ClipsKeyword")]
    //this should be visible to the end user
    [UserVisible(true)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class ClipsKeyword : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "ordinary" classification type
        /// </summary>
        public ClipsKeyword()
        {
            this.DisplayName = "ClipsKeyword"; //human readable version of the name
            this.ForegroundColor = Colors.DarkCyan;
        }
    }
    #endregion //Format definition
}
