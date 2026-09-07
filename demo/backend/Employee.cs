namespace Demo.Api;

public record Office(string City, string Country, string SecretId);

public record Department(string Name, Office Office);

public record Employee(
    int Id,
    string Name,
    Department Department,
    string JobTitle,
    decimal Salary,
    DateOnly HireDate,
    bool IsActive
);

public static class EmployeeData
{
    private static readonly Office s_london = new("London", "UK", "shh");
    private static readonly Office s_newYork = new("New York", "US", "shh");
    private static readonly Office s_berlin = new("Berlin", "DE", "shh");
    private static readonly Office s_singapore = new("Singapore", "SG", "shh");

    private static readonly Department s_engineering = new("Engineering", s_london);
    private static readonly Department s_marketing = new("Marketing", s_newYork);
    private static readonly Department s_finance = new("Finance", s_singapore);
    private static readonly Department s_hR = new("HR", s_berlin);
    private static readonly Department s_sales = new("Sales", s_newYork);

    public static readonly Employee[] All =
    [
        new(1,  "Alice Nguyen",   s_engineering, "Senior Engineer",    95000m,  new DateOnly(2019, 3, 10),  true),
        new(2,  "Bob Carter",     s_engineering, "Junior Engineer",    62000m,  new DateOnly(2022, 7, 1),   true),
        new(3,  "Carol Smith",    s_marketing,   "Marketing Manager",  80000m,  new DateOnly(2018, 1, 15),  true),
        new(4,  "David Lee",      s_marketing,   "Analyst",            55000m,  new DateOnly(2021, 4, 20),  false),
        new(5,  "Eve Torres",     s_hR,          "HR Specialist",      58000m,  new DateOnly(2020, 9, 5),   true),
        new(6,  "Frank Miller",   s_engineering, "Principal Engineer", 120000m, new DateOnly(2015, 6, 30),  true),
        new(7,  "Grace Kim",      s_finance,     "Finance Director",   110000m, new DateOnly(2016, 11, 1),  true),
        new(8,  "Hank Zhou",      s_finance,     "Analyst",            60000m,  new DateOnly(2023, 2, 14),  true),
        new(9,  "Iris Patel",     s_hR,          "HR Director",        95000m,  new DateOnly(2017, 8, 22),  true),
        new(10, "Jack Robinson",  s_engineering, "Staff Engineer",     105000m, new DateOnly(2014, 5, 11),  false),
        new(11, "Karen White",    s_marketing,   "Content Strategist", 67000m,  new DateOnly(2020, 3, 3),   true),
        new(12, "Leo Martinez",   s_engineering, "Junior Engineer",    63000m,  new DateOnly(2023, 8, 7),   true),
        new(13, "Mia Johnson",    s_finance,     "Accountant",         72000m,  new DateOnly(2019, 12, 1),  true),
        new(14, "Nick Brown",     s_sales,       "Account Executive",  75000m,  new DateOnly(2018, 7, 18),  true),
        new(15, "Olivia Davis",   s_sales,       "Sales Director",     115000m, new DateOnly(2013, 4, 25),  true),
        new(16, "Paul Wilson",    s_sales,       "Account Executive",  74000m,  new DateOnly(2021, 10, 12), false),
        new(17, "Quinn Moore",    s_engineering, "Senior Engineer",    98000m,  new DateOnly(2018, 2, 28),  true),
        new(18, "Rachel Taylor",  s_marketing,   "SEO Specialist",     61000m,  new DateOnly(2022, 5, 16),  true),
        new(19, "Sam Anderson",   s_hR,          "Recruiter",          54000m,  new DateOnly(2021, 1, 7),   true),
        new(20, "Tina Jackson",   s_finance,     "Senior Accountant",  84000m,  new DateOnly(2017, 3, 19),  true),
    ];
}
