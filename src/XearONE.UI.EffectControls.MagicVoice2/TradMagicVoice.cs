using System;
using Cmedia;

namespace XearONE.UI.EffectControls.MagicVoice2;

internal class TradMagicVoice : IMagicVoice
{
	private Guid AudioEndpointUID = Guid.Empty;

	private const string propertyName_XearMagicVoice_Enable = "Enable_MAGICVOICE";

	private const string propertyName_XearMagicVoice_Mode = "MagicVoice_Selection";

	private int lastErrorCode;

	private string lastErrorString = "";

	public int GetLastErrorCode()
	{
		return lastErrorCode;
	}

	public string GetLastErrorString()
	{
		return lastErrorString;
	}

	public TradMagicVoice(Guid audioEndpointUID)
	{
		AudioEndpointUID = audioEndpointUID;
	}

	public bool IsAvailable()
	{
		IsEnabled();
		if (lastErrorCode == 0)
		{
			return true;
		}
		return false;
	}

	public int SetEnable(bool isEnable)
	{
		int enable = (isEnable ? 1 : 0);
		XearSDK.SetProperty(AudioEndpointUID, "Enable_MAGICVOICE", enable, out var cmErr);
		return (cmErr == XearSDK.CmErr.CE_OK) ? 1 : 0;
	}

	public bool IsEnabled()
	{
		int enable = 0;
		if (XearSDK.GetProperty(AudioEndpointUID, "Enable_MAGICVOICE", out enable, out XearSDK.CmErr cmErr))
		{
			return enable == 1;
		}
		lastErrorCode = (int)cmErr;
		lastErrorString = "Unable to get Magic Voice Enable property.";
		return false;
	}

	public int SetTarget(MagicVoice2Target target)
	{
		XearSDK.SetProperty(AudioEndpointUID, "MagicVoice_Selection", (int)target, out var cmErr);
		return (int)cmErr;
	}

	public MagicVoice2Target GetTarget()
	{
		if (XearSDK.GetProperty(AudioEndpointUID, "MagicVoice_Selection", out int mode, out XearSDK.CmErr cmErr))
		{
			return mode switch
			{
				0 => MagicVoice2Target.Monster, 
				1 => MagicVoice2Target.Cartoon, 
				2 => MagicVoice2Target.Man, 
				3 => MagicVoice2Target.Woman, 
				_ => MagicVoice2Target.None, 
			};
		}
		lastErrorCode = (int)cmErr;
		lastErrorString = "Unable to get Magic Voice Selection property.";
		return MagicVoice2Target.None;
	}
}
