namespace XearONE.UI.Driver.Settings;

public class JsonHeadTrackingConfig
{
	public int HotKey { get; set; }

	public double DetectionTime { get; set; } = 10.0;

	public double FadingTime { get; set; } = 1.0;

	public double YawShiftThreshold { get; set; } = 3.0;

	public double YawPathThreshold { get; set; } = 20.0;

	public int AutoReset { get; set; } = 1;

	public string TestSound { get; set; } = "Wav_Guitar.wav";

	public int BatteryThreshold { get; set; } = 50;

	public string FWVid { get; set; } = "0x0D8C";

	public string FWPid { get; set; } = "0x0337";
}
