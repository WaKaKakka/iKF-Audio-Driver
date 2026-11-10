using System;
using System.Collections.Generic;
using Cmedia;

namespace XearONE.UI.Driver.DeviceInfo;

public class JackDeviceInfo
{
	public Guid audEndptUID;

	public string displayName;

	public string controllerDisplayName;

	public XearSDK.CmDataFlow dataFlow;

	public XearSDK.CmJackType jackType;

	public string endpointID;

	public string instanceID;

	public int usbVendorID;

	public int usbProductID;

	public string usbDeviceInstanceID;

	public string driverVersion;

	public string usbRevisionNumber;

	public int channelCount;

	public string string50;

	public List<XearSDK.CmLicenseCategory> licenseCategoryList;

	public IntPtr CMediaHIDObj;

	public IntPtr CMedia108DHIDObj;

	public IntPtr CMediaLEDObj;

	public IntPtr HeadTrackingHIDObj;

	public JackDeviceInfo()
	{
		audEndptUID = Guid.Empty;
		displayName = string.Empty;
		controllerDisplayName = string.Empty;
		dataFlow = XearSDK.CmDataFlow.CDF_Unknown;
		jackType = XearSDK.CmJackType.CJT_Unknown;
		endpointID = string.Empty;
		instanceID = string.Empty;
		usbVendorID = 0;
		usbProductID = 0;
		usbDeviceInstanceID = string.Empty;
		driverVersion = string.Empty;
		usbRevisionNumber = string.Empty;
		channelCount = 0;
		string50 = string.Empty;
		licenseCategoryList = new List<XearSDK.CmLicenseCategory>();
		CMediaHIDObj = IntPtr.Zero;
		CMedia108DHIDObj = IntPtr.Zero;
		CMediaLEDObj = IntPtr.Zero;
		HeadTrackingHIDObj = IntPtr.Zero;
	}
}
