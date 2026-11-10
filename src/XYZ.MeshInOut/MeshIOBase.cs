using System;
using System.Globalization;
using System.IO;
using XYZ.model.geom;

namespace XYZ.MeshInOut;

public class MeshIOBase
{
	public enum STATUS
	{
		Idle,
		Busy,
		Done,
		UserAbort
	}

	public enum COMMAND
	{
		None,
		Abort
	}

	protected NumberFormatInfo NumFormat { get; private set; }

	public STATUS Status { get; protected set; }

	public COMMAND Command { get; set; }

	public MeshIOBase()
	{
		NumFormat = CultureInfo.InvariantCulture.NumberFormat;
	}

	public virtual int Load(string filename, TopoModel model, Action<int> updateRate)
	{
		return -1;
	}

	public virtual int Load(FileStream fs, TopoModel model, Action<int> updateRate)
	{
		return -1;
	}

	public void TaskAbort(object sender, EventArgs e)
	{
		Command = COMMAND.Abort;
	}
}
