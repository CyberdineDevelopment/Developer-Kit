using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Services.DataProvider.Abstractions.Models;

/// <summary>
/// Represents a hierarchical path to a data container that can handle any depth and separator style.
/// </summary>
/// <remarks>
/// DataPath provides a flexible way to represent paths across different data providers:
/// - SQL Server: ["sales", "customers"] represents sales.customers
/// - File System: ["data", "customers", "archive"] represents /data/customers/archive
/// - REST API: ["api", "v1", "customers"] represents /api/v1/customers
/// - SFTP: ["uploads", "daily", "customers"] represents /uploads/daily/customers
/// </remarks>
public sealed class DataPath : IEquatable<DataPath>
{
    private readonly string[] _segments;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPath"/> class.
    /// </summary>
    /// <param name="segments">The path segments.</param>
    /// <param name="separator">The separator to use when converting to string.</param>
    /// <exception cref="ArgumentNullException">Thrown when segments is null.</exception>
    /// <exception cref="ArgumentException">Thrown when segments contains null or empty values.</exception>
    public DataPath(IEnumerable<string> segments, string separator = "/")
    {
        if (segments == null)
            throw new ArgumentNullException(nameof(segments));
        
        _segments = segments.ToArray();
        
        if (_segments.Length == 0)
            throw new ArgumentException("Path must contain at least one segment.", nameof(segments));
        
        for (int i = 0; i < _segments.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(_segments[i]))
                throw new ArgumentException($"Segment at index {i} cannot be null or empty.", nameof(segments));
        }
        
        Separator = separator ?? throw new ArgumentNullException(nameof(separator));
    }

    /// <summary>
    /// Gets the path segments.
    /// </summary>
    public IReadOnlyList<string> Segments => _segments;

    /// <summary>
    /// Gets the separator used when converting to string.
    /// </summary>
    public string Separator { get; }

    /// <summary>
    /// Gets the depth of the path (number of segments).
    /// </summary>
    public int Depth => _segments.Length;

    /// <summary>
    /// Gets the first segment of the path.
    /// </summary>
    public string Root => _segments[0];

    /// <summary>
    /// Gets the last segment of the path.
    /// </summary>
    public string Leaf => _segments[_segments.Length - 1];

    /// <summary>
    /// Parses a string path into a DataPath instance.
    /// </summary>
    /// <param name="path">The string path to parse.</param>
    /// <param name="separator">The separator used in the path string.</param>
    /// <returns>A new DataPath instance.</returns>
    /// <exception cref="ArgumentException">Thrown when path is null or empty.</exception>
    public static DataPath Parse(string path, string separator = "/")
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        
        var segments = path.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        return new DataPath(segments, separator);
    }

    /// <summary>
    /// Creates a DataPath with a single segment.
    /// </summary>
    /// <param name="segment">The single segment.</param>
    /// <param name="separator">The separator to use.</param>
    /// <returns>A new DataPath instance.</returns>
    public static DataPath Single(string segment, string separator = "/")
    {
        return new DataPath(new[] { segment }, separator);
    }

    /// <summary>
    /// Creates a DataPath from multiple segments.
    /// </summary>
    /// <param name="segments">The path segments.</param>
    /// <param name="separator">The separator to use.</param>
    /// <returns>A new DataPath instance.</returns>
    public static DataPath Create(string separator = "/", params string[] segments)
    {
        return new DataPath(segments, separator);
    }

    /// <summary>
    /// Creates a new DataPath by appending a segment to this path.
    /// </summary>
    /// <param name="segment">The segment to append.</param>
    /// <returns>A new DataPath instance with the appended segment.</returns>
    public DataPath Append(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Segment cannot be null or empty.", nameof(segment));
        
        var newSegments = new string[_segments.Length + 1];
        Array.Copy(_segments, newSegments, _segments.Length);
        newSegments[_segments.Length] = segment;
        
        return new DataPath(newSegments, Separator);
    }

    /// <summary>
    /// Creates a new DataPath by prepending a segment to this path.
    /// </summary>
    /// <param name="segment">The segment to prepend.</param>
    /// <returns>A new DataPath instance with the prepended segment.</returns>
    public DataPath Prepend(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Segment cannot be null or empty.", nameof(segment));
        
        var newSegments = new string[_segments.Length + 1];
        newSegments[0] = segment;
        Array.Copy(_segments, 0, newSegments, 1, _segments.Length);
        
        return new DataPath(newSegments, Separator);
    }

    /// <summary>
    /// Creates a new DataPath with a different separator.
    /// </summary>
    /// <param name="newSeparator">The new separator to use.</param>
    /// <returns>A new DataPath instance with the new separator.</returns>
    public DataPath WithSeparator(string newSeparator)
    {
        if (newSeparator == null)
            throw new ArgumentNullException(nameof(newSeparator));
        
        return new DataPath(_segments, newSeparator);
    }

    /// <summary>
    /// Gets a parent path by removing the last segment.
    /// </summary>
    /// <returns>A new DataPath instance without the last segment, or null if this is a root path.</returns>
    public DataPath? GetParent()
    {
        if (_segments.Length <= 1)
            return null;
        
        var parentSegments = new string[_segments.Length - 1];
        Array.Copy(_segments, parentSegments, parentSegments.Length);
        
        return new DataPath(parentSegments, Separator);
    }

    /// <summary>
    /// Checks if this path starts with the specified path.
    /// </summary>
    /// <param name="other">The path to check against.</param>
    /// <returns>True if this path starts with the other path; otherwise, false.</returns>
    public bool StartsWith(DataPath other)
    {
        if (other == null)
            return false;
        
        if (other._segments.Length > _segments.Length)
            return false;
        
        for (int i = 0; i < other._segments.Length; i++)
        {
            if (!string.Equals(_segments[i], other._segments[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Converts the path to a string using the specified separator.
    /// </summary>
    /// <param name="separator">The separator to use, or null to use the default separator.</param>
    /// <returns>The string representation of the path.</returns>
    public string ToString(string? separator)
    {
        return string.Join(separator ?? Separator, _segments);
    }

    /// <summary>
    /// Returns the string representation of the path using the default separator.
    /// </summary>
    /// <returns>The string representation of the path.</returns>
    public override string ToString()
    {
        return ToString(Separator);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as DataPath);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>True if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(DataPath? other)
    {
        if (other == null)
            return false;
        
        if (_segments.Length != other._segments.Length)
            return false;
        
        for (int i = 0; i < _segments.Length; i++)
        {
            if (!string.Equals(_segments[i], other._segments[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        
        for (int i = 0; i < _segments.Length; i++)
        {
            hash.Add(_segments[i].ToLowerInvariant());
        }
        
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether two DataPath instances are equal.
    /// </summary>
    /// <param name="left">The first DataPath to compare.</param>
    /// <param name="right">The second DataPath to compare.</param>
    /// <returns>True if the DataPath instances are equal; otherwise, false.</returns>
    public static bool operator ==(DataPath? left, DataPath? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        
        if (left is null || right is null)
            return false;
        
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two DataPath instances are not equal.
    /// </summary>
    /// <param name="left">The first DataPath to compare.</param>
    /// <param name="right">The second DataPath to compare.</param>
    /// <returns>True if the DataPath instances are not equal; otherwise, false.</returns>
    public static bool operator !=(DataPath? left, DataPath? right)
    {
        return !(left == right);
    }
}