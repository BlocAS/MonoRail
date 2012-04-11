//   Copyright 2011 Microsoft Corporation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace System.Data.OData.Query
{
    #region Namespaces.
    using System;
    using System.Data.OData;
    using System.Diagnostics;
    using System.Text;
    #endregion Namespaces.

    /// <summary>Use this class to parse an expression in the OData URI format.</summary>
    /// <remarks>
    /// Literals (non-normative "handy" reference - see spec for correct expression):
    /// Null        null
    /// Boolean     true | false
    /// Int32       (digit+)
    /// Int64       (digit+)(L|l)
    /// Decimal     (digit+ ['.' digit+])(M|m)
    /// Float       (digit+ ['.' digit+][e|E [+|-] digit+)(f|F)
    /// Double      (digit+ ['.' digit+][e|E [+|-] digit+)
    /// String      "'" .* "'"
    /// DateTime    datetime"'"dddd-dd-dd[T|' ']dd:mm[ss[.fffffff]]"'"
    /// Binary      (binary|X)'digit*'
    /// GUID        guid'digit*'
    /// </remarks>
    [DebuggerDisplay("ExpressionLexer ({text} @ {textPos} [{token}]")]
    internal class ExpressionLexer
    {
        #region Private fields.

        /// <summary>Suffix for single literals.</summary>
        private const char SingleSuffixLower = 'f';

        /// <summary>Suffix for single literals.</summary>
        private const char SingleSuffixUpper = 'F';

        /// <summary>Text being parsed.</summary>
        private readonly string text;

        /// <summary>Length of text being parsed.</summary>
        private readonly int textLen;

        /// <summary>Position on text being parsed.</summary>
        private int textPos;

        /// <summary>Character being processed.</summary>
        private char ch;

        /// <summary>Token being processed.</summary>
        private ExpressionToken token;

        #endregion Private fields.

        #region Constructors.

        /// <summary>Initializes a new <see cref="ExpressionLexer"/>.</summary>
        /// <param name="expression">Expression to parse.</param>
        internal ExpressionLexer(string expression)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(expression != null, "expression != null");

            this.text = expression;
            this.textLen = this.text.Length;
            this.SetTextPos(0);
            this.NextToken();
        }

        #endregion Constructors.

        #region Internal properties.

        /// <summary>Token being processed.</summary>
        internal ExpressionToken CurrentToken
        {
            get 
            {
                DebugUtils.CheckNoExternalCallers();
                return this.token; 
            }

            set 
            {
                DebugUtils.CheckNoExternalCallers();
                this.token = value; 
            }
        }

        /// <summary>Text being parsed.</summary>
        internal string ExpressionText
        {
            get 
            {
                DebugUtils.CheckNoExternalCallers();
                return this.text; 
            }
        }

        /// <summary>Position on text being parsed.</summary>
        internal int Position
        {
            get 
            {
                DebugUtils.CheckNoExternalCallers();
                return this.token.Position; 
            }
        }

        #endregion Internal properties.

        #region Internal methods.

        /// <summary>Whether the specified token identifier is a numeric literal.</summary>
        /// <param name="id">Token to check.</param>
        /// <returns>true if it's a numeric literal; false otherwise.</returns>
        internal static bool IsNumeric(ExpressionTokenKind id)
        {
            DebugUtils.CheckNoExternalCallers();

            return 
                id == ExpressionTokenKind.IntegerLiteral || id == ExpressionTokenKind.DecimalLiteral ||
                id == ExpressionTokenKind.DoubleLiteral || id == ExpressionTokenKind.Int64Literal ||
                id == ExpressionTokenKind.SingleLiteral;
        }

        /// <summary>Creates an exception for a parse error.</summary>
        /// <param name="message">Message text.</param>
        /// <returns>A new Exception.</returns>
        internal static Exception ParseError(string message)
        {
            DebugUtils.CheckNoExternalCallers();

            return new ODataException(message);
        }

        /// <summary>Reads the next token, skipping whitespace as necessary.</summary>
        /// <returns>The next token.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This parser method is all about the switch statement and would be harder to maintain if it were broken up.")]
        internal ExpressionToken NextToken()
        {
            DebugUtils.CheckNoExternalCallers();

            while (Char.IsWhiteSpace(this.ch))
            {
                this.NextChar();
            }

            ExpressionTokenKind t;
            int tokenPos = this.textPos;
            switch (this.ch)
            {
                case '(':
                    this.NextChar();
                    t = ExpressionTokenKind.OpenParen;
                    break;
                case ')':
                    this.NextChar();
                    t = ExpressionTokenKind.CloseParen;
                    break;
                case ',':
                    this.NextChar();
                    t = ExpressionTokenKind.Comma;
                    break;
                case '-':
                    bool hasNext = this.textPos + 1 < this.textLen;
                    if (hasNext && Char.IsDigit(this.text[this.textPos + 1]))
                    {
                        this.NextChar();
                        t = this.ParseFromDigit();
                        if (IsNumeric(t))
                        {
                            break;
                        }
                        
                        // If it looked like a numeric but wasn't (because it was a binary 0x... value for example), 
                        // we'll rewind and fall through to a simple '-' token.
                        this.SetTextPos(tokenPos);
                    }
                    else if (hasNext && this.text[tokenPos + 1] == ExpressionConstants.InfinityLiteral[0])
                    {
                        this.NextChar();
                        this.ParseIdentifier();
                        string currentIdentifier = this.text.Substring(tokenPos + 1, this.textPos - tokenPos - 1);

                        if (IsInfinityLiteralDouble(currentIdentifier))
                        {
                            t = ExpressionTokenKind.DoubleLiteral;
                            break;
                        }
                        else if (IsInfinityLiteralSingle(currentIdentifier))
                        {
                            t = ExpressionTokenKind.SingleLiteral;
                            break;
                        }

                        // If it looked like '-INF' but wasn't we'll rewind and fall through to a simple '-' token.
                        this.SetTextPos(tokenPos);
                    }

                    this.NextChar();
                    t = ExpressionTokenKind.Minus;
                    break;
                case '=':
                    this.NextChar();
                    t = ExpressionTokenKind.Equal;
                    break;
                case '/':
                    this.NextChar();
                    t = ExpressionTokenKind.Slash;
                    break;
                case '?':
                    this.NextChar();
                    t = ExpressionTokenKind.Question;
                    break;
                case '.':
                    this.NextChar();
                    t = ExpressionTokenKind.Dot;
                    break;
                case '\'':
                    char quote = this.ch;
                    do
                    {
                        this.NextChar();
                        while (this.textPos < this.textLen && this.ch != quote)
                        {
                            this.NextChar();
                        }

                        if (this.textPos == this.textLen)
                        {
                            throw ParseError(Strings.ExpressionLexer_UnterminatedStringLiteral(this.textPos, this.text));
                        }

                        this.NextChar();
                    } 
                    while (this.ch == quote);
                    t = ExpressionTokenKind.StringLiteral;
                    break;
                case '*':
                    this.NextChar();
                    t = ExpressionTokenKind.Star;
                    break;
                default:
                    if (Char.IsLetter(this.ch) || this.ch == '_')
                    {
                        this.ParseIdentifier();
                        t = ExpressionTokenKind.Identifier;
                        break;
                    }

                    if (Char.IsDigit(this.ch))
                    {
                        t = this.ParseFromDigit();
                        break;
                    }
                    
                    if (this.textPos == this.textLen)
                    {
                        t = ExpressionTokenKind.End;
                        break;
                    }

                    throw ParseError(Strings.ExpressionLexer_InvalidCharacter(this.ch, this.textPos, this.text));
            }

            this.token.Kind = t;
            this.token.Text = this.text.Substring(tokenPos, this.textPos - tokenPos);
            this.token.Position = tokenPos;

            // Handle type-prefixed literals such as binary, datetime or guid.
            this.HandleTypePrefixedLiterals();

            // Handle keywords.
            if (this.token.Kind == ExpressionTokenKind.Identifier)
            {
                if (IsInfinityOrNaNDouble(this.token.Text))
                {
                    this.token.Kind = ExpressionTokenKind.DoubleLiteral;
                }
                else if (IsInfinityOrNanSingle(this.token.Text))
                {
                    this.token.Kind = ExpressionTokenKind.SingleLiteral;
                }
                else if (this.token.Text == ExpressionConstants.KeywordTrue || this.token.Text == ExpressionConstants.KeywordFalse)
                {
                    this.token.Kind = ExpressionTokenKind.BooleanLiteral;
                }
                else if (this.token.Text == ExpressionConstants.KeywordNull)
                {
                    this.token.Kind = ExpressionTokenKind.NullLiteral;
                }
            }

            return this.token;
        }

        /// <summary>
        /// Starting from an identifier, reads a sequence of dots and 
        /// identifiers, and returns the text for it, with whitespace 
        /// stripped.
        /// </summary>
        /// <returns>The dotted identifier starting at the current identifier.</returns>
        internal string ReadDottedIdentifier()
        {
            DebugUtils.CheckNoExternalCallers();

            this.ValidateToken(ExpressionTokenKind.Identifier);
            StringBuilder builder = null;
            string result = this.CurrentToken.Text;
            this.NextToken();
            while (this.CurrentToken.Kind == ExpressionTokenKind.Dot)
            {
                this.NextToken();
                this.ValidateToken(ExpressionTokenKind.Identifier);
                if (builder == null)
                {
                    builder = new StringBuilder(result, result.Length + 1 + this.CurrentToken.Text.Length);
                }

                builder.Append('.');
                builder.Append(this.CurrentToken.Text);                
                this.NextToken();
            }

            if (builder != null)
            {
                result = builder.ToString();
            }

            return result;
        }

        /// <summary>Returns the next token without advancing the lexer.</summary>
        /// <returns>The next token.</returns>
        internal ExpressionToken PeekNextToken()
        {
            DebugUtils.CheckNoExternalCallers();

            int savedTextPos = this.textPos;
            char savedChar = this.ch;
            ExpressionToken savedToken = this.token;

            this.NextToken();
            ExpressionToken result = this.token;

            this.textPos = savedTextPos;
            this.ch = savedChar;
            this.token = savedToken;

            return result;
        }

        /// <summary>Validates the current token is of the specified kind.</summary>
        /// <param name="t">Expected token kind.</param>
        internal void ValidateToken(ExpressionTokenKind t)
        {
            DebugUtils.CheckNoExternalCallers();

            if (this.token.Kind != t)
            {
                throw ParseError(Strings.ExpressionLexer_SyntaxError(this.textPos, this.text));
            }
        }

        #endregion Internal methods.

        #region Private methods.

        /// <summary>Checks if the <paramref name="tokenText"/> is INF or NaN.</summary>
        /// <param name="tokenText">Input token.</param>
        /// <returns>true if match found, false otherwise.</returns>
        private static bool IsInfinityOrNaNDouble(string tokenText)
        {
            Debug.Assert(tokenText != null, "tokenText != null");

            if (tokenText.Length == 3)
            {
                if (tokenText[0] == ExpressionConstants.InfinityLiteral[0])
                {
                    return IsInfinityLiteralDouble(tokenText);
                }
                else
                if (tokenText[0] == ExpressionConstants.NaNLiteral[0])
                {
                    return String.CompareOrdinal(tokenText, 0, ExpressionConstants.NaNLiteral, 0, 3) == 0;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks whether <paramref name="text"/> equals to 'INF'
        /// </summary>
        /// <param name="text">Text to look in.</param>
        /// <returns>true if the substring is equal using an ordinal comparison; false otherwise.</returns>
        private static bool IsInfinityLiteralDouble(string text)
        {
            Debug.Assert(text != null, "text != null");

            return String.CompareOrdinal(text, 0, ExpressionConstants.InfinityLiteral, 0, text.Length) == 0;
        }

        /// <summary>Checks if the <paramref name="tokenText"/> is INFf/INFF or NaNf/NaNF.</summary>
        /// <param name="tokenText">Input token.</param>
        /// <returns>true if match found, false otherwise.</returns>
        private static bool IsInfinityOrNanSingle(string tokenText)
        {
            Debug.Assert(tokenText != null, "tokenText != null");

            if (tokenText.Length == 4)
            {
                if (tokenText[0] == ExpressionConstants.InfinityLiteral[0])
                {
                    return IsInfinityLiteralSingle(tokenText);
                }
                else if (tokenText[0] == ExpressionConstants.NaNLiteral[0])
                {
                    return (tokenText[3] == ExpressionLexer.SingleSuffixLower || tokenText[3] == ExpressionLexer.SingleSuffixUpper) && 
                            String.CompareOrdinal(tokenText, 0, ExpressionConstants.NaNLiteral, 0, 3) == 0;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks whether <paramref name="text"/> EQUALS to 'INFf' or 'INFF'.
        /// </summary>
        /// <param name="text">Text to look in.</param>
        /// <returns>true if the substring is equal using an ordinal comparison; false otherwise.</returns>
        private static bool IsInfinityLiteralSingle(string text)
        {
            Debug.Assert(text != null, "text != null");
            return text.Length == 4 &&
                   (text[3] == ExpressionLexer.SingleSuffixLower || text[3] == ExpressionLexer.SingleSuffixUpper) &&
                   String.CompareOrdinal(text, 0, ExpressionConstants.InfinityLiteral, 0, 3) == 0;
        }

        /// <summary>Handles lexemes that are formed by an identifier followed by a quoted string.</summary>
        /// <remarks>This method modified the token field as necessary.</remarks>
        private void HandleTypePrefixedLiterals()
        {
            ExpressionTokenKind id = this.token.Kind;
            if (id != ExpressionTokenKind.Identifier)
            {
                return;
            }

            bool quoteFollows = this.ch == '\'';
            if (!quoteFollows)
            {
                return;
            }

            string tokenText = this.token.Text;
            if (String.Equals(tokenText, ExpressionConstants.LiteralPrefixDateTime, StringComparison.OrdinalIgnoreCase))
            {
                id = ExpressionTokenKind.DateTimeLiteral;
            }
            else if (String.Equals(tokenText, ExpressionConstants.LiteralPrefixGuid, StringComparison.OrdinalIgnoreCase))
            {
                id = ExpressionTokenKind.GuidLiteral;
            }
            else if (String.Equals(tokenText, ExpressionConstants.LiteralPrefixBinary, StringComparison.OrdinalIgnoreCase) || 
                String.Equals(tokenText, ExpressionConstants.LiteralPrefixShortBinary, StringComparison.OrdinalIgnoreCase))
            {
                id = ExpressionTokenKind.BinaryLiteral;
            }
            else
            {
                return;
            }

            int tokenPos = this.token.Position;
            do
            {
                this.NextChar();
            }
            while (this.ch != '\0' && this.ch != '\'');

            if (this.ch == '\0')
            {
                throw ParseError(Strings.ExpressionLexer_UnterminatedLiteral(this.textPos, this.text));
            }

            this.NextChar();
            this.token.Kind = id;
            this.token.Text = this.text.Substring(tokenPos, this.textPos - tokenPos);
        }

        /// <summary>Advanced to the next character.</summary>
        private void NextChar()
        {
            if (this.textPos < this.textLen)
            {
                this.textPos++;
            }

            this.ch = this.textPos < this.textLen ? this.text[this.textPos] : '\0';
        }

        /// <summary>Parses a token that starts with a digit.</summary>
        /// <returns>The kind of token recognized.</returns>
        private ExpressionTokenKind ParseFromDigit()
        {
            Debug.Assert(Char.IsDigit(this.ch), "Char.IsDigit(this.ch)");
            ExpressionTokenKind result;
            char startChar = this.ch;
            this.NextChar();
            if (startChar == '0' && this.ch == 'x' || this.ch == 'X')
            {
                result = ExpressionTokenKind.BinaryLiteral;
                do
                {
                    this.NextChar();
                }
                while (UriPrimitiveTypeParser.IsCharHexDigit(this.ch));
            }
            else
            {
                result = ExpressionTokenKind.IntegerLiteral;
                while (Char.IsDigit(this.ch))
                {
                    this.NextChar();
                }

                if (this.ch == '.')
                {
                    result = ExpressionTokenKind.DoubleLiteral;
                    this.NextChar();
                    this.ValidateDigit();

                    do
                    {
                        this.NextChar();
                    }
                    while (Char.IsDigit(this.ch));
                }

                if (this.ch == 'E' || this.ch == 'e')
                {
                    result = ExpressionTokenKind.DoubleLiteral;
                    this.NextChar();
                    if (this.ch == '+' || this.ch == '-')
                    {
                        this.NextChar();
                    }

                    this.ValidateDigit();
                    do
                    {
                        this.NextChar();
                    }
                    while (Char.IsDigit(this.ch));
                }

                if (this.ch == 'M' || this.ch == 'm')
                {
                    result = ExpressionTokenKind.DecimalLiteral;
                    this.NextChar();
                }
                else
                if (this.ch == 'd' || this.ch == 'D')
                {
                    result = ExpressionTokenKind.DoubleLiteral;
                    this.NextChar();
                }
                else if (this.ch == 'L' || this.ch == 'l')
                {
                    result = ExpressionTokenKind.Int64Literal;
                    this.NextChar();
                }
                else if (this.ch == 'f' || this.ch == 'F')
                {
                    result = ExpressionTokenKind.SingleLiteral;
                    this.NextChar();
                }
            }

            return result;
        }

        /// <summary>Parses an identifier by advancing the current character.</summary>
        private void ParseIdentifier()
        {
            Debug.Assert(Char.IsLetter(this.ch) || this.ch == '_', "Char.IsLetter(this.ch) || this.ch == '_'");
            do
            {
                this.NextChar();
            }
            while (Char.IsLetterOrDigit(this.ch) || this.ch == '_');
        }

        /// <summary>Sets the text position.</summary>
        /// <param name="pos">New text position.</param>
        private void SetTextPos(int pos)
        {
            this.textPos = pos;
            this.ch = this.textPos < this.textLen ? this.text[this.textPos] : '\0';
        }

        /// <summary>Validates the current character is a digit.</summary>
        private void ValidateDigit()
        {
            if (!Char.IsDigit(this.ch))
            {
                throw ParseError(Strings.ExpressionLexer_DigitExpected(this.textPos, this.text));
            }
        }

        #endregion Private methods.
    }
}
