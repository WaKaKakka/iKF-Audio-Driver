using System.Linq;
using System.Reflection;
using System.Windows;

namespace XearONE.UI;

public partial class App : Application
{
	private SingleInstanceAppHelper _singleInstance;

	protected override void OnStartup(StartupEventArgs e)
	{
		_singleInstance = new SingleInstanceAppHelper(GetAssemblyCompanyAndProduct(), "SHOW", WakeUpMainWindow);
		if (!_singleInstance.TryStart())
		{
			_singleInstance.NotifyExistingInstance();
			Shutdown();
		}
		else
		{
			base.OnStartup(e);
		}
	}

	private void WakeUpMainWindow()
	{
		Dispatcher.Invoke(() =>
		{
			MainWindow.WindowState = WindowState.Normal;
			MainWindow.Show();
			MainWindow.Activate();
		});
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_singleInstance?.Dispose();
		base.OnExit(e);
	}

	private static string GetAssemblyCompanyAndProduct()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string company = executingAssembly.GetCustomAttributes<AssemblyCompanyAttribute>().FirstOrDefault()?.Company ?? string.Empty;
		string product = executingAssembly.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? string.Empty;
		if (!string.IsNullOrEmpty(company) && !string.IsNullOrEmpty(product))
		{
			return company + "_" + product;
		}
		if (!string.IsNullOrEmpty(company))
		{
			return company;
		}
		return product;
	}
}
