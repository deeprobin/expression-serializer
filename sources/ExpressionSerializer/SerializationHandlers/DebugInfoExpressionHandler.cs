using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class DebugInfoExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.DebugInfo;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var fileName = stream.Read32BitPrefixed();
        var language = stream.ReadGuid();
        var languageVendor = stream.ReadGuid();
        var documentType = stream.ReadGuid();
        var document = Expression.SymbolDocument(fileName.ToString(), language, languageVendor, documentType);

        var startLine = stream.ReadInt32();
        var startColumn = stream.ReadInt32();

        var endLine = stream.ReadInt32();
        var endColumn = stream.ReadInt32();

        return Expression.DebugInfo(document, startLine, startColumn, endLine, endColumn);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is DebugInfoExpression);
        var debugInfoExpression = (DebugInfoExpression)expression;

        stream.Write32BitPrefixed(debugInfoExpression.Document.FileName);
        stream.Write(debugInfoExpression.Document.Language);
        stream.Write(debugInfoExpression.Document.LanguageVendor);
        stream.Write(debugInfoExpression.Document.DocumentType);

        stream.Write(debugInfoExpression.StartLine);
        stream.Write(debugInfoExpression.StartColumn);

        stream.Write(debugInfoExpression.EndLine);
        stream.Write(debugInfoExpression.EndColumn);
    }
}