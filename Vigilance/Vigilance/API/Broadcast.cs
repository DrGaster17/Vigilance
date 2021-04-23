using System.Collections.Generic;
using System.Linq;

namespace Vigilance.API
{
    public class Broadcast
    {
        public int Duration { get; set; }
        public string Message { get; set; }
        public bool Monospaced { get; set; }

        public Broadcast(int duration, string message, bool monoSpaced = false)
        {
            Duration = duration;
            Message = message;
            Monospaced = monoSpaced;
        }

        public void Show() => Map.Broadcast(this);
        public void ShowHint() => Map.ShowHint(this);
        public void Show(Player player) => player.Broadcast(this);
        public void ShowHint(Player player) => player.ShowHint(this);
        public void Show(IEnumerable<Player> players) => players.ToList().ForEach((x) => x.Broadcast(this));
        public void ShowHint(IEnumerable<Player> players) => players.ToList().ForEach((x) => x.ShowHint(this));
    }
}
