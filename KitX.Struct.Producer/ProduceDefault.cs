using KitX.Web.Rules;
using System.Text.Json;

namespace KitX.Struct.Producer
{
    internal class ProduceDefault
    {
        public static void ProduceDefaultPluginStruct(string path)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                IncludeFields = true,
            };
            PluginStruct ps = new()
            {
                Name = "Plugin.Name",
                Version = "v1.0.0",
                DisplayName = new()
                {
                    { "zh-cn", "显示名称" },
                    { "en-us", "Display Name" }
                },
                AuthorName = "AuthorName",
                PublisherName = "PublisherName",
                AuthorLink = "https://blog.catrol.cn",
                PublisherLink = "https://www.catrol.cn",
                SimpleDescription = new()
                {
                    { "zh-cn", "简单描述" },
                    { "en-us", "SimpleDescription" }
                },
                ComplexDescription = new()
                {
                    { "zh-cn", "复杂描述" },
                    { "en-us", "ComplexDescription" }
                },
                TotalDescriptionInMarkdown = new()
                {
                    { "zh-cn", "Markdown 语法完整描述" },
                    { "en-us", "TotalDescriptionInMarkdown" }
                },
                IconInBase64 = "Base64 Format Icon",
                PublishDate = DateTime.Now,
                LastUpdateDate = DateTime.Now,
                IsMarketVersion = false,
                Tags = new(),
                Functions = new()
                {
                    new()
                    {
                        Name = "FunctionName",
                        DisplayNames = new()
                        {
                            { "zh-cn", "功能显示名称" },
                            { "en-us", "DisplayNames" }
                        },
                        Parameters = new()
                        {
                            {
                                "ParameterName",
                                new()
                                {
                                    { "zh-cn", "参数显示名称" },
                                    { "en-us", "Parameter Display Name" }
                                }
                            }
                        },
                        ParametersType = new()
                        {
                            "string"
                        },
                        HasAppendParameters = false,
                        ReturnValueType = "void"
                    }
                },
                RootStartupFileName = "RootStartupFileName"
            };
            File.WriteAllText(Path.GetFullPath($"{path}/PluginStruct.json"),
                JsonSerializer.Serialize(ps, options));
        }

        public static void ProduceDefaultLoaderStruct(string path)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                IncludeFields = true,
            };
            LoaderStruct ls = new()
            {
                LoaderName = "LoaderName",
                LoaderVersion = "v1.0.0",
                LoaderLanguage = "CSharp",
                LoaderFramework = "Avalonia",
                LoaderRunType = LoaderStruct.RunType.Desktop,
                SupportOS = new()
                {
                    OperatingSystems.Windows, OperatingSystems.MacOS, OperatingSystems.Linux
                }
            };
            File.WriteAllText(Path.GetFullPath($"{path}/LoaderStruct.json"),
                JsonSerializer.Serialize(ls, options));
        }
    }
}
