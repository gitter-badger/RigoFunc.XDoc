// Copyright (c) RigoFunc (xuyingting). All rights reserved.

using System;
using System.Text;

namespace RigoFunc.XDoc {
    /// <summary>
    /// Represents the style for line indentation.
    /// </summary>
    public enum IndentStyle {
        /// <summary>
        /// Indicating the same line indentation.
        /// </summary>
        Same,
        /// <summary>
        /// Indicating the inner scope line indentation.
        /// </summary>
        Inner,
        /// <summary>
        /// Indicating the outer scope line indentation.
        /// </summary>
        Outer
    }

    /// <summary>
    /// Provides the Json style string format capability.
    /// </summary>
    public class JsonFormatter {
        StringBuilder sb;
        int indent = 4;
        int depth;
        int maxLength;
        int currentLen;

        /// <summary>
        /// Create a new instance of the <see cref="JsonFormatter"/> class.
        /// </summary>
        public JsonFormatter() {
            sb = new StringBuilder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => sb.ToString();

        /// <summary>
        /// Gets or sets the indentation.
        /// </summary>
        public int Indentation {
            get { return indent; }
            set { indent = value; }
        }

        /// <summary>
        /// Gets or sets the max property length.
        /// </summary>
        public int MaxProLength {
            set {
                maxLength = value;
            }
            get {
                return maxLength;
            }
        }

        /// <summary>
        /// Writes the specified value.
        /// </summary>
        /// <param name="value">The value</param>
        public void WriteName(string value) {
            sb.Append(value);
            sb.Append(": ");
            currentLen = value.Length;
        }

        /// <summary>
        /// Writes the specified object.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public void WriteValue(object value) {
            // left alignment.
            for (int i = 0; i < maxLength - currentLen; i++) {
                sb.Append(" ");
            }

            if (value.GetType() == typeof(string) || value.GetType() == typeof(DateTime)) {
                sb.Append("\"");
                sb.Append(value);
                sb.Append("\"");
            }
            else {
                sb.Append(value);
            }
            sb.Append(",");
        }

        /// <summary>
        /// Writes the json type open or close symbol.
        /// </summary>
        /// <param name="value"></param>
        public void WriteSymbol(string value) {
            if (value == "[" || value == "{") {
                sb.Append(value);
            }
            else {
                sb.Append(value);
                if (depth > 0) {
                    if (value == "]" || value == "}") {
                        sb.Append(",");
                    }
                }
            }
        }

        /// <summary>
        /// Write a comments.
        /// </summary>
        /// <param name="value"></param>
        public void WriteComments(string value) {
            sb.Append("// ");
            sb.Append(value);
        }

        /// <summary>
        /// Write a line with specified style.
        /// </summary>
        /// <param name="style">The indentation style.</param>
        public void WriteLine(IndentStyle style) {
            sb.AppendLine();
            Indent(style);
            for (int i = 0, n = depth * indent; i < n; i++) {
                sb.Append(" ");
            }
        }

        /// <summary>
        /// Indent by the specified style.
        /// </summary>
        /// <param name="style">The style.</param>
        protected virtual void Indent(IndentStyle style) {
            if (style == IndentStyle.Inner) {
                depth++;
            }
            else if (style == IndentStyle.Outer) {
                depth--;
                System.Diagnostics.Debug.Assert(depth >= 0);
            }
        }
    }
}
