using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace XamlBrewer.Uni.Pull2Refresh.Model
{
    public class Episode
    {
        private static List<Episode> episodes;

        static Episode()
        {
            episodes = new List<Episode>();
            // http://www.tv.com/shows/mr-magoo/episodes/
            episodes.Add(new Episode() { Number = 1, Title = "Magoo's bear" });
            episodes.Add(new Episode() { Number = 2, Title = "Military Magoo" });
            episodes.Add(new Episode() { Number = 3, Title = "Mis-guided Missile" });
            episodes.Add(new Episode() { Number = 4, Title = "Base on bawls" });
            episodes.Add(new Episode() { Number = 5, Title = "Magoo gets his man" });
            episodes.Add(new Episode() { Number = 6, Title = "Magoo's buggy" });
            episodes.Add(new Episode() { Number = 7, Title = "Martian Magoo" });
            episodes.Add(new Episode() { Number = 8, Title = "Pet sitters" });
            episodes.Add(new Episode() { Number = 9, Title = "Thin skinned driver" });
            episodes.Add(new Episode() { Number = 10, Title = "A day at the beach" });
            episodes.Add(new Episode() { Number = 11, Title = "Fish 'n tricks" });
            episodes.Add(new Episode() { Number = 12, Title = "Robinson Crusoe Magoo" });
        }
        private static List<Episode> Episodes
        {
            get { return episodes; }
        }

        public int Number { get; set; }
        public string Title { get; set; }

        public static List<Episode> FetchNextTwo(int current)
        {
            current = current % 12;

            return (from e in Episodes
                    where e.Number > current
                    orderby e.Number
                    select e).Take(2).ToList();
        }
    }
}
