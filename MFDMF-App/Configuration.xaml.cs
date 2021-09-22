namespace MFDMFApp
{
	using MFDMF_Models.Models;
	using Newtonsoft.Json;
	using System;
	using System.IO;
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for Configuration.xaml
	/// </summary>
	public partial class Configuration : Window
	{
		private readonly AppSettings _settings;
		private AppSettings _originalSettings;
		private bool _isLoading = true;
		private bool _isDataDirty = false;

		public Configuration(AppSettings settings)
		{
			InitializeComponent();
			_settings = settings;
			_originalSettings = new AppSettings(settings);
			UpdateButtonState();
			this.Closing += Configuration_Closing;
		}

		private void Configuration_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(_isDataDirty)
			{
				e.Cancel = true;
				var msgResult = MessageBox.Show("There are unsaved changes, do you want to save the changes and exit? If you want to save your changes then select Yes. To discard changes and exit select No and select Cancel to continue editing.", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				switch (msgResult)
				{
					case MessageBoxResult.Cancel:
					case MessageBoxResult.None:
						break;
					case MessageBoxResult.OK:
					case MessageBoxResult.Yes:
						SaveChanges();
						e.Cancel = false;
						break;
					case MessageBoxResult.No:
						LoadValues();
						e.Cancel = false;
						break;
				}
			}
		}

		private void CheckBox_Click(object sender, RoutedEventArgs e)
		{
			var chkBox = sender as CheckBox;
			var cmdParamnter = chkBox.CommandParameter.ToString();
			var property = _settings.GetType().GetProperty(cmdParamnter);
			if(property != null)
			{
				property.SetValue(_settings, chkBox.IsChecked);
			}
			UpdateButtonState();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LoadValues();
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			SaveChanges();
		}

		private void TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_isLoading)
			{
				var txtBox = sender as TextBox;
				if(txtBox != null)
				{
					var propertyName = txtBox.Tag?.ToString();
					if(!string.IsNullOrEmpty(propertyName))
					{
						var property = _settings.GetType().GetProperty(propertyName);
						property.SetValue(_settings, txtBox.Text);
					}
				}
				UpdateButtonState();
			}
		}

		private void LoadValues()
		{
			_isLoading = true;
			chkUseRulers.IsChecked = _settings.ShowRulers ?? false;
			chkShowTooltips.IsChecked = _settings.ShowTooltips ?? false;
			chkSaveCroppedImages.IsChecked = _settings.SaveCroppedImages ?? false;
			chkTurnOffCache.IsChecked = _settings.TurnOffCache ?? false;
			chkCreateKneeboard.IsChecked = _settings.CreateKneeboard ?? false;
			txtDisplayConfigurationFile.Text = _settings.DisplayConfigurationFile;
			txtFilePath.Text = _settings.FilePath;
			txtFileSpec.Text = _settings.FileSpec;
			txtDcsSavedGamesPath.Text = _settings.DcsSavedGamesPath;
			txtDefaultConfiguration.Text = _settings.DefaultConfiguration;
			_isLoading = false;
			UpdateButtonState();
		}

		private void SaveChanges()
		{
			var contents = JsonConvert.SerializeObject(_settings);
			var currentDir = Environment.CurrentDirectory;
			var appSettings = Path.Combine(currentDir, "appsettings.json");
			File.WriteAllText(appSettings, contents);
			_originalSettings = new AppSettings(_settings);
			UpdateButtonState();
		}
		private void UpdateButtonState()
		{
			_isDataDirty = !_settings.Equals(_originalSettings);
			btnSave.IsEnabled = _isDataDirty;
		}
	}
}
