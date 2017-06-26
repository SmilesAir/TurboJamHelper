using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace TurboJamHelper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		static EventData data = new EventData();
		public static EventData StaticData
		{
			get { return data; }
		}
		public ObservableCollection<TeamData> Data
		{
			get { return data.Data; }
			set
			{
				data.Data = value;
				OnPropertyChanged("Data");
			}
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		int currentScore = 0;
		public int CurrentScore
		{
			get { return currentScore; }
			set
			{
				currentScore = value;
				OnPropertyChanged("CurrentScore");
			}
		}

		int playingTeamIndex = 0;
		bool bFirstPlayer = true;

		Scoreboard scoreboard = new Scoreboard();

		public MainWindow()
		{
			InitializeComponent();

			TopLevelGrid.DataContext = this;
		}

		private void NextPlayer()
		{
			if (playingTeamIndex + 1 >= Data.Count)
			{
				playingTeamIndex = 0;
				bFirstPlayer = !bFirstPlayer;
			}
			else
			{
				++playingTeamIndex;
			}

			UpdateDataState();
		}

		private void UpdateDataState()
		{
			for (int i = 0; i < Data.Count; ++i)
			{
				TeamData td = Data[i];
				if (i == playingTeamIndex)
				{
					td.TeamState = bFirstPlayer ? ETeamState.PlayerTurn1 : ETeamState.PlayerTurn2;
				}
				else
				{
					td.TeamState = ETeamState.None;
				}
			}
		}

		private void AddTeam_Click(object sender, RoutedEventArgs e)
		{
			TeamData newTeam = new TeamData();
			newTeam.PlayerName1 = NewPlayer1.Text;
			newTeam.PlayerName2 = NewPlayer2.Text;

			Data.Add(newTeam);
		}

		private void RemoveTeam_Click(object sender, RoutedEventArgs e)
		{
			Button senderButton = sender as Button;
			string playerName1 = senderButton.Tag as string;

			for (int i = 0; i < Data.Count; ++i)
			{
				if (Data[i].PlayerName1 == playerName1)
				{
					Data.RemoveAt(i);
					break;
				}
			}
		}

		private void NextPlayer_Click(object sender, RoutedEventArgs e)
		{
			NextPlayer();
		}

		private void ClearScore_Click(object sender, RoutedEventArgs e)
		{
			CurrentScore = 0;
		}

		private void AddCurrentPlayerScore()
		{
			TeamData td = data.Data[playingTeamIndex];
			ObservableCollection<int> scores = bFirstPlayer ? td.Scores1 : td.Scores2;

			scores.Add(currentScore);
			
			data.TopTeamScore = Math.Max(data.TopTeamScore, td.TeamScore);

			foreach (TeamData updateTd in data.Data)
			{
				int delta = data.TopTeamScore - updateTd.TeamScore;

				updateTd.ScoreToTakeLead1 = delta > 0 ? delta + updateTd.HighestScore1 + 1: 0;
				updateTd.ScoreToTakeLead2 = delta > 0 ? delta + updateTd.HighestScore2 + 1: 0;

				updateTd.Update();
			}
		}

		private void ConfirmScore_Click(object sender, RoutedEventArgs e)
		{
			AddCurrentPlayerScore();

			CurrentScore = 0;

			NextPlayer();

			Save();
		}

		private void NumberButton_Click(object sender, RoutedEventArgs e)
		{
			Button senderButton = sender as Button;
			int number = int.Parse(senderButton.Tag as string);

			CurrentScore = CurrentScore * 10 + number;

			CurrentScore = Math.Min(50, currentScore);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(EventData));

				using (StreamReader reader = new StreamReader("Save.xml"))
				{
					data = serializer.Deserialize(reader) as EventData;
				}

				OnPropertyChanged("Data");
			}
			catch { }

			UpdateDataState();

			scoreboard.Init(data);

			scoreboard.Show();

			scoreboard.Update();
		}

		private void Save()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(EventData));

			using (StreamWriter writer = new StreamWriter("Save.xml"))
			{
				serializer.Serialize(writer, data);
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			scoreboard.Close();
		}

		private void MoveTeamUp_Click(object sender, RoutedEventArgs e)
		{
			Button senderButton = sender as Button;
			string playerName1 = senderButton.Tag as string;

			for (int i = 1; i < Data.Count; ++i)
			{
				if (Data[i].PlayerName1 == playerName1)
				{
					Data.Insert(i - 1, Data[i]);
					Data.RemoveAt(i + 1);
					return;
				}
			}
		}

		private void MoveTeamDown_Click(object sender, RoutedEventArgs e)
		{
			Button senderButton = sender as Button;
			string playerName1 = senderButton.Tag as string;

			for (int i = 0; i < Data.Count - 1; ++i)
			{
				if (Data[i].PlayerName1 == playerName1)
				{
					Data.Insert(i + 2, Data[i]);
					Data.RemoveAt(i);
					return;
				}
			}
		}
	}

	public class TeamData : INotifyPropertyChanged
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

		[XmlIgnore]
		Timer pulsePlayerStateColorTimer = new Timer();
		string playerName1 = "";
		public string PlayerName1
		{
			get { return playerName1; }
			set
			{
				playerName1 = value;
				OnPropertyChanged("PlayerName1");
			}
		}
		string playerName2 = "";
		public string PlayerName2
		{
			get { return playerName2; }
			set
			{
				playerName2 = value;
				OnPropertyChanged("PlayerName2");
			}
		}
		public ObservableCollection<int> Scores1= new ObservableCollection<int>();
		public int HighestScore1
		{
			get
			{
				int ret = 0;
				foreach (int x in Scores1)
				{
					ret = Math.Max(ret, x);
				}

				return ret;
			}
		}

		public ObservableCollection<int> Scores2 = new ObservableCollection<int>();
		public int HighestScore2
		{
			get
			{
				int ret = 0;
				foreach (int x in Scores2)
				{
					ret = Math.Max(ret, x);
				}

				return ret;
			}
		}

		public int TeamScore
		{
			get
			{
				return HighestScore1 + HighestScore2;
			}
		}

		ETeamState teamState = ETeamState.None;
		[XmlIgnore]
		public ETeamState TeamState
		{
			get { return teamState; }
			set
			{
				ResetPlayerStateColor();

				if (teamState != value)
				{
					if (value != ETeamState.None)
					{
						pulsePlayerStateColorTimer.Stop();
						pulsePlayerStateColorTimer.Start();
					}
					else
					{
						pulsePlayerStateColorTimer.Stop();
					}
				}

				teamState = value;
				OnPropertyChanged("TeamState");
				OnPropertyChanged("PlayerState1");
				OnPropertyChanged("PlayerState2");
			}
		}

		public string PlayerState1
		{
			get { return teamState == ETeamState.PlayerTurn1 ? "->" : ""; }
		}

		public string PlayerState2
		{
			get { return teamState == ETeamState.PlayerTurn2 ? "->" : ""; }
		}

		[XmlIgnore]
		Brush playerStateColor1 = Brushes.Black;
		[XmlIgnore]
		public Brush PlayerStateColor1
		{
			get
			{
				return playerStateColor1;
			}
			set
			{
				playerStateColor1 = value;
				OnPropertyChanged("PlayerStateColor1");
			}
		}
		[XmlIgnore]
		Brush playerStateColor2 = Brushes.Black;
		[XmlIgnore]
		public Brush PlayerStateColor2
		{
			get
			{
				return playerStateColor2;
			}
			set
			{
				playerStateColor2 = value;
				OnPropertyChanged("PlayerStateColor2");
			}
		}

		[XmlIgnore]
		public Brush ScoreboardColor
		{
			get { return teamState != ETeamState.None ? Brushes.DarkSeaGreen : Brushes.AliceBlue; }
		}

		public int ScoreboardOrderNumber
		{
			get
			{
				int ret = 1;
				foreach (TeamData td in MainWindow.StaticData.Data)
				{
					if (td == this)
					{
						return ret;
					}

					++ret;
				}

				return 0;
			}
		}

		public int ScoreboardRankNumber
		{
			get
			{
				int ret = 1;
				foreach (TeamData td in MainWindow.StaticData.Data)
				{
					if (td != this && td.TeamScore > TeamScore)
					{
						++ret;
					}
				}

				return ret;
			}
		}

		int scoreToTakeLead1 = 0;
		[XmlIgnore]
		public int ScoreToTakeLead1
		{
			get { return scoreToTakeLead1; }
			set
			{
				scoreToTakeLead1 = value;
				OnPropertyChanged("ScoreToTakeLead1");
			}
		}

		int scoreToTakeLead2 = 0;
		[XmlIgnore]
		public int ScoreToTakeLead2
		{
			get { return scoreToTakeLead2; }
			set
			{
				scoreToTakeLead2 = value;
				OnPropertyChanged("ScoreToTakeLead2");
			}
		}

		public string ScoreToTakeLeadString
		{
			get { return ScoreToTakeLead1.ToString() + " | " + ScoreToTakeLead2.ToString(); }
		}

		public TeamData()
		{
			pulsePlayerStateColorTimer.Elapsed += PulsePlayerStateColorTimer_Elapsed;
			pulsePlayerStateColorTimer.Interval = 700;
			pulsePlayerStateColorTimer.AutoReset = true;
		}

		public void Update()
		{
			OnPropertyChanged("HighestScore1");
			OnPropertyChanged("HighestScore2");
			OnPropertyChanged("TeamScore");
			OnPropertyChanged("ScoreToTakeLeadString");
			OnPropertyChanged("ScoreboardRankNumber");
		}

		private void ResetPlayerStateColor()
		{
			PlayerStateColor1 = Brushes.Black;
			PlayerStateColor2 = Brushes.Black;
		}

		private void PulsePlayerStateColorTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (teamState == ETeamState.PlayerTurn1)
			{
				PlayerStateColor1 = PlayerStateColor1 == Brushes.Black ? Brushes.Tomato : Brushes.Black;
			}
			else if (teamState == ETeamState.PlayerTurn2)
			{
				PlayerStateColor2 = PlayerStateColor2 == Brushes.Black ? Brushes.Tomato : Brushes.Black;
			}
			else
			{
				ResetPlayerStateColor();
			}
		}
	}

	public enum ETeamState
	{
		None,
		PlayerTurn1,
		PlayerTurn2
	}

	public class EventData
	{
		public ObservableCollection<TeamData> Data = new ObservableCollection<TeamData>();
		public int TopTeamScore = 0;
	}
}
