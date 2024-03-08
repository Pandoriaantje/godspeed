using System;
using Neurotoxin.Godspeed.Presentation.Bindings;
using Neurotoxin.Godspeed.Presentation.Converters;
using Neurotoxin.Godspeed.Shell.Constants;
using Neurotoxin.Godspeed.Shell.ViewModels;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Converters
{
    public class SizeConverter : MultiBindingConverterBase<FileSystemItemViewModel, string, SizeConverterParameter>
    {
        public SizeConverter() : base(new [] { "Size", "IsRefreshing", "Type", "TitleType" })
        {
        }

        protected override string Convert(FileSystemItemViewModel viewModel, Type targetType, SizeConverterParameter parameter)
        {
            if (viewModel == null) return null;

            switch (parameter)
            {
                case SizeConverterParameter.Plain:
                    return SizeFormat(viewModel.Size);
                case SizeConverterParameter.ListView:
                case SizeConverterParameter.ContentView:
                    if (viewModel.IsRefreshing) return "?";

                    //size is set
                    if (viewModel.Size != null) return SizeFormat(viewModel.Size);

                    string resxName;
                    if (viewModel.TitleType != TitleType.Unknown)
                    {
                        resxName = viewModel.TitleType.GetType().Name + viewModel.TitleType;
                        if (parameter == SizeConverterParameter.ContentView) resxName += "Long";
                    }
                    else
                        resxName = viewModel.Type.GetType().Name + viewModel.Type;

                    var rm = Resx.ResourceManager;
                    return parameter == SizeConverterParameter.ListView
                               ? string.Format("<{0}>", rm.GetString(resxName))
                               : rm.GetString(resxName);
                default:
                    throw new NotSupportedException("Invalid parameter value: " + parameter);
            }
        }

        private string SizeFormat(long? size)
        {
            if (size.HasValue) {
                var si = ((string, string))new FileSizeConverter().Convert(size.Value, null, null, null);
                return String.Format("{0} {1}", si.Item1, Resx.ResourceManager.GetString(si.Item2));
            }
            return null;
        }

    }
}