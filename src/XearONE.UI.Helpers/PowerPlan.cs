using System;

namespace XearONE.UI.Helpers;

public class PowerPlan
{
	public readonly string name;

	public Guid guid;

	public PowerPlan(string name, Guid guid)
	{
		this.name = name;
		this.guid = guid;
	}
}
