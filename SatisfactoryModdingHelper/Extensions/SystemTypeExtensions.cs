using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Markup;

namespace SatisfactoryModdingHelper.Extensions;
public class SystemTypeExtension : MarkupExtension
{
    private object parameter;

    public int Int
    {
        set => parameter = value;
    }
    public double Double
    {
        set => parameter = value;
    }
    public float Float
    {
        set => parameter = value;
    }
    public bool Bool
    {
        set => parameter = value;
    }
    // add more as needed here

}
