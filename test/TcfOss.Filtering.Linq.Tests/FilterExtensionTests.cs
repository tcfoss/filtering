using TcfOss.Filtering.Contracts;

namespace TcfOss.Filtering.Linq.Tests;

public class FilterExtensionTests
{
    private static Contracts.SimpleFilter Simple(string field = "Field", string op = FilterOperators.EqualTo, string value = "Value")
        => new(field, op, value);

    private static Contracts.QuantifiedFilter Quantified(string field = "Items", string op = QuantifiedOperators.Any)
        => new() { Field = field, Operator = op, SubFilter = Simple() };

    // -------------------------------------------------------------------------
    // Map
    // -------------------------------------------------------------------------

    public class MapTests
    {
        [Fact]
        public void Null_ReturnsNull()
        {
            Filter? result = ((Filter?)null).Map(f => f);
            Assert.Null(result);
        }

        [Fact]
        public void LeafFilter_TransformApplied()
        {
            Contracts.SimpleFilter simple = Simple("Name");
            Contracts.SimpleFilter replacement = Simple("Age");

            Filter? result = simple.Map(_ => replacement);

            Assert.Same(replacement, result);
        }

        [Fact]
        public void LeafFilter_TransformReturnsNull_ReturnsNull()
        {
            Filter? result = Simple().Map(_ => null);
            Assert.Null(result);
        }

        [Fact]
        public void CompositeFilter_TransformCalledOnEachLeaf()
        {
            int leafCallCount = 0;
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);

            composite.Map(f =>
            {
                if (f is Contracts.SimpleFilter)
                {
                    leafCallCount++;
                }

                return f;
            });

            Assert.Equal(2, leafCallCount);
        }

        [Fact]
        public void CompositeFilter_TransformCalledOnCompositeItself()
        {
            bool compositeCalled = false;
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);

            composite.Map(f =>
            {
                if (f is Contracts.CompositeFilter)
                {
                    compositeCalled = true;
                }

                return f;
            });

            Assert.True(compositeCalled);
        }

        [Fact]
        public void CompositeFilter_TransformCanReplaceComposite()
        {
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);
            Contracts.SimpleFilter replacement = Simple("Replacement");

            Filter? result = composite.Map(f => f is Contracts.CompositeFilter ? replacement : f);

            Assert.Same(replacement, result);
        }

        [Fact]
        public void CompositeFilter_AllChildrenRemoved_ReturnsNull()
        {
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);

            Filter? result = composite.Map(_ => null);

            Assert.Null(result);
        }

        [Fact]
        public void CompositeFilter_OneChildRemains_CollapsesToChild()
        {
            Contracts.SimpleFilter keep = Simple("Keep");
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("Remove"),
                keep,
            ]);

            Filter? result = composite.Map(f => f is Contracts.SimpleFilter { Field: "Remove" } ? null : f);

            Contracts.SimpleFilter leaf = Assert.IsType<Contracts.SimpleFilter>(result);
            Assert.Equal("Keep", leaf.Field);
        }

        [Fact]
        public void CompositeFilter_CollapsedToOneChild_TransformNotCalledOnComposite()
        {
            bool compositeCalled = false;
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("Remove"),
                Simple("Keep"),
            ]);

            composite.Map(f =>
            {
                if (f is Contracts.CompositeFilter)
                {
                    compositeCalled = true;
                }

                return f is Contracts.SimpleFilter { Field: "Remove" } ? null : f;
            });

            Assert.False(compositeCalled);
        }

        [Fact]
        public void QuantifiedFilter_SubFilterTransformed()
        {
            Contracts.SimpleFilter replacement = Simple("Value", FilterOperators.EqualTo, "new");
            var quantified = new Contracts.QuantifiedFilter
            {
                Field = "Tags",
                Operator = QuantifiedOperators.Any,
                SubFilter = Simple("Value", FilterOperators.EqualTo, "old"),
            };

            Filter? result = quantified.Map(f => f is Contracts.SimpleFilter ? replacement : f);

            Contracts.QuantifiedFilter resultQuantified = Assert.IsType<Contracts.QuantifiedFilter>(result);
            Assert.Same(replacement, resultQuantified.SubFilter);
        }

        [Fact]
        public void QuantifiedFilter_TransformCalledOnQuantifiedItself()
        {
            bool quantifiedCalled = false;
            Contracts.QuantifiedFilter quantified = Quantified();

            quantified.Map(f =>
            {
                if (f is Contracts.QuantifiedFilter)
                {
                    quantifiedCalled = true;
                }

                return f;
            });

            Assert.True(quantifiedCalled);
        }

        [Fact]
        public void QuantifiedFilter_SubFilterRemoved_ReturnsNull()
        {
            Filter? result = Quantified().Map(_ => null);
            Assert.Null(result);
        }

        [Fact]
        public void NestedComposite_AllLeavesVisited()
        {
            var visited = new List<string>();
            var outer = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                new Contracts.CompositeFilter(LogicalOperators.Or,
                [
                    Simple("B"),
                    Simple("C"),
                ]),
            ]);

            outer.Map(f =>
            {
                if (f is Contracts.SimpleFilter s)
                {
                    visited.Add(s.Field);
                }

                return f;
            });

            Assert.Equal(["A", "B", "C"], [.. visited.Order()]);
        }
    }

    // -------------------------------------------------------------------------
    // Find
    // -------------------------------------------------------------------------

    public class FindTests
    {
        [Fact]
        public void Null_ReturnsNull()
        {
            Filter? result = ((Filter?)null).Find(_ => true);
            Assert.Null(result);
        }

        [Fact]
        public void LeafFilter_Matches_ReturnsIt()
        {
            Contracts.SimpleFilter simple = Simple("Name");
            Filter? result = simple.Find(f => f is Contracts.SimpleFilter { Field: "Name" });
            Assert.Same(simple, result);
        }

        [Fact]
        public void LeafFilter_NoMatch_ReturnsNull()
        {
            Filter? result = Simple("Name").Find(f => f is Contracts.SimpleFilter { Field: "Age" });
            Assert.Null(result);
        }

        [Fact]
        public void CompositeFilter_Itself_Matches()
        {
            var composite = new Contracts.CompositeFilter(LogicalOperators.And, [Simple()]);
            Filter? result = composite.Find(f => f is Contracts.CompositeFilter);
            Assert.Same(composite, result);
        }

        [Fact]
        public void CompositeFilter_ChildMatches_ReturnsChild()
        {
            Contracts.SimpleFilter target = Simple("Target");
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("Other"),
                target,
            ]);

            Filter? result = composite.Find(f => f is Contracts.SimpleFilter { Field: "Target" });

            Assert.Same(target, result);
        }

        [Fact]
        public void CompositeFilter_NoMatch_ReturnsNull()
        {
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);

            Filter? result = composite.Find(f => f is Contracts.SimpleFilter { Field: "C" });

            Assert.Null(result);
        }

        [Fact]
        public void QuantifiedFilter_Itself_Matches()
        {
            Contracts.QuantifiedFilter quantified = Quantified("Tags");
            Filter? result = quantified.Find(f => f is Contracts.QuantifiedFilter);
            Assert.Same(quantified, result);
        }

        [Fact]
        public void QuantifiedFilter_SubFilterMatches_ReturnsSubFilter()
        {
            Contracts.SimpleFilter subFilter = Simple("Value");
            var quantified = new Contracts.QuantifiedFilter
            {
                Field = "Tags",
                Operator = QuantifiedOperators.Any,
                SubFilter = subFilter,
            };

            Filter? result = quantified.Find(f => f is Contracts.SimpleFilter { Field: "Value" });

            Assert.Same(subFilter, result);
        }

        [Fact]
        public void NestedComposite_DeepMatch_ReturnsCorrectFilter()
        {
            Contracts.SimpleFilter deep = Simple("Deep");
            var outer = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("Top"),
                new Contracts.CompositeFilter(LogicalOperators.Or,
                [
                    Simple("Shallow"),
                    deep,
                ]),
            ]);

            Filter? result = outer.Find(f => f is Contracts.SimpleFilter { Field: "Deep" });

            Assert.Same(deep, result);
        }
    }

    // -------------------------------------------------------------------------
    // TryExtract
    // -------------------------------------------------------------------------

    public class TryExtractTests
    {
        [Fact]
        public void Null_RemainingNullAndExtractedNull()
        {
            Filter? remaining = ((Filter?)null).TryExtract(
                f => (f as Contracts.SimpleFilter)?.Field,
                out string? extracted);

            Assert.Null(remaining);
            Assert.Null(extracted);
        }

        [Fact]
        public void SingleLeaf_Match_ExtractedAndRemainingNull()
        {
            Filter? remaining = Simple("Name").TryExtract(
                f => (f as Contracts.SimpleFilter)?.Field,
                out string? extracted);

            Assert.Null(remaining);
            Assert.Equal("Name", extracted);
        }

        [Fact]
        public void SingleLeaf_NoMatch_RemainingPreservedAndExtractedNull()
        {
            Contracts.SimpleFilter simple = Simple("Name");
            Filter? remaining = simple.TryExtract(
                f => f is Contracts.SimpleFilter { Field: "Age" } s ? s.Field : null,
                out string? extracted);

            Assert.Same(simple, remaining);
            Assert.Null(extracted);
        }

        [Fact]
        public void CompositeFilter_MatchingChildExtracted_OtherChildRemains()
        {
            Contracts.SimpleFilter target = Simple("Target");
            Contracts.SimpleFilter other = Simple("Other");
            var composite = new Contracts.CompositeFilter(LogicalOperators.And, [target, other]);

            Filter? remaining = composite.TryExtract(
                f => f is Contracts.SimpleFilter { Field: "Target" } s ? s : null,
                out Contracts.SimpleFilter? extracted);

            Contracts.SimpleFilter remainingLeaf = Assert.IsType<Contracts.SimpleFilter>(remaining);
            Assert.Equal("Other", remainingLeaf.Field);
            Assert.Equal("Target", extracted?.Field);
        }

        [Fact]
        public void CompositeFilter_OnlyFirstMatchExtracted_RestRemain()
        {
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
                Simple("C"),
            ]);

            Filter? remaining = composite.TryExtract(
                f => f as Contracts.SimpleFilter,
                out Contracts.SimpleFilter? extracted);

            Assert.Equal("A", extracted?.Field);
            Contracts.CompositeFilter remainingComposite = Assert.IsType<Contracts.CompositeFilter>(remaining);
            Assert.Equal(2, remainingComposite.Filters.Length);
        }

        [Fact]
        public void QuantifiedFilter_CanBeExtracted()
        {
            var quantified = new Contracts.QuantifiedFilter
            {
                Field = "Tags",
                Operator = QuantifiedOperators.Any,
                SubFilter = Simple("Value"),
            };
            var composite = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("Name"),
                quantified,
            ]);

            Filter? remaining = composite.TryExtract(
                f => f as Contracts.QuantifiedFilter,
                out Contracts.QuantifiedFilter? extracted);

            Contracts.SimpleFilter remainingLeaf = Assert.IsType<Contracts.SimpleFilter>(remaining);
            Assert.Equal("Name", remainingLeaf.Field);
            Assert.NotNull(extracted);
            Assert.Equal("Tags", extracted.Field);
            Assert.Equal(QuantifiedOperators.Any, extracted.Operator);
        }
    }

    // -------------------------------------------------------------------------
    // Merge
    // -------------------------------------------------------------------------

    public class MergeTests
    {
        [Fact]
        public void BothNull_ReturnsNull()
        {
            Filter? result = ((Filter?)null).Merge(null);
            Assert.Null(result);
        }

        [Fact]
        public void LeftNull_ReturnsRight()
        {
            Contracts.SimpleFilter right = Simple("Right");
            Filter? result = ((Filter?)null).Merge(right);
            Assert.Same(right, result);
        }

        [Fact]
        public void RightNull_ReturnsLeft()
        {
            Contracts.SimpleFilter left = Simple("Left");
            Filter? result = left.Merge(null);
            Assert.Same(left, result);
        }

        [Fact]
        public void TwoSimpleFilters_CreatesComposite()
        {
            Contracts.SimpleFilter left = Simple("Left");
            Contracts.SimpleFilter right = Simple("Right");

            Filter? result = left.Merge(right);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            Assert.Equal(2, composite.Filters.Length);
            Assert.Same(left, composite.Filters[0]);
            Assert.Same(right, composite.Filters[1]);
        }

        [Fact]
        public void TwoSimpleFilters_WithOrOperator_CreatesOrComposite()
        {
            Contracts.SimpleFilter left = Simple("Left");
            Contracts.SimpleFilter right = Simple("Right");

            Filter? result = left.Merge(right, LogicalOperators.Or);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.Or, composite.LogicalOperator);
            Assert.Equal(2, composite.Filters.Length);
        }

        [Fact]
        public void BothCompositeAndOperatorWithAnd_FlattensBothToOne()
        {
            var left = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);
            var right = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("C"),
                Simple("D"),
            ]);

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            Assert.Equal(4, composite.Filters.Length);
            Assert.Equal("A", ((Contracts.SimpleFilter)composite.Filters[0]).Field);
            Assert.Equal("B", ((Contracts.SimpleFilter)composite.Filters[1]).Field);
            Assert.Equal("C", ((Contracts.SimpleFilter)composite.Filters[2]).Field);
            Assert.Equal("D", ((Contracts.SimpleFilter)composite.Filters[3]).Field);
        }

        [Fact]
        public void BothCompositeButDifferentOperators_FlattensBothIntoMergeOperator()
        {
            var left = new Contracts.CompositeFilter(LogicalOperators.And, [Simple("A"), Simple("B")]);
            var right = new Contracts.CompositeFilter(LogicalOperators.Or, [Simple("C"), Simple("D")]);

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            // Left AND is flattened (A, B), then right OR is added as-is: (A, B, (C OR D))
            Assert.Equal(3, composite.Filters.Length);
            Assert.Equal("A", ((Contracts.SimpleFilter)composite.Filters[0]).Field);
            Assert.Equal("B", ((Contracts.SimpleFilter)composite.Filters[1]).Field);
            Assert.Same(right, composite.Filters[2]);
        }

        [Fact]
        public void LeftCompositeMatchingOperator_AddsRightToIt()
        {
            var left = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("A"),
                Simple("B"),
            ]);
            Contracts.SimpleFilter right = Simple("C");

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            Assert.Equal(3, composite.Filters.Length);
            Assert.Equal("C", ((Contracts.SimpleFilter)composite.Filters[2]).Field);
        }

        [Fact]
        public void RightCompositeMatchingOperator_AddsLeftToIt()
        {
            Contracts.SimpleFilter left = Simple("A");
            var right = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("B"),
                Simple("C"),
            ]);

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            Assert.Equal(3, composite.Filters.Length);
            Assert.Same(left, composite.Filters[0]);
            Assert.Equal("B", ((Contracts.SimpleFilter)composite.Filters[1]).Field);
            Assert.Equal("C", ((Contracts.SimpleFilter)composite.Filters[2]).Field);
        }

        [Fact]
        public void LeftCompositeNonMatchingOperator_WrapsInNewComposite()
        {
            var left = new Contracts.CompositeFilter(LogicalOperators.Or,
            [
                Simple("A"),
                Simple("B"),
            ]);
            Contracts.SimpleFilter right = Simple("C");

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            Assert.Equal(2, composite.Filters.Length);
            Assert.Same(left, composite.Filters[0]);
            Assert.Same(right, composite.Filters[1]);
        }

        [Fact]
        public void RightCompositeNonMatchingOperator_WrapsInNewComposite()
        {
            Contracts.SimpleFilter left = Simple("A");
            var right = new Contracts.CompositeFilter(LogicalOperators.Or,
            [
                Simple("B"),
                Simple("C"),
            ]);

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
            Assert.Equal(2, composite.Filters.Length);
            Assert.Same(left, composite.Filters[0]);
            Assert.Same(right, composite.Filters[1]);
        }

        [Fact]
        public void WithQuantifiedFilter_MergesTreatingAsNormalFilter()
        {
            Contracts.QuantifiedFilter left = Quantified("Items");
            Contracts.SimpleFilter right = Simple("Name");

            Filter? result = left.Merge(right);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(2, composite.Filters.Length);
            Assert.Same(left, composite.Filters[0]);
            Assert.Same(right, composite.Filters[1]);
        }

        [Fact]
        public void QuantifiedWithComposite_FlattensBothComposites()
        {
            var left = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Quantified("Tags"),
                Simple("A"),
            ]);
            var right = new Contracts.CompositeFilter(LogicalOperators.And,
            [
                Simple("B"),
                Quantified("Items"),
            ]);

            Filter? result = left.Merge(right, LogicalOperators.And);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(4, composite.Filters.Length);
        }

        [Fact]
        public void ChainedMerges_AccumulatesFilters()
        {
            Contracts.SimpleFilter a = Simple("A");
            Contracts.SimpleFilter b = Simple("B");
            Contracts.SimpleFilter c = Simple("C");

            Filter? result = a.Merge(b).Merge(c);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(3, composite.Filters.Length);
            Assert.Equal("A", ((Contracts.SimpleFilter)composite.Filters[0]).Field);
            Assert.Equal("B", ((Contracts.SimpleFilter)composite.Filters[1]).Field);
            Assert.Equal("C", ((Contracts.SimpleFilter)composite.Filters[2]).Field);
        }

        [Fact]
        public void OrOperatorChained_MaintainsOrLogic()
        {
            Contracts.SimpleFilter a = Simple("A");
            Contracts.SimpleFilter b = Simple("B");
            Contracts.SimpleFilter c = Simple("C");

            Filter? result = a.Merge(b, LogicalOperators.Or).Merge(c, LogicalOperators.Or);

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.Or, composite.LogicalOperator);
            Assert.Equal(3, composite.Filters.Length);
        }

        [Fact]
        public void DefaultOperatorIsAnd()
        {
            Contracts.SimpleFilter left = Simple("Left");
            Contracts.SimpleFilter right = Simple("Right");

            Filter? result = left.Merge(right); // No operator specified

            Contracts.CompositeFilter composite = Assert.IsType<Contracts.CompositeFilter>(result);
            Assert.Equal(LogicalOperators.And, composite.LogicalOperator);
        }
    }
}
