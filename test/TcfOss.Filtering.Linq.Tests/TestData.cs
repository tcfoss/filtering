namespace TcfOss.Filtering.Linq.Tests;

public static class TestData
{
    // ReSharper disable NotAccessedPositionalProperty.Local
    public record Person(string Name, int Age, string City, List<Person>? Friends = null, int? Score = null);
    // ReSharper restore NotAccessedPositionalProperty.Local

    public static readonly Person[] People =
    [
        new("Alice", 30, "New York", [
            new Person("Bob", 25, "Los Angeles"),
            new Person("Grace", 29, "San Antonio")
        ], 85),
        new("Bob", 25, "Los Angeles", [
            new Person("Alice", 30, "New York"),
            new Person("Ivan", 26, "Dallas")
        ], 90),
        new("Charlie", 35, "Chicago", null, 78),
        new("David", 28, "Houston", [
            new Person("Mallory", 27, "Fort Worth"),
            new Person("Oscar", 28, "Charlotte")
        ], null),
        new("Eve", 32, "Phoenix", null, 92),
        new("Frank", 27, "Philadelphia", null, null),
        new("Grace", 29, "San Antonio", [
            new Person("Alice", 30, "New York"),
            new Person("Yvonne", 29, "Memphis")
        ], 88),
        new("Heidi", 31, "San Diego", null, 95),
        new("Ivan", 26, "Dallas", [
            new Person("Bob", 25, "Los Angeles"),
            new Person("Sybil", 26, "Denver")
        ], 82),
        new("Judy", 33, "New Dublin", null, null),

        new("Karl", 24, "Austin", null, 87),
        new("Leo", 34, "Jacksonville", null, 91),
        new("Mallory", 27, "Fort Worth", null, 80),
        new("Nina", 30, "Columbus", null, null),
        new("Oscar", 28, "Charlotte", null, 89),
        new("Peggy", 31, "San Francisco", null, 85),
        new("Quentin", 29, "Indianapolis", null, 93),
        new("Rupert", 32, "Seattle", null, null),
        new("Sybil", 26, "Denver", null, 84),
        new("Trent", 33, "Washington", null, 90),

        new("Uma", 27, "Boston", null, null),
        new("Victor", 30, "El Paso", null, 86),
        new("Walter", 28, "Nashville", null, 88),
        new("Xavier", 31, "New Orleans", null, 94),
        new("Yvonne", 29, "Memphis", null, 81),
        new("Zara", 35, "Portland", null, null)
    ];
}
