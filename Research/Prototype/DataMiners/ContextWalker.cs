using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Prototype.Extensions;
using Prototype.Models;

namespace Prototype.DataMiners
{
    internal class ContextWalker<T> : CSharpSyntaxWalker where T : AbstractContextInfo
    {
        private readonly SemanticModel _semanticModel;
        private readonly List<List<SyntaxNode>> _nodes;
        private readonly int _linesOfContext;

        public Dictionary<string, List<T>> Contexts { get; set; }

        public ContextWalker(SemanticModel semanticModel,
            Dictionary<string, List<T>> contexts = null,
            int linesOfContext = 4)
            : base(SyntaxWalkerDepth.Token)
        {
            _semanticModel = semanticModel;
            Contexts = contexts ?? new Dictionary<string, List<T>>();
            _linesOfContext = linesOfContext;
            _nodes = new List<List<SyntaxNode>>();
        }

        /// <summary>
        /// Visits <paramref name="node"/> in depth-first order and adds it to <see cref="_nodes"/>.
        /// </summary>
        /// <param name="node">The <see cref="ObjectCreationExpressionSyntax"/> that is visited</param>
        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);

            AddNode(node);
        }

        /// <summary>
        /// Visits <paramref name="node"/> in depth-first order and adds it to <see cref="_nodes"/>.
        /// </summary>
        /// <param name="node">The <see cref="MemberAccessExpressionSyntax"/> that is visited</param>
        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            base.VisitMemberAccessExpression(node);

            AddNode(node);
        }

        /// <summary>
        /// Visits <paramref name="node"/> in depth-first order and adds it to <see cref="_nodes"/>.
        /// It is only added if it is not an instance of <see cref="MemberAccessExpressionSyntax"/>.
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode"/> that is visited</param>
        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            base.VisitInvocationExpression(node);

            if (node.Expression is MemberAccessExpressionSyntax)
            {
                return;
            }

            AddNode(node);
        }

        /// <summary>
        /// Visits <paramref name="node"/> in depth-first order and adds it to <see cref="_nodes"/>.
        /// It is only added if it is an instance of <see cref="MemberAccessExpressionSyntax"/>
        /// or <see cref="ObjectCreationExpressionSyntax"/>.
        /// </summary>
        /// <param name="node">The <see cref="ArgumentSyntax"/> that is visited</param>
        public override void VisitArgument(ArgumentSyntax node)
        {
            base.VisitArgument(node);

            if (node.Expression is MemberAccessExpressionSyntax
                || node.Expression is ObjectCreationExpressionSyntax)
            {
                AddNode(node);
            }
        }

        /// <summary>
        /// Visits <paramref name="token"/> in depth-first order.
        /// If <paramref name="token"/> does not have a <see cref="SyntaxNode"/> of
        /// type <see cref="MethodDeclarationSyntax"/>, no action is taken.
        /// </summary>
        /// <param name="token">The <see cref="SyntaxToken"/> that is visited</param>
        public override void VisitToken(SyntaxToken token)
        {
            base.VisitToken(token);

            if (!token.Parent.Ancestors().OfType<MethodDeclarationSyntax>().Any())
            {
                return;
            }

            if (token.IsKind(SyntaxKind.SemicolonToken))
            {
                _nodes.Add(new List<SyntaxNode>());
            }
            else if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                for (var i = 0; i < _nodes.Count; i++)
                {
                    var nodes = _nodes[i];
                    if (nodes.Count == 0)
                    {
                        continue;
                    }

                    var lhsType = AssignmentType(nodes[nodes.Count - 1]);
                    var context = lhsType;
                    var extendedContext = ExtendedContext(i - _linesOfContext, i);
                    foreach (var node in nodes)
                    {
                        var symbol = NodeSymbol(node);
                        if (string.IsNullOrEmpty(symbol))
                        {
                            continue;
                        }

                        if (IsPublic(node) && symbol.StartsWith("System."))
                        {
                            AddContext(NodeType(node),
                                Activator.CreateInstance(typeof (T), extendedContext, context, symbol) as T);
                        }
                        context = context.AppendWithWhitespace(symbol);
                    }
                }
                _nodes.Clear();
                /*foreach (var type in _contexts)
                {
                    Console.WriteLine(type.Key);
                    foreach (var context in type.Value)
                    {
                        Console.WriteLine(context);
                    }
                }*/
            }
        }

        private void AddNode(SyntaxNode node)
        {
            if (_nodes.Count == 0)
            {
                _nodes.Add(new List<SyntaxNode>());
            }

            _nodes[_nodes.Count - 1].Add(node);
        }

        private void AddContext(string type, T invocation)
        {
            List<T> invocations;

            if (!Contexts.TryGetValue(type, out invocations))
            {
                invocations = new List<T>();
                Contexts.Add(type, invocations);
            }

            invocations.Add(invocation);
        }

        private string ExtendedContext(int start, int end)
        {
            var extendedContext = string.Empty;
            for (var i = start < 0 ? 0 : start; i < end; i++)
            {
                var nodes = _nodes[i];
                if (nodes.Count == 0)
                {
                    continue;
                }

                extendedContext = extendedContext.AppendWithWhitespace(AssignmentType(nodes[nodes.Count - 1]));
                extendedContext = nodes.Select(NodeSymbol)
                    .Aggregate(extendedContext, (current, symbol) => current.AppendWithWhitespace(symbol));
            }

            return extendedContext.Equals(string.Empty) ? extendedContext : extendedContext.TrimEnd(' ');
        }

        private bool IsPublic(SyntaxNode node)
        {
            if (node is ArgumentSyntax)
            {
                node = ((ArgumentSyntax)node).Expression;
            }
            else if (node is InvocationExpressionSyntax)
            {
                node = ((InvocationExpressionSyntax)node).Expression;
            }

            var invocation = _semanticModel.GetSymbolInfo(node).Symbol;

            return invocation.DeclaredAccessibility == Accessibility.Public;
        }

        private string NodeType(SyntaxNode node)
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

            var invocation = _semanticModel.GetSymbolInfo(node).Symbol;

            return invocation == null || invocation is IErrorTypeSymbol ? string.Empty : invocation.ContainingSymbol.ToString();
        }

        private string AssignmentType(SyntaxNode node)
        {
            if (!(node is InvocationExpressionSyntax)
                && !(node is MemberAccessExpressionSyntax)
                && !(node is ObjectCreationExpressionSyntax))
            {
                return string.Empty;
            }

            var lhsType = _semanticModel.GetTypeInfo(node).Type;

            return lhsType == null || lhsType is IErrorTypeSymbol ? string.Empty : lhsType.ToString();
        }

        private string NodeSymbol(SyntaxNode node)
        {
            if (!(node is MemberAccessExpressionSyntax)
                && !(node is ObjectCreationExpressionSyntax)
                && !(node is ArgumentSyntax))
            {
                return string.Empty;
            }

            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            var constructor = node as ObjectCreationExpressionSyntax;
            if (constructor != null)
            {
                return symbol == null ? string.Empty : $"{constructor.NewKeyword} {symbol}";
            }

            var argument = node as ArgumentSyntax;
            if (argument != null)
            {
                symbol = _semanticModel.GetSymbolInfo(argument.Expression).Symbol;
            }

            return symbol?.ToString() ?? string.Empty;
        }
    }
}