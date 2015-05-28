using Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using XamlBrewer.Uni.Pull2Refresh.Model;

namespace XamlBrewer.Uni.Pull2Refresh.ViewModels
{
    class MainViewModel : BindableBase
    {
        private ObservableCollection<Episode> episodes = new ObservableCollection<Episode>();
        private RelayCommand refreshCommand;

        public MainViewModel()
        {
            refreshCommand = new RelayCommand(Refresh_Executed);
        }

        public ObservableCollection<Episode> Episodes
        {
            get { return episodes; }
            set { episodes = value; }
        }

        public ICommand RefreshCommand
        { get { return refreshCommand; } }

        private void Refresh_Executed()
        {
            foreach (var item in Episode.FetchNextTwo(episodes.Count))
            {
                episodes.Insert(0, item);
            }
        }
    }
}
