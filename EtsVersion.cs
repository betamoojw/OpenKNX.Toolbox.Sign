
namespace OpenKNX.Toolbox.Sign;

class EtsVersion
{
    public EtsVersion(string version, int ns, bool isExact)
    {
        Name = "ETS " + version;
        Version = version;
        Namespace = ns;
        IsExactNamespace = isExact;
    }

    public string Path { get; set; }

    public string Name { get; private set; }

    public string Version { get; private set; }

    public int Namespace { get; private set; }

    public bool IsExactNamespace { get; private set; }

    public bool CheckNs(int ns)
    {
        if(IsExactNamespace)
            return Namespace == ns;
        return ns < Namespace;
    }
}