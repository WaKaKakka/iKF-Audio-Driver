namespace XearONE.UI.EffectControls.MagicVoice2;

internal interface IMagicVoice
{
	int GetLastErrorCode();

	string GetLastErrorString();

	bool IsAvailable();

	int SetEnable(bool enable);

	bool IsEnabled();

	int SetTarget(MagicVoice2Target index);

	MagicVoice2Target GetTarget();
}
