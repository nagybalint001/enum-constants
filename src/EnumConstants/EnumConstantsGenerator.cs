using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnumConstants
{
    [Generator]
    public class EnumConstantsGenerator : ISourceGenerator
    {
        private const string attributeSource = @"
using System;

namespace EnumConstants
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AutoConstantsAttribute : Attribute
    {
        public AutoConstantsAttribute(Type enumType, string prefix = null)
        {
            EnumType = enumType;
            Prefix = prefix;
        }

        public Type EnumType { get; set; }
        public string Prefix { get; set; }
    }
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterForPostInitialization((i) => i.AddSource("AutoConstantsAttribute.g.cs", attributeSource));

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            var attributeSymbol = context.Compilation.GetTypeByMetadataName("EnumConstants.AutoConstantsAttribute");
            foreach (var @class in receiver.Classes)
            {
                var name = @class.Name;
                context.AddSource($"{name}.g.cs", SourceText.From(ProcessClass(@class, attributeSymbol), Encoding.UTF8));
            }
        }

        private string ProcessClass(ITypeSymbol classSymbol, INamedTypeSymbol attributeSymbol)
        {
            var sb = new StringBuilder();
            sb.Append($@"
namespace {classSymbol.ContainingNamespace}
{{
    public partial class {classSymbol.ContainingType.Name}
    {{
        public partial class {classSymbol.Name}
        {{");

            var attributeData = classSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

            var enumType = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;

            var prefix = attributeData.ConstructorArguments[1].Value as string;

            if (string.IsNullOrEmpty(prefix) && attributeData.NamedArguments.Any(kv => kv.Key == "Prefix"))
            {
                prefix = attributeData.NamedArguments.SingleOrDefault(kv => kv.Key == "Prefix").Value.Value as string;
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                prefix += "_";
            }

            foreach (var enumValue in enumType.GetMembers().Where(m => !m.IsImplicitlyDeclared))
            {
                sb.Append($@"
            public const string {enumValue.Name} = ""{prefix}{enumValue.Name}"";");
            }

            sb.Append($@"
        }}
    }}
}}");
            return sb.ToString();
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<ITypeSymbol> Classes { get; } = new List<ITypeSymbol>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax && typeDeclarationSyntax.AttributeLists.Count > 0)
                {
                    var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax) as ITypeSymbol;
                    if (typeSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "EnumConstants.AutoConstantsAttribute"))
                    {
                        Classes.Add(typeSymbol);
                    }
                }
            }
        }
    }
}
