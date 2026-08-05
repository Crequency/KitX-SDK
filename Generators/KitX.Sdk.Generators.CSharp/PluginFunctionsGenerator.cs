using System;
using Microsoft.CodeAnalysis;

namespace KitX.Sdk.Generators.CSharp;

/// <summary>
/// 已弃用。该生成器原计划从编译上下文中读取插件函数清单并生成 PluginFunctions.json，
/// 但当前所有消费方（加载器、客户端）均不再读取该文件，插件函数信息改由插件自身在
/// PluginInfo.Functions 中提供。该生成器已从所有项目移除 Analyzer 引用，保留此类型
/// 仅为避免破坏既有二进制引用。
/// </summary>
[Obsolete("KitX.Sdk.Generators.CSharp is no longer used; plugin functions are provided by PluginInfo.Functions.")]
[Generator]
public class PluginFunctionsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Intentionally empty: this generator is obsolete and no longer emits source files.
    }
}
