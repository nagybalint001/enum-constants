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
        private const string attributeSource = @"using System;

namespace EnumConstants
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AutoConstantsAttribute : Attribute
    {
        public AutoConstantsAttribute(Type enumType, string valuePrefix = null)
        {
            // intentionally empty, only used for code generator
        }
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

            AddNamespaceBeginning(sb, classSymbol);
            var identationLevel = AddClassBeginnings(sb, classSymbol);

            AddConstantsDefinitions(sb, classSymbol, attributeSymbol, identationLevel + 1);

            AddClassClosings(sb, identationLevel);
            AddNamespaceClosing(sb);
            return sb.ToString();
        }

        private void AddNamespaceBeginning(StringBuilder sb, ITypeSymbol classSymbol)
        {
            sb.AppendLine($@"namespace {classSymbol.ContainingNamespace}");
            sb.AppendLine($@"{{");
        }

        private void AddNamespaceClosing(StringBuilder sb)
        {
            sb.AppendLine($@"}}");
        }

        private int AddClassBeginnings(StringBuilder sb, ITypeSymbol classSymbol)
        {
            if (classSymbol == null)
                return 0;

            var identationLevel = AddClassBeginnings(sb, classSymbol.ContainingType) + 1;

            sb.Append(new string(' ', identationLevel * 4));
            sb.AppendLine($@"public partial class {classSymbol.Name}");
            sb.Append(new string(' ', identationLevel * 4));
            sb.AppendLine($@"{{");

            return identationLevel;
        }

        private void AddClassClosings(StringBuilder sb, int identationLevel)
        {
            for (int i = identationLevel; i > 0; i--)
            {
                sb.Append(new string(' ', i * 4));
                sb.AppendLine($@"}}");
            }
        }

        private void AddConstantsDefinitions(StringBuilder sb, ITypeSymbol classSymbol, INamedTypeSymbol attributeSymbol, int identationLevel)
        {
            var attributeData = classSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

            var enumType = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            var valuePrefix = attributeData.ConstructorArguments[1].Value as string;

            foreach (var enumValue in enumType.GetMembers().Where(m => !m.IsImplicitlyDeclared))
            {
                sb.Append(new string(' ', identationLevel * 4));
                sb.AppendLine($@"public const string {enumValue.Name} = ""{valuePrefix}{enumValue.Name}"";");
            }
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
