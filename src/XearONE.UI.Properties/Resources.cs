using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace XearONE.UI.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("XearONE.UI.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static Icon Icon => (Icon)ResourceManager.GetObject("Icon", resourceCulture);

	public static string IconPath => ResourceManager.GetString("IconPath", resourceCulture);

	public static string Img_Main_Guide01 => ResourceManager.GetString("Img_Main_Guide01", resourceCulture);

	public static string Img_Main_Guide02 => ResourceManager.GetString("Img_Main_Guide02", resourceCulture);

	public static string Img_Main_Guide03 => ResourceManager.GetString("Img_Main_Guide03", resourceCulture);

	internal Resources()
	{
	}
}
