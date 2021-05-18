namespace Vigilance.API
{
    public class Version
    {
        public int Major { get; }
        public int Minor { get; }
        public int Revision { get; }

        public string Letter { get; }
        public string FullName { get; }
        public string Description { get; set; }

        public bool IsTesting { get; }
        public bool IsBeta { get; }
        public bool IsReleaseCandidate { get; }

        public Version(int major = 0, int minor = 0, int rev = 0, string let = "", bool beta = false)
        {
            Major = major;
            Minor = minor;
            Revision = rev;
            Letter = let;

            if (!string.IsNullOrEmpty(let) && let != "RC") 
                IsTesting = true;

            IsBeta = beta;

            if (!string.IsNullOrEmpty(let) && let == "RC")
                IsReleaseCandidate = true;

            if (!string.IsNullOrEmpty(Letter))
                FullName = $"{Major}.{Minor}.{Revision}-{Letter}";
            else 
                FullName = $"{Major}.{Minor}.{Revision}";
        }

        public override string ToString() => FullName;
        public override int GetHashCode() => Major + Minor + Revision;
        public override bool Equals(object obj) => ((Version)obj).GetHashCode() == GetHashCode();
    }
}
