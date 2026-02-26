namespace TestTaskINT20H.Application.Shared;

/// <summary>
/// Represents a paginated result set. This is an immutable value object.
/// </summary>
public sealed record Page<T>
{
    public int Size { get; init; }
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<T> Content { get; init; }

    public Page(int size, int pageNumber, int totalPages, List<T> content)
    {
        if (size <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(size));

        if (pageNumber < 0)
            throw new ArgumentException("Page number cannot be negative.", nameof(pageNumber));

        if (totalPages < 0)
            throw new ArgumentException("Total pages cannot be negative.", nameof(totalPages));

        ArgumentNullException.ThrowIfNull(content);

        Size = size;
        PageNumber = pageNumber;
        TotalPages = totalPages;
        Content = content.AsReadOnly();
    }
}