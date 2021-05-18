namespace Vigilance.Discord.Networking
{
    public class NetworkPlayer
    {
        public string UserId { get; set; }
        public string Nick { get; set; }
        public string IpAdress { get; set; }
        public string Token { get; set; }
        public string Team { get; set; }
        public string CurrentRoom { get; set; }
        public string NtfUnit { get; set; }
        public string GroupName { get; set; }
        public string GroupText { get; set; }
        public string GroupColor { get; set; }
        public string AuthType { get; set; }

        public NetworkRole Role { get; set; }
        public NetworkVector Position { get; set; }
        public NetworkQuaternion Rotation { get; set; }
        public NetworkItem CurrentItem { get; set; }

        public int PlayerId { get; set; }
        public int CufferId { get; set; }
        public int CameraId { get; set; }

        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Stamina { get; set; }

        public bool IsInvisible { get; set; }
        public bool IsInOverwatch { get; set; }
        public bool IsMuted { get; set; }
        public bool IsIntercomMuted { get; set; }
        public bool IsInPocketDimension { get; set; }
        public bool BypassMode { get; set; }
        public bool DoNotTrack { get; set; }
        public bool GodMode { get; set; }
        public bool HasRaAccess { get; set; }

        public object[] GetParameters() => new object[]
        {
            BypassMode,
            AuthType,
            CameraId,
            CufferId,
            CurrentItem,
            CurrentRoom,
            DoNotTrack,
            GodMode,
            GroupColor,
            GroupName,
            GroupText,
            HasRaAccess,
            Health,
            IpAdress,
            IsInOverwatch,
            IsInPocketDimension,
            IsIntercomMuted,
            IsMuted,
            MaxHealth,
            Nick,
            NtfUnit,
            PlayerId,
            Position,
            Role,
            Rotation,
            Stamina,
            Team,
            UserId
        };

        public void Broadcast(string message, int duration = 10, bool monospaced = false) => NetworkPlayerAction.Broadcast(this, message, duration, monospaced);
        public void Hint(string message, int duration = 10) => NetworkPlayerAction.Hint(this, message, duration);
        public void RaMessage(string message) => NetworkPlayerAction.RaMessage(this, message);
        public void ConsoleMessage(string message, string color = "green") => NetworkPlayerAction.ConsoleMessage(this, message, color);
        public void SyncValuesTo(NetworkPlayer target) => NetworkPlayerAction.SyncValuesTo(this, target);
        public void SyncInventory() => NetworkPlayerAction.SyncInventory(this, this);
        public void SyncPosition() => NetworkPlayerAction.SyncPosition(this, this);
        public void SyncRotation() => NetworkPlayerAction.SyncRotation(this, this);
        public void SyncHealth() => NetworkPlayerAction.SyncHealth(this, this);
        public void SyncGroup() => NetworkPlayerAction.SyncGroup(this, this);
        public void SyncItem() => NetworkPlayerAction.SyncItem(this, this);

        public static NetworkPlayer FromPlayer(object[] p) => SyncValuesFrom(new NetworkPlayer(), p);


        public static NetworkPlayer SyncValuesFrom(NetworkPlayer p, object[] par)
        {
            if (par.Length < 35)
                return p;

            p.BypassMode = (bool)par[0];
            p.AuthType = par[1].ToString();
            p.CameraId = (int)par[2];
            p.CufferId = (int)par[3];
            p.CurrentItem = (NetworkItem)(int)par[4];
            p.CurrentRoom = par[5].ToString();
            p.DoNotTrack = (bool)par[6];
            p.GodMode = (bool)par[7];
            p.GroupColor = par[8].ToString();
            p.GroupName = par[9].ToString();
            p.GroupText = par[10].ToString();
            p.HasRaAccess = (bool)par[11];
            p.Health = (int)par[12];
            p.IpAdress = par[13].ToString();
            p.IsInOverwatch = (bool)par[14];
            p.IsInPocketDimension = (bool)par[15];
            p.IsIntercomMuted = (bool)par[16];
            p.IsInvisible = (bool)par[17];
            p.IsMuted = (bool)par[18];
            p.MaxHealth = (float)par[19];
            p.Nick = par[20].ToString();
            p.NtfUnit = par[21].ToString();
            p.PlayerId = (int)par[22];
            p.Position = NetworkVector.FromVector((float)par[23], (float)par[25], (float)par[26]);
            p.Role = (NetworkRole)(int)par[27];
            p.Rotation = NetworkQuaternion.FromQuaternion((float)par[28], (float)par[29], (float)par[30], (float)par[31]);
            p.Stamina = (float)par[32];
            p.Team = par[33].ToString();
            p.UserId = par[34].ToString();

            return p;
        }
    }
}
