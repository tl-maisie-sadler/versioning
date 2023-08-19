namespace Versioning;

internal record DateAndOrder(int order, DateOnly date);

public class KnownTlVersions
{
    // todo: can this be non-static?
    public static KnownTlVersions Instance { get; } = new();

    private readonly Dictionary<string, DateAndOrder> _versions = new();
    private int _order = 0;

    public void Register(string version)
    {
        if (!DateOnly.TryParseExact(version, "yyyy-MM-dd", out var date)) throw new InvalidOperationException("Date is not valid");

        _versions[version] = new DateAndOrder(_order++, date);
    }

    public int GetOrder(string version)
    {
        if (_versions.TryGetValue(version, out var dateAndOrder))
            return dateAndOrder.order;

        throw new InvalidOperationException("Version is not registered");
    }

    internal string FindVersionForDate(DateOnly date)
    {
        DateOnly? lastDate = null;
        foreach (var (version, (versionOrder, versionDate)) in _versions)
        {
            var comparisonResult = date.CompareTo(versionDate);
            if (comparisonResult < 0)
                return lastDate?.ToString("yyyy-MM-dd") ?? throw new Exception("Not really handled");

            if (comparisonResult == 0)
                return versionDate.ToString("yyyy-MM-dd");

            lastDate = versionDate;
        }

        // todo: default logic
        throw new Exception("Could not find appropriate date");
    }
}
