using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public class CodexItemValueTemplateContext : AvaloniaObject
    {
        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<CodexItemValueTemplateContext, object>(nameof(Value));

        public static readonly StyledProperty<CodexItemPropertyViewModel> InfoProperty =
            AvaloniaProperty.Register<CodexItemValueTemplateContext, CodexItemPropertyViewModel>(nameof(Info));

        public object Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

        public CodexItemPropertyViewModel Info { get => GetValue(InfoProperty); set => SetValue(InfoProperty, value); }

        public CodexItemValueTemplateContext() { }
    }

    public class CodexItemValueTemplateSelector : IDataTemplate
    {
        [Content]
        public List<IDataTemplate> Templates { get; } = new();

        public Control Build(object param)
        {
            var context = param as CodexItemValueTemplateContext;

            if (context is null)
            {
                if (param is CodexItemPropertyViewModel prop)
                    context = new() { Value = prop.Value, Info = prop };
                else
                    context = new() { Value = param };
            }

            var template = Templates.FirstOrDefault(x => x.Match(context.Value));
            return template?.Build(context);
        }

        public bool Match(object data)
        {
            var propValue = (data as CodexItemPropertyViewModel)?.Value ??
                            (data as CodexItemValueTemplateContext)?.Value ?? data;

            return Templates.Any(x => x.Match(propValue));
        }
    }
}
