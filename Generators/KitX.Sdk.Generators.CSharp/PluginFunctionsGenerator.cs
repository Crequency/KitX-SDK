using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KitX.Contract.CSharp.Attributes;
using KitX.Shared.CSharp.Plugin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace KitX.Sdk.Generators.CSharp;

[Generator]
public class PluginFunctionsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {

    }

    //public Dictionary<string, string> GetTranslations(string field, IEnumerable<TranslationAttribute> translations)
    //{
    //    var result = new Dictionary<string, string>();

    //    foreach (var item in translations.Where(t => t.Field.Equals(field)))
    //        result.Add(item.Language, item.Value);

    //    return result;
    //}

    public void Execute(GeneratorExecutionContext context)
    {
        var entryAttrType = typeof(EntryClassAttribute);
        var funcAttrType = typeof(FunctionAttribute);

        //var functions = context.Compilation.Assembly.GetAttributes()
        //    .Where(x => x.AttributeClass?.GetType().Equals(entryAttrType) ?? false)
        //    .SelectMany(x => x.GetType().GetMethods())
        //    .Where(f => f.CustomAttributes.Any(
        //        x => x.AttributeType.Equals(funcAttrType)
        //    ))
        //    ;

        //var result = functions.Select(func =>
        //{
        //    var parameters = func.GetParameters().Select(param =>
        //    {
        //        var paramAttr = param.GetCustomAttribute<ParameterAttribute>()!;

        //        return new Parameter
        //        {
        //            Name = paramAttr.Name,
        //            DisplayNames = GetTranslations("DisplayName", param.GetCustomAttributes<TranslationAttribute>()),
        //            Type = param.ParameterType.Name,
        //            IsOptional = param.IsOptional,
        //        };
        //    });

        //    var funcAttr = func.GetCustomAttribute<FunctionAttribute>()!;

        //    return new Function
        //    {
        //        Name = funcAttr.Name,
        //        DisplayNames = GetTranslations("DisplayName", func.GetCustomAttributes<TranslationAttribute>()),
        //        ReturnValueType = func.ReturnType.Name,
        //        Parameters = parameters.ToList(),
        //    };
        //});

        //var sourceText = JsonConvert.SerializeObject(result);

        //context.AddSource("PluginFunctions.json", SourceText.From(sourceText, Encoding.UTF8));

        context.AddSource("Test.json", SourceText.From("", Encoding.UTF8));

        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "KSDK0001",
                    "Plugin Info Generated",
                    "PluginFunctions.json generated successfully.",
                    "Plugin",
                    DiagnosticSeverity.Info,
                    true
                ),
            Location.None)
        );

        context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "KSDK0000",
                    "Your location",
                    $"You are at {Path.GetFullPath(".")}",
                    "Plugin",
                    DiagnosticSeverity.Warning,
                    true
                ),
            Location.None)
        );
    }
}
