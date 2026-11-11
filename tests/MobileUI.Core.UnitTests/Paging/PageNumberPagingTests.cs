using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.Core.UnitTests.Paging;

public class PageNumberPagingTests
{
    [TestCase(0, 20, 0)]
    [TestCase(20, 20, 1)]
    [TestCase(100, 20, 5)]
    public void ToPageIndex_WholePages_ReturnsZeroBasedPage(int skip, int take, int expected)
    {
        PageNumberPaging.ToPageIndex(skip, take).Should().Be(expected);
    }

    [Test]
    public void ToPageIndex_PartialPage_Throws()
    {
        // Skipping into the middle of a page would silently re-fetch or drop items.
        var act = () => PageNumberPaging.ToPageIndex(15, 20);

        act.Should().Throw<ArgumentException>();
    }

    [TestCase(-1, 20)]
    [TestCase(0, 0)]
    public void ToPageIndex_InvalidArguments_Throws(int skip, int take)
    {
        var act = () => PageNumberPaging.ToPageIndex(skip, take);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
