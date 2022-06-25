namespace HoI2Editor.Parsers
{
    /// <summary>
    ///     Lexical analysis token
    /// </summary>
    public class Token
    {
        #region Public properties

        /// <summary>
        ///     Token type
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        ///     Token value
        /// </summary>
        public object Value { get; set; }

        #endregion
    }

    /// <summary>
    ///     Token type
    /// </summary>
    public enum TokenType
    {
        Invalid, // Illegal value
        Identifier, // identifier
        Number, // Numbers
        String, // Character string
        Equal, // = =
        OpenBrace, // {
        CloseBrace, // }
        WhiteSpace, // White space
        Comment // comment
    }
}
