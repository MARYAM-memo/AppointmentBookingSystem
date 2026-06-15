using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AppointmentBooking.Infrastructure.Comparers;

public class ValueComparers
{
    /// <summary>
    /// Provides a ValueComparer for Dictionary&lt;string, string&gt; to enable EF Core change tracking on dictionary properties.
    /// </summary>
    public static ValueComparer<Dictionary<string, string>> GetDictionaryComparer()
    {
        return new ValueComparer<Dictionary<string, string>>(
            // Comparing equality: Are the two dictionaries equal?
            (d1, d2) =>
                (d1 != null && d2 != null && d1.Count == d2.Count && !d1.Except(d2).Any()) ||
                (d1 == null && d2 == null),

            // Calculate a unique hash code for the dictionary (for speed)
            d => d != null
                ? d.Aggregate(0, (hash, pair) =>
                    HashCode.Combine(hash, pair.Key.GetHashCode(), pair.Value != null ? pair.Value.GetHashCode() : 0))
                : 0,

            // Deep copy (for tracking)
            d => new Dictionary<string, string>(d)
        );
    }

    /// <summary>
    /// Provides a ValueComparer for List&lt;string&gt; to enable EF Core change tracking on list properties.
    /// </summary>
    public static ValueComparer<List<string>> GetStringListComparer()
    {
        return new ValueComparer<List<string>>(
            // compare equality
            (l1, l2) =>
                (l1 != null && l2 != null && l1.SequenceEqual(l2)) ||
                (l1 == null && l2 == null),

            // Hash calculation
            l => l != null
                ? l.Aggregate(0, (hash, item) =>
                    HashCode.Combine(hash, item != null ? item.GetHashCode() : 0))
                : 0,

            //deep copy
            l => l != null ? l.ToList() : new List<string>()
        );
    }

}
