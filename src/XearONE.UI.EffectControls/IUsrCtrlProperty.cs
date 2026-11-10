using System.Collections.Generic;

namespace XearONE.UI.EffectControls;

public interface IUsrCtrlProperty
{
	List<string> propertyNames { get; set; }

	void RefreshAllControls();

	void InitPropertyNames();
}
