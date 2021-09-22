using MFDMF_Models.Interfaces;
using MFDMF_Models.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MFDMFApp
{
	/// <summary>
	/// Interaction logic for Modules.xaml
	/// </summary>
	public partial class Modules : Window
	{
		private AppSettings _settings;
		public Modules(AppSettings settings)
		{
			_settings = settings;
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{

			_settings.ModuleItems.GroupBy(mod => mod.Category).Select(group => $"{group.Key}").ToList()
				.ForEach(cat =>
				{
				var modules = _settings.ModuleItems.Where(mod => mod.Category == cat);
					var newCategory = new TreeViewItem()
					{
						Header = cat,
						ItemsSource = modules,
					};
					var index = treeModules.Items.Add(newCategory);
					/*
					foreach (var item in newCategory.Items)
					{
						var itemCounter = 0;
						var categoryItem = treeModules.Items[index] as TreeViewItem;
						var newItem = categoryItem.Items[itemCounter] as TreeViewItem;
						newItem.ItemsSource = ((IModuleDefinition)item).Configurations;
						itemCounter++;
					}
					*/
				});
		}

		private void treeModules_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var selectedItem = treeModules.SelectedItem;
		}
	}
}
