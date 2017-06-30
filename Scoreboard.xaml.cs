using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TurboJamHelper
{
	/// <summary>
	/// Interaction logic for Scoreboard.xaml
	/// </summary>
	public partial class Scoreboard : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public Scoreboard()
		{
			InitializeComponent();
		}

		public void Init(EventData data)
		{
			MainItemsControl.ItemsSource = data.Data;
			TopLevelGrid.DataContext = data;
		}

		public void Update()
		{
			OnPropertyChanged("Data");
			OnPropertyChanged("PlayerState1");
		}
	}
}
