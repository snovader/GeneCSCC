using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Prototype.Extensions
{
    internal static class SyntaxNodeExtensions
    {
        public static string ExtendedContext(this List<List<SyntaxNode>> nodes,
            SemanticModel semanticModel, int start, int end)
        {
            var extendedContext = string.Empty;
            for (var i = start < 0 ? 0 : start; i < end; i++)
            {
                extendedContext += "";
            }

            return extendedContext.Equals(string.Empty) ? extendedContext : extendedContext.TrimEnd(' ');
        }

        public static string NodeType(this SyntaxNode node, SemanticModel semanticModel)
        {
            if (!(node is ArgumentSyntax)
                && !(node is InvocationExpressionSyntax)
                && !(node is MemberAccessExpressionSyntax)
                && !(node is ObjectCreationExpressionSyntax))
            {
                return string.Empty;
            }

            if (node is ArgumentSyntax)
            {
                node = ((ArgumentSyntax)node).Expression;
            }
            else if (node is InvocationExpressionSyntax)
            {
                node = ((InvocationExpressionSyntax)node).Expression;
            }

            var invocation = semanticModel.GetSymbolInfo(node).Symbol;

            return invocation == null || invocation is IErrorTypeSymbol ? string.Empty : invocation.ContainingSymbol.ToString();
        }

        public static string AssignmentType(this SyntaxNode node, SemanticModel semanticModel)
        {
            var context = string.Empty;
            if (!(node is MemberAccessExpressionSyntax) && !(node is ObjectCreationExpressionSyntax))
            {
                return context;
            }

            var lhsType = semanticModel.GetTypeInfo(node).Type;
            if (lhsType != null && !(lhsType is IErrorTypeSymbol))
            {
                context = $"{lhsType.OriginalDefinition}";
            }

            return context;
        }

        public static string NodeSymbol(this SyntaxNode node, SemanticModel semanticModel)
        {
            var context = string.Empty;
            if (!(node is MemberAccessExpressionSyntax) && !(node is ObjectCreationExpressionSyntax))
            {
                return context;
            }

            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol != null)
            {
                context = $"{symbol.OriginalDefinition}";
            }

            return context;
        }
    }
}
