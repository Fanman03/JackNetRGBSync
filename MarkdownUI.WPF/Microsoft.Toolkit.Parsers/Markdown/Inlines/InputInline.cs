// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Toolkit.Parsers.Core;
using Microsoft.Toolkit.Parsers.Markdown.Helpers;

namespace Microsoft.Toolkit.Parsers.Markdown.Inlines
{
    /// <summary>
    /// Represents an embedded image.
    /// </summary>
    public class InputInline : MarkdownInline, IInlineLeaf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInline"/> class.
        /// </summary>
        public InputInline() : base(MarkdownInlineType.InputBox)
        {
        }

        /// <summary>
        /// Gets or sets the image URL.
        /// </summary>
        public string BoundTo { get; set; }

        /// <summary>
        /// Gets or sets the image Render URL.
        /// </summary>
        public string RenderUrl { get; set; }

        /// <summary>
        /// Gets or sets a text to display on hover.
        /// </summary>
        public string InputType { get; set; }

        /// <inheritdoc/>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of a reference, if this is a reference-style link.
        /// </summary>
        public string ReferenceId { get; set; }

        public Dictionary<string,string> Args { get; private set; }

        internal static void AddTripChars(List<InlineTripCharHelper> tripCharHelpers)
        {
            tripCharHelpers.Add(new InlineTripCharHelper() { FirstChar = '?', Method = InlineParseMethod.InputBox });
        }

        /// <summary>
        /// Attempts to parse an image e.g. "![Toolkit logo](https://raw.githubusercontent.com/windows-toolkit/WindowsCommunityToolkit/master/Microsoft.Toolkit.Uwp.SampleApp/Assets/ToolkitLogo.png)".
        /// </summary>
        /// <param name="markdown"> The markdown text. </param>
        /// <param name="start"> The location to start parsing. </param>
        /// <param name="end"> The location to stop parsing. </param>
        /// <returns> A parsed markdown image, or <c>null</c> if this is not a markdown image. </returns>
        internal static InlineParseResult Parse(string markdown, int start, int end)
        {
            // Expect a '!' character.
            if (start >= end || markdown[start] != '?')
            {
                return null;
            }

            int pos = start + 1;

            // Then a '[' character
            if (pos >= end || markdown[pos] != '[')
            {
                return null;
            }

            pos++;

            // Find the ']' character
            while (pos < end)
            {
                if (markdown[pos] == ']')
                {
                    break;
                }

                pos++;
            }

            if (pos == end)
            {
                return null;
            }

            // Extract the alt.
            string tooltip = markdown.Substring(start + 2, pos - (start + 2));

            // Expect the '(' character.
            pos++;

            string reference = string.Empty;
            string url = string.Empty;
            Dictionary<string, string> args = new Dictionary<string, string>();

            if (pos < end && markdown[pos] == '[')
            {
                int refstart = pos;

                // Find the reference ']' character
                while (pos < end)
                {
                    if (markdown[pos] == ']')
                    {
                        break;
                    }

                    pos++;
                }

                reference = markdown.Substring(refstart + 1, pos - refstart - 1);
            }
            else if (pos < end && markdown[pos] == '(')
            {
                while (pos < end && ParseHelpers.IsMarkdownWhiteSpace(markdown[pos]))
                {
                    pos++;
                }

                // Extract the URL.
                int urlStart = pos;
                while (pos < end && markdown[pos] != ')')
                {
                    pos++;
                }

                var imageDimensionsPos = markdown.IndexOf(" ", urlStart, pos - urlStart, StringComparison.Ordinal);

                url = imageDimensionsPos > 0
                    ? TextRunInline.ResolveEscapeSequences(markdown, urlStart + 1, imageDimensionsPos)
                    : TextRunInline.ResolveEscapeSequences(markdown, urlStart + 1, pos);

                if (imageDimensionsPos > 0)
                {
                    string remainingText = markdown.Substring(imageDimensionsPos + 1, pos - imageDimensionsPos - 1);
                    Debug.WriteLine(remainingText);

                    
                    var parts = remainingText.Split(',');
                    foreach(var part in parts)
                    {
                        var seperator = part.IndexOf('=');
                        string name = part.Substring(0,seperator);
                        string value = part.Substring(seperator + 1);

                        args.Add(name, value);
                    }
                }
            }

            if (pos == end)
            {
                return null;
            }

            // We found something!
            var result = new InputInline
            {
                InputType = !string.IsNullOrWhiteSpace(tooltip) ? tooltip : "Text",
                RenderUrl = url,
                ReferenceId = reference,
                BoundTo = url,
                Text = markdown.Substring(start, pos + 1 - start),
                Args = args
            };
            return new InlineParseResult(result, start, pos + 1);
        }

        /// <summary>
        /// If this is a reference-style link, attempts to converts it to a regular link.
        /// </summary>
        /// <param name="document"> The document containing the list of references. </param>
        internal void ResolveReference(MarkdownDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (ReferenceId == null)
            {
                return;
            }

            // Look up the reference ID.
            var reference = document.LookUpReference(ReferenceId);
            if (reference == null)
            {
                return;
            }

            // The reference was found. Check the URL is valid.
            if (!Common.IsUrlValid(reference.Url))
            {
                return;
            }

            // Everything is cool when you're part of a team.
            RenderUrl = reference.Url;
            ReferenceId = null;
        }

        /// <summary>
        /// Converts the object into it's textual representation.
        /// </summary>
        /// <returns> The textual representation of this object. </returns>
        public override string ToString()
        {
            return string.Format("![{0}]: {1}", InputType, BoundTo);
        }
    }
}