using TcfOss.Filtering.Contracts;
using TcfOss.Filtering.Linq.FilterMapping;

namespace TcfOss.Filtering.Linq.Tests.FilterMapping;

public class ReflectionFilterMapperTests
{
    private enum Gender
    {
        Male,
        Female,
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    // ReSharper disable ClassNeverInstantiated.Local
    private record Person(
        string Name,
        bool IsActive,
        char MiddleInitial,
        char? PreferredLetter,
        int BirthYear,
        int? DeathYear,
        uint Age,
        uint? AgeAtDeath,
        byte Iq,
        sbyte DeviationFromMeanIq,
        ushort WeightInPounds,
        short ShortNumber,
        ulong NetWorth,
        long LargeNumber,
        decimal Salary,
        double Score,
        float Rating,
        DateTime CreatedAt,
        DateOnly BirthDate,
        Guid Id,
        Gender Gender
    );

    private record Item(string Name, int Age, string Secret);

    private record Location(string City, string Country);
    private record Office(string Building, Location Location);
    private record Company(string Name, int Revenue, Office HeadOffice);

    // ReSharper restore ClassNeverInstantiated.Local
    // ReSharper restore NotAccessedPositionalProperty.Local

    private readonly ReflectionFilterMapper<Person> _mapper = new();

    [Fact]
    public void StringField_MapsCorrectly()
    {
        var dto = new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal("Alice", result.Value);
        Assert.IsType<string>(result.Value);
    }

    [Fact]
    public void IntField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("BirthYear", FilterOperators.GreaterThan, "1990");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(1990, result.Value);
        Assert.IsType<int>(result.Value);
    }

    [Fact]
    public void UIntField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Age", FilterOperators.GreaterThan, "30");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(30u, result.Value);
        Assert.IsType<uint>(result.Value);
    }

    [Fact]
    public void NullableUIntField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("AgeAtDeath", FilterOperators.EqualTo, "75");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(75u, result.Value);
        Assert.IsType<uint>(result.Value);
    }

    [Fact]
    public void ByteField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Iq", FilterOperators.GreaterThan, "100");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal((byte)100, result.Value);
        Assert.IsType<byte>(result.Value);
    }

    [Fact]
    public void SByteField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("DeviationFromMeanIq", FilterOperators.LessThan, "-5");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal((sbyte)-5, result.Value);
        Assert.IsType<sbyte>(result.Value);
    }

    [Fact]
    public void ShortField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("ShortNumber", FilterOperators.EqualTo, "1000");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal((short)1000, result.Value);
        Assert.IsType<short>(result.Value);
    }

    [Fact]
    public void UShortField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("WeightInPounds", FilterOperators.GreaterThan, "150");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal((ushort)150, result.Value);
        Assert.IsType<ushort>(result.Value);
    }

    [Fact]
    public void ULongField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("NetWorth", FilterOperators.GreaterThan, "1000000");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(1000000UL, result.Value);
        Assert.IsType<ulong>(result.Value);
    }

    [Fact]
    public void EnumField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Gender", FilterOperators.EqualTo, "Female");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(Gender.Female, result.Value);
    }

    [Fact]
    public void EnumField_CaseInsensitive_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Gender", FilterOperators.EqualTo, "male");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(Gender.Male, result.Value);
    }

    [Fact]
    public void DecimalField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Salary", FilterOperators.GreaterThanOrEqualTo, "1234.56");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(1234.56m, result.Value);
    }

    [Fact]
    public void DoubleField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Score", FilterOperators.LessThan, "9.9");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(9.9, result.Value);
    }

    [Fact]
    public void FloatField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("Rating", FilterOperators.LessThanOrEqualTo, "4.5");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(4.5f, result.Value);
    }

    [Fact]
    public void BoolField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("IsActive", FilterOperators.EqualTo, "true");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(true, result.Value);
        Assert.IsType<bool>(result.Value);
    }

    [Fact]
    public void CharField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("MiddleInitial", FilterOperators.EqualTo, "A");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal('A', result.Value);
        Assert.IsType<char>(result.Value);
    }

    [Fact]
    public void NullableCharField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("PreferredLetter", FilterOperators.EqualTo, "Z");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal('Z', result.Value);
        Assert.IsType<char>(result.Value);
    }

    [Fact]
    public void LongField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("LargeNumber", FilterOperators.EqualTo, "9999999999");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(9999999999L, result.Value);
        Assert.IsType<long>(result.Value);
    }

    [Fact]
    public void DateTimeField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("CreatedAt", FilterOperators.GreaterThan, "2024-01-15T00:00:00");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(new DateTime(2024, 1, 15), result.Value);
        Assert.IsType<DateTime>(result.Value);
    }

    [Fact]
    public void DateOnlyField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("BirthDate", FilterOperators.EqualTo, "1990-06-20");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(new DateOnly(1990, 6, 20), result.Value);
        Assert.IsType<DateOnly>(result.Value);
    }

    [Fact]
    public void GuidField_ParsesValue()
    {
        var id = Guid.NewGuid();
        var dto = new Contracts.SimpleFilter("Id", FilterOperators.EqualTo, id.ToString());
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(id, result.Value);
        Assert.IsType<Guid>(result.Value);
    }

    [Fact]
    public void NullableIntField_ParsesValue()
    {
        var dto = new Contracts.SimpleFilter("DeathYear", FilterOperators.EqualTo, "1865");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(1865, result.Value);
        Assert.IsType<int>(result.Value);
    }

    [Fact]
    public void IsNull_NullValue_DoesNotThrow()
    {
        var dto = new Contracts.SimpleFilter("DeathYear", FilterOperators.IsNull);
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(QueryOperator.IsNull, result.Operator);
    }

    [Fact]
    public void IsNotNull_NullValue_DoesNotThrow()
    {
        var dto = new Contracts.SimpleFilter("DeathYear", FilterOperators.IsNotNull);
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(QueryOperator.IsNotNull, result.Operator);
    }

    [Fact]
    public void NullValue_NonNullOperator_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SimpleFilter("BirthYear", FilterOperators.EqualTo);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains(FilterOperators.EqualTo, ex.Message);
    }

    [Fact]
    public void UnknownField_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SimpleFilter("Nonexistent", FilterOperators.EqualTo, "x");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }

    [Fact]
    public void FieldName_CaseInsensitive()
    {
        var dto = new Contracts.SimpleFilter("name", FilterOperators.EqualTo, "Alice");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal("Alice", result.Value);
    }

    [Fact]
    public void ParserIsCached_SecondCallDoesNotRepeatReflection()
    {
        // Both calls should succeed; this validates the cache doesn't corrupt
        var dto1 = new Contracts.SimpleFilter("BirthYear", FilterOperators.EqualTo, "1990");
        var dto2 = new Contracts.SimpleFilter("BirthYear", FilterOperators.EqualTo, "2000");

        SimpleFilter r1 = _mapper.ToFilter(dto1);
        SimpleFilter r2 = _mapper.ToFilter(dto2);

        Assert.Equal(1990, r1.Value);
        Assert.Equal(2000, r2.Value);
    }

    [Fact]
    public void CompositeFilter_InnerFiltersUseReflectionParsers()
    {
        var dto = new Contracts.CompositeFilter(LogicalOperators.And,
        [
            new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice"),
                new Contracts.SimpleFilter("Age", FilterOperators.GreaterThan, "25"),
            ]);

        CompositeFilter result = _mapper.ToFilter(dto);

        SimpleFilter nameFilter = Assert.IsType<SimpleFilter>(result.Filters[0]);
        SimpleFilter ageFilter = Assert.IsType<SimpleFilter>(result.Filters[1]);
        Assert.IsType<string>(nameFilter.Value);
        Assert.IsType<uint>(ageFilter.Value);
    }

    [Fact]
    public void DottedCountField_ParsesValueAsInt()
    {
        var dto = new Contracts.SimpleFilter("Name.Length", FilterOperators.GreaterThan, "3");
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(3, result.Value);
        Assert.IsType<int>(result.Value);
    }

    [Fact]
    public void DottedCountField_UnknownRoot_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SimpleFilter("Nonexistent.Count", FilterOperators.GreaterThan, "0");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToFilter(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }

    [Fact]
    public void DottedCountField_Whitelist_DisallowedRoot_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>(whitelist: ["BirthYear"]);
        var dto = new Contracts.SimpleFilter("Name.Length", FilterOperators.GreaterThan, "3");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void DottedCountField_IsNull_KnownRoot_Passes()
    {
        var dto = new Contracts.SimpleFilter("Name.Length", FilterOperators.IsNull);
        SimpleFilter result = _mapper.ToFilter(dto);

        Assert.Equal(QueryOperator.IsNull, result.Operator);
    }

    [Fact]
    public void CollectionNavigation_Count_Passes()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SimpleFilter("Friends.Count", FilterOperators.GreaterThan, "2");
        SimpleFilter result = mapper.ToFilter(dto);

        Assert.Equal(2, result.Value);
        Assert.IsType<int>(result.Value);
    }

    [Fact]
    public void CollectionNavigation_NonCountLength_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SimpleFilter("Friends.Name", FilterOperators.EqualTo, "Alice");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Friends", ex.Message);
    }

    [Fact]
    public void GetValueParser_UnsupportedType_ThrowsArgumentException()
    {
        Assert.Throws<FilterMappingException>(() => typeof(object).ConstructValueParser("Field"));
    }

    // -------------------------------------------------------------------------
    // Invalid value throws parsing error tests
    // -------------------------------------------------------------------------

    [Fact]
    public void InvalidInt_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.SimpleFilter("BirthYear", FilterOperators.EqualTo, "not-a-number");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("BirthYear", ex.Message);
        Assert.Contains("not-a-number", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void InvalidDateTime_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.SimpleFilter("CreatedAt", FilterOperators.EqualTo, "not-a-date");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("CreatedAt", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void InvalidGuid_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.SimpleFilter("Id", FilterOperators.EqualTo, "not-a-guid");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Id", ex.Message);
    }

    [Fact]
    public void InvalidBool_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.SimpleFilter("IsActive", FilterOperators.EqualTo, "yes");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("IsActive", ex.Message);
    }

    [Fact]
    public void InvalidChar_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.SimpleFilter("MiddleInitial", FilterOperators.EqualTo, "AB");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("MiddleInitial", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }


    // -------------------------------------------------------------------------
    // Whitelist / blacklist basic tests
    // -------------------------------------------------------------------------

    [Fact]
    public void Whitelist_AllowedField_Passes()
    {
        var mapper = new ReflectionFilterMapper<Item>(whitelist: ["Name", "Age"]);
        var dto = new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("Alice", result.Value);
    }

    [Fact]
    public void Whitelist_DisallowedField_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Item>(whitelist: ["Name", "Age"]);
        var dto = new Contracts.SimpleFilter("Secret", FilterOperators.EqualTo, "hidden");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Secret", ex.Message);
    }

    [Fact]
    public void Blacklist_AllowedField_Passes()
    {
        var mapper = new ReflectionFilterMapper<Item>(blacklist: ["Secret"]);
        var dto = new Contracts.SimpleFilter("Name", FilterOperators.EqualTo, "Alice");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("Alice", result.Value);
    }

    [Fact]
    public void Blacklist_BlockedField_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Item>(blacklist: ["Secret"]);
        var dto = new Contracts.SimpleFilter("Secret", FilterOperators.EqualTo, "hidden");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Secret", ex.Message);
    }

    [Fact]
    public void NoRestrictions_AnyField_Passes()
    {
        var mapper = new ReflectionFilterMapper<Item>();
        var dto = new Contracts.SimpleFilter("Secret", FilterOperators.EqualTo, "hidden");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("hidden", result.Value);
    }

    [Fact]
    public void Whitelist_CaseInsensitive_Passes()
    {
        var mapper = new ReflectionFilterMapper<Item>(whitelist: ["name"]);
        var dto = new Contracts.SimpleFilter("NAME", FilterOperators.EqualTo, "Alice");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("Alice", result.Value);
    }

    [Fact]
    public void Blacklist_CaseInsensitive_Blocks()
    {
        var mapper = new ReflectionFilterMapper<Item>(blacklist: ["secret"]);
        var dto = new Contracts.SimpleFilter("SECRET", FilterOperators.EqualTo, "hidden");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("SECRET", ex.Message);
    }

    [Fact]
    public void IsNull_StillEnforcesAccessControl()
    {
        var mapper = new ReflectionFilterMapper<Item>(whitelist: ["Name"]);
        var dto = new Contracts.SimpleFilter("Secret", FilterOperators.IsNull);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Secret", ex.Message);
    }

    // -------------------------------------------------------------------------
    // Whitelist / blacklist with wildcards
    // -------------------------------------------------------------------------

    // Whitelist .*

    [Fact]
    public void Whitelist_DirectWildcard_AllowsDirectChild()
    {
        var mapper = new ReflectionFilterMapper<Company>(whitelist: ["HeadOffice.*"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Building", FilterOperators.EqualTo, "Tower A");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("Tower A", result.Value);
    }

    [Fact]
    public void Whitelist_DirectWildcard_BlocksGrandchild()
    {
        var mapper = new ReflectionFilterMapper<Company>(whitelist: ["HeadOffice.*"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Location.City", FilterOperators.EqualTo, "London");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("HeadOffice.Location.City", ex.Message);
    }

    // Whitelist .**

    [Fact]
    public void Whitelist_RecursiveWildcard_AllowsDirectChild()
    {
        var mapper = new ReflectionFilterMapper<Company>(whitelist: ["HeadOffice.**"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Building", FilterOperators.EqualTo, "Tower A");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("Tower A", result.Value);
    }

    [Fact]
    public void Whitelist_RecursiveWildcard_AllowsGrandchild()
    {
        var mapper = new ReflectionFilterMapper<Company>(whitelist: ["HeadOffice.**"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Location.City", FilterOperators.EqualTo, "London");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("London", result.Value);
    }

    [Fact]
    public void Whitelist_RecursiveWildcard_MatchesOnMiddleAncestor()
    {
        var mapper = new ReflectionFilterMapper<Company>(whitelist: ["HeadOffice.Location.**"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Location.City", FilterOperators.EqualTo, "London");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("London", result.Value);
    }

    // Blacklist .*

    [Fact]
    public void Blacklist_DirectWildcard_BlocksDirectChild()
    {
        var mapper = new ReflectionFilterMapper<Company>(blacklist: ["HeadOffice.*"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Building", FilterOperators.EqualTo, "Tower A");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("HeadOffice.Building", ex.Message);
    }

    [Fact]
    public void Blacklist_DirectWildcard_AllowsGrandchild()
    {
        var mapper = new ReflectionFilterMapper<Company>(blacklist: ["HeadOffice.*"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Location.City", FilterOperators.EqualTo, "London");
        SimpleFilter result = mapper.ToFilter(dto);
        Assert.Equal("London", result.Value);
    }

    // Blacklist .**

    [Fact]
    public void Blacklist_RecursiveWildcard_BlocksDirectChild()
    {
        var mapper = new ReflectionFilterMapper<Company>(blacklist: ["HeadOffice.**"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Building", FilterOperators.EqualTo, "Tower A");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("HeadOffice.Building", ex.Message);
    }

    [Fact]
    public void Blacklist_RecursiveWildcard_BlocksGrandchild()
    {
        var mapper = new ReflectionFilterMapper<Company>(blacklist: ["HeadOffice.**"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Location.City", FilterOperators.EqualTo, "London");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("HeadOffice.Location.City", ex.Message);
    }

    [Fact]
    public void Blacklist_RecursiveWildcard_BlocksOnMiddleAncestor()
    {
        var mapper = new ReflectionFilterMapper<Company>(blacklist: ["HeadOffice.Location.**"]);
        var dto = new Contracts.SimpleFilter("HeadOffice.Location.City", FilterOperators.EqualTo, "London");
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("HeadOffice.Location.City", ex.Message);
    }

    // -------------------------------------------------------------------------
    // ReflectionFilterMapper<T> — ToSortComponent
    // -------------------------------------------------------------------------


    [Fact]
    public void Sort_KnownField_Passes()
    {
        var dto = new Contracts.SortComponent("Name", SortDirections.Ascending);
        SortComponent result = _mapper.ToSortComponent(dto);

        Assert.Equal("Name", result.Field);
        Assert.Equal(SortDirection.Ascending, result.Direction);
    }

    [Fact]
    public void Sort_UnknownField_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SortComponent("Nonexistent", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToSortComponent(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }

    [Fact]
    public void Sort_Whitelist_AllowedField_Passes()
    {
        var mapper = new ReflectionFilterMapper<Person>(whitelist: ["Name", "BirthYear"]);
        var dto = new Contracts.SortComponent("Name", SortDirections.Descending);
        SortComponent result = mapper.ToSortComponent(dto);

        Assert.Equal("Name", result.Field);
    }

    [Fact]
    public void Sort_Whitelist_DisallowedField_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>(whitelist: ["Name"]);
        var dto = new Contracts.SortComponent("Salary", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("Salary", ex.Message);
    }

    [Fact]
    public void Sort_Blacklist_BlockedField_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>(blacklist: ["Salary"]);
        var dto = new Contracts.SortComponent("Salary", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("Salary", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_Count_KnownRoot_Passes()
    {
        // Tests that .Count navigation on a collection field is validated and passed through
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SortComponent("Friends.Count", SortDirections.Ascending);
        SortComponent result = mapper.ToSortComponent(dto);

        Assert.Equal("Friends.Count", result.Field);
    }

    [Fact]
    public void Sort_DottedField_Length_KnownRoot_Passes()
    {
        var dto = new Contracts.SortComponent("Name.Length", SortDirections.Ascending);
        SortComponent result = _mapper.ToSortComponent(dto);

        Assert.Equal("Name.Length", result.Field);
    }

    [Fact]
    public void Sort_DottedField_UnknownRoot_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SortComponent("Nonexistent.Count", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToSortComponent(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_InvalidSuffix_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SortComponent("Name.Something", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToSortComponent(dto));
        Assert.Contains("Name.Something", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_TooManyParts_ThrowsFilterMappingException()
    {
        var dto = new Contracts.SortComponent("A.B.Count", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => _mapper.ToSortComponent(dto));
        Assert.Contains("A.B.Count", ex.Message);
    }

    [Fact]
    public void Sort_DottedField_Whitelist_DisallowedRoot_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>(whitelist: ["BirthYear"]);
        var dto = new Contracts.SortComponent("Name.Count", SortDirections.Ascending);
        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToSortComponent(dto));
        Assert.Contains("Name", ex.Message);
    }

    // -------------------------------------------------------------------------
    // SetFilter Mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void SetFilter_StringValues_MapsCorrectly()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SetFilter("Name", ["Alice", "Bob"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(2, result.Values.Length);
        Assert.IsType<string>(result.Values[0]);
    }

    [Fact]
    public void SetFilter_NavigationPropertyPath_MapsCorrectly()
    {
        var mapper = new ReflectionFilterMapper<Company>();
        var dto = new Contracts.SetFilter("HeadOffice.Location.City", ["London", "Paris"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(2, result.Values.Length);
        Assert.Equal("London", result.Values[0]);
        Assert.Equal("Paris", result.Values[1]);
        Assert.NotNull(result.ElementType);
        Assert.Equal(typeof(string), result.ElementType);
    }

    [Fact]
    public void SetFilter_EmptyValues_Throws()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SetFilter("Name", []);

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void SetFilter_UnknownField_Throws()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SetFilter("Nonexistent", ["A"]);

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }

    [Fact]
    public void SetFilter_NullableIntField_StoresElementTypeWithNullableWrapper()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SetFilter("Score", ["85", "90", "95"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(3, result.Values.Length);
        Assert.Equal(85, result.Values[0]);
        Assert.Equal(90, result.Values[1]);
        Assert.Equal(95, result.Values[2]);
        Assert.NotNull(result.ElementType);
        // Verify it's the nullable wrapper type
        Assert.Equal(typeof(int?), result.ElementType);
    }

    [Fact]
    public void SetFilter_NonNullableIntField_Age_StoresElementType()
    {
        var mapper = new ReflectionFilterMapper<TestData.Person>();
        var dto = new Contracts.SetFilter("Age", ["25", "30", "35"]);

        SetFilter result = mapper.ToFilter(dto);

        Assert.Equal(3, result.Values.Length);
        Assert.Equal(25, result.Values[0]);
        Assert.NotNull(result.ElementType);
        // Verify it's the non-nullable int type
        Assert.Equal(typeof(int), result.ElementType);
    }


    // -------------------------------------------------------------------------
    // RangeFilter Mapping
    // -------------------------------------------------------------------------

    [Fact]
    public void RangeFilter_ParsesBothBounds()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.RangeFilter("BirthYear", "1990", "2000");

        RangeFilter result = mapper.ToFilter(dto);

        Assert.Equal(1990, result.ValueFrom);
        Assert.Equal(2000, result.ValueTo);
    }

    [Fact]
    public void RangeFilter_InvalidValueFrom_ThrowsFilterMappingException()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.RangeFilter("BirthYear", "bad", "2000");

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("BirthYear", ex.Message);
        Assert.Contains("bad", ex.Message);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void RangeFilter_UnknownField_Throws()
    {
        var mapper = new ReflectionFilterMapper<Person>();
        var dto = new Contracts.RangeFilter("Nonexistent", "1", "10");

        FilterMappingException ex = Assert.Throws<FilterMappingException>(() => mapper.ToFilter(dto));
        Assert.Contains("Nonexistent", ex.Message);
    }
}
