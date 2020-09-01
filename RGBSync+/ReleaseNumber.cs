using System;

public class ReleaseNumberComponent
{
    public int Value { get; set; }


}
public class Major : ReleaseNumberComponent
{
    public static explicit operator Major(int a)
    {
        return new Major
        {
            Value = a
        };
    }
}

public class Minor : ReleaseNumberComponent
{
    public static explicit operator Minor(int a)
    {
        return new Minor
        {
            Value = a
        };
    }
}
public class Revision : ReleaseNumberComponent
{
    public static explicit operator Revision(int a)
    {
        return new Revision
        {
            Value = a
        };
    }
}

public class Build : ReleaseNumberComponent
{
    public static explicit operator Build(int a)
    {
        return new Build
        {
            Value = a
        };
    }
}

public class ReleaseNumber : IEquatable<ReleaseNumber>, IComparable<ReleaseNumber>
{
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Revision { get; set; }
    public int Build { get; set; }

    public ReleaseNumber()
    {

    }

    public ReleaseNumber(string input)
    {
        string[] parts = input.Split('.');

        if (parts.Length > 0) Major = int.Parse(parts[0]);
        if (parts.Length > 1) Minor = int.Parse(parts[1]);
        if (parts.Length > 2) Revision = int.Parse(parts[2]);
        if (parts.Length > 3) Build = int.Parse(parts[3]);

    }

    public static explicit operator ReleaseNumber(string input)
    {
        var x = new ReleaseNumber();

        string[] parts = input.Split('.');

        if (parts.Length > 0) x.Major = int.Parse(parts[0]);
        if (parts.Length > 1) x.Minor = int.Parse(parts[1]);
        if (parts.Length > 2) x.Revision = int.Parse(parts[2]);
        if (parts.Length > 3) x.Build = int.Parse(parts[3]);

        return x;
    }

    public static ReleaseNumber operator +(ReleaseNumber a, int b)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision, a.Build + b);
    }

    public static ReleaseNumber operator +(ReleaseNumber a, Major b)
    {
        return new ReleaseNumber(a.Major + b.Value, a.Minor, a.Revision, a.Build);
    }

    public static ReleaseNumber operator +(ReleaseNumber a, Minor b)
    {
        return new ReleaseNumber(a.Major, a.Minor + b.Value, a.Revision, a.Build);
    }

    public static ReleaseNumber operator +(ReleaseNumber a, Revision b)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision + b.Value, a.Build);
    }
    public static ReleaseNumber operator +(ReleaseNumber a, Build b)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision, a.Build + b.Value);
    }

    public static ReleaseNumber operator ++(ReleaseNumber a)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision, a.Build + 1);
    }

    public static ReleaseNumber operator --(ReleaseNumber a)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision, a.Build - 1);
    }

    public static ReleaseNumber operator -(ReleaseNumber a, int b)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision, a.Build - b);
    }


    public static ReleaseNumber operator -(ReleaseNumber a, Major b)
    {
        return new ReleaseNumber(a.Major - b.Value, a.Minor, a.Revision, a.Build);
    }

    public static ReleaseNumber operator -(ReleaseNumber a, Minor b)
    {
        return new ReleaseNumber(a.Major, a.Minor - b.Value, a.Revision, a.Build);
    }

    public static ReleaseNumber operator -(ReleaseNumber a, Revision b)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision - b.Value, a.Build);
    }
    public static ReleaseNumber operator -(ReleaseNumber a, Build b)
    {
        return new ReleaseNumber(a.Major, a.Minor, a.Revision, a.Build - b.Value);
    }

    public override string ToString()
    {
        string b = Build.ToString();
        while (b.Length < 4)
        {
            b = "0" + b;
        }

        return $"{Major}.{Minor}.{Revision}.{b}";
    }

    public ReleaseNumber(int major, int minor, int revision, int build)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
        Build = build;
    }


    public bool Equals(ReleaseNumber other)
    {
        return ToString() == other?.ToString();
    }

    public int CompareTo(ReleaseNumber other)
    {
        if (other.Major > Major) return -1;
        if (other.Minor > Minor) return -1;
        if (other.Revision > Revision) return -1;
        if (other.Build > Build) return -1;

        if (other.Major < Major) return 1;
        if (other.Minor < Minor) return 1;
        if (other.Revision < Revision) return 1;
        if (other.Build < Build) return 1;

        return 0;
    }

    public static bool operator <(ReleaseNumber self, ReleaseNumber other)
    {
        if (self.Major < other.Major) return true;
        if (self.Minor < other.Minor) return true;
        if (self.Revision < other.Revision) return true;
        if (self.Build < other.Build) return true;

        return false;
    }

    public static bool operator <=(ReleaseNumber self, ReleaseNumber other)
    {
        if (self.Major == other.Major &&
            self.Minor == other.Minor &&
            self.Revision == other.Revision &&
            self.Build == other.Build) return true;

        if (self.Major < other.Major) return true;
        if (self.Minor < other.Minor) return true;
        if (self.Revision < other.Revision) return true;
        if (self.Build < other.Build) return true;

        return false;
    }

    public static bool operator >=(ReleaseNumber self, ReleaseNumber other)
    {
        if (self.Major == other.Major &&
            self.Minor == other.Minor &&
            self.Revision == other.Revision &&
            self.Build == other.Build) return true;

        if (self.Major > other.Major) return true;
        if (self.Minor > other.Minor) return true;
        if (self.Revision > other.Revision) return true;
        if (self.Build > other.Build) return true;

        return false;
    }

    public static bool operator >(ReleaseNumber self, ReleaseNumber other)
    {
        if (self.Major > other.Major) return true;
        if (self.Minor > other.Minor) return true;
        if (self.Revision > other.Revision) return true;
        if (self.Build > other.Build) return true;

        return false;
    }
}