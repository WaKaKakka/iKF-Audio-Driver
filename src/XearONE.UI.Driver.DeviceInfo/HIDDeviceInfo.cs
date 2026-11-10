using System;

namespace XearONE.UI.Driver.DeviceInfo;

public class HIDDeviceInfo
{
	public int vendorID;

	public int productID;

	public string usbDeviceInstanceID;

	public int revisionNumber;

	public int inputReportByteLength;

	public int outputReportByteLength;

	public int featureReportByteLength;

	public IntPtr hidEndpointObj;

	public bool isSupportCM108DHID;

	public bool isSupportXearHID;

	public bool isSupportXearLED;

	public bool isSupportHeadTrackingHID;

	public byte[] supportedHIDEffect;

	public HIDDeviceInfo()
	{
		vendorID = 0;
		productID = 0;
		usbDeviceInstanceID = string.Empty;
		revisionNumber = 0;
		inputReportByteLength = 0;
		outputReportByteLength = 0;
		featureReportByteLength = 0;
		hidEndpointObj = IntPtr.Zero;
		isSupportXearHID = false;
		isSupportXearLED = false;
		isSupportHeadTrackingHID = false;
		isSupportCM108DHID = false;
		supportedHIDEffect = new byte[0];
	}
}
