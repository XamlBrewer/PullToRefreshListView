using Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using XamlBrewer.Uni.Pull2Refresh.Model;

namespace XamlBrewer.Uni.Pull2Refresh.ViewModels
{
    class MainViewModel : BindableBase
    {
        private ObservableCollection<Episode> episodes = new ObservableCollection<Episode>();

        public MainViewModel()
        {
            // http://www.tv.com/shows/mr-magoo/episodes/
            this.episodes.Add(new Episode() { Number = 1, Title = "Magoo's bear" });
            this.episodes.Add(new Episode() { Number = 2, Title = "Military Magoo" });
            this.episodes.Add(new Episode() { Number = 3, Title = "Mis-Guided Missile" });
        }

        public ObservableCollection<Episode> Episodes
        {
            get { return episodes; }
            set { episodes = value; }
        }


    }
}
