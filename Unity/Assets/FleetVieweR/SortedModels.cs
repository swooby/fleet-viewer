using System;
using System.Collections.Generic;
using UnityEngine;

public class SortedModels
{
    public enum SortType
    {
        Name,
        Length
    }

    private class FunctionalComparer<T> : IComparer<T>
    {
        private Func<T, T, int> comparer;

        private FunctionalComparer(Func<T, T, int> comparer)
        {
            this.comparer = comparer;
        }

        public static IComparer<T> Create(Func<T, T, int> comparer)
        {
            return new FunctionalComparer<T>(comparer);
        }

        public int Compare(T x, T y)
        {
            int result = comparer(x, y);
            // Avoid "ArgumentException: element already exists"
            // by treating equality as being greater
			return result == 0 ? 1 : result;
        }
    }

    private static readonly IComparer<ModelInfo> CompareName = FunctionalComparer<ModelInfo>
        .Create((a, b) =>
    {
        return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
    });

    private static readonly IComparer<ModelInfo> CompareLength = FunctionalComparer<ModelInfo>
        .Create((a, b) =>
    {
        int result = ((IComparable)a.LengthMeters).CompareTo(b.LengthMeters);
        if (result == 0)
        {
            result = CompareName.Compare(a, b);
        }
        return result;
    });

    public static IComparer<ModelInfo> getComparer(SortType sortType)
    {
		switch (sortType)
		{
			case SortType.Name:
				return CompareName;
			case SortType.Length:
				return CompareLength;
			default:
				throw new ArgumentException("Unknown sortType == " + sortType);
		}
    }

	// TODO:(pv) Compare metadata such as manufacturer, role/type, etc...
}
