using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.UnitTests;

public class OnBoardingViewModelTests : BaseUnitTest<OnBoardingViewModel>
{
    #region Ctor

    [Fact]
    public void Ctor_ShouldInitializeCommands()
    {
        // Arrange

        // Act

        // Assert
        Sut.DoActionCommand.Should().NotBeNull();
        Sut.Swiped.Should().NotBeNull();
        Sut.Skip.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_ShouldInitializePropertiesCollection()
    {
        // Arrange
        var expected = new[]
        {
            nameof(Sut.SubHeading), nameof(Sut.Content), nameof(Sut.BackgroundColour), nameof(Sut.ButtonText),
            nameof(Sut.Points), nameof(Sut.HasPoints)
        };
        
        // Act

        // Assert
        Sut.Properties.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Ctor_ShouldInitializeWelcomeCarouselItem()
    {
        // Arrange

        // Act

        // Assert
        Sut.Items[0].Content.Should().Be("Talk to SSW people, attend their talks and scan their QR codes, and complete other fun achievements to earn points.");
        Sut.Items[0].Animation.Should().Be("Sophie.json");
        Sut.Items[0].SubHeading.Should().Be("Welcome!");
        Sut.Items[0].ButtonText.Should().Be("GET STARTED");
        Sut.Items[0].IsAnimation.Should().BeTrue();
    }
    
    [Fact]
    public void Ctor_ShouldInitializeClaimRewardsCarouselItem()
    {
        // Arrange

        // Act

        // Assert
        Sut.Items[1].Content.Should().Be("Exchange your points at SSW Events or at SSW booths at developer conferences for awesome rewards.");
        Sut.Items[1].Image.Should().Be("test_passed");
        Sut.Items[1].SubHeading.Should().Be("Claim Rewards");
        Sut.Items[1].ButtonText.Should().Be("NEXT");
        Sut.Items[1].IsAnimation.Should().BeFalse();
    }
    
    [Fact]
    public void Ctor_ShouldInitializeGoogleHubMaxCarouselItem()
    {
        // Arrange

        // Act

        // Assert
        Sut.Items[2].Content.Should().Be("Get on the leaderboard for a chance to win a Google Hub Max.");
        Sut.Items[2].Image.Should().Be("prize_hubmax");
        Sut.Items[2].SubHeading.Should().Be("Google Nest Hub Max");
        Sut.Items[2].ButtonText.Should().Be("NEXT");
        Sut.Items[2].IsAnimation.Should().BeFalse();
    }
    
    [Fact]
    public void Ctor_ShouldInitializeKeepcupCarouselItem()
    {
        // Arrange

        // Act

        // Assert
        Sut.Items[3].Content.Should().Be("Earn enough points and you could claim a cool SSW Keepcup.");
        Sut.Items[3].Image.Should().Be("v2cups");
        Sut.Items[3].SubHeading.Should().Be("SSW Keepcup");
        Sut.Items[3].ButtonText.Should().Be("NEXT");
        Sut.Items[3].HasPoints.Should().BeTrue();
        Sut.Items[3].Points.Should().Be(2000);
        Sut.Items[3].IsAnimation.Should().BeFalse();
    }
    
    [Fact]
    public void Ctor_ShouldInitializeMiBandCarouselItem()
    {
        // Arrange

        // Act

        // Assert
        Sut.Items[4].Content.Should().Be("Get on the leaderboard and earn a Mi Wrist band. Just like a FitBit, except more functionality and a month's battery life!");
        Sut.Items[4].Image.Should().Be("v2band");
        Sut.Items[4].SubHeading.Should().Be("Mi Band 6");
        Sut.Items[4].ButtonText.Should().Be("NEXT");
        Sut.Items[4].HasPoints.Should().BeTrue();
        Sut.Items[4].Points.Should().Be(6000);
        Sut.Items[4].IsAnimation.Should().BeFalse();
    }
    
    [Fact]
    public void Ctor_ShouldInitializeSpecificationReviewCarouselItem()
    {
        // Arrange

        // Act

        // Assert
        Sut.Items[5].Content.Should().Be("SSW Architects will help you successfully implement your project.");
        Sut.Items[5].Image.Should().Be("v2consultation");
        Sut.Items[5].SubHeading.Should().Be("Half Price Specification Review");
        Sut.Items[5].ButtonText.Should().Be("DONE");
        Sut.Items[5].HasPoints.Should().BeTrue();
        Sut.Items[5].Points.Should().Be(3000);
        Sut.Items[5].IsAnimation.Should().BeFalse();
    }

    [Fact]
    public void Ctor_ShouldSetSelectedItemToFirst()
    {
        // Arrange
        var expected = Sut.Items.First();
        
        // Act

        // Assert
        Sut.SelectedItem.Should().Be(expected);
    }

    [Fact]
    public void Ctor_ShouldSetDetailsFromSelectedItem()
    {
        // Arrange
        var selectedItem = Sut.SelectedItem;

        // Act

        // Assert
        Sut.SubHeading.Should().Be(selectedItem.SubHeading);
        Sut.Content.Should().Be(selectedItem.Content);
        Sut.ButtonText.Should().Be(selectedItem.ButtonText);
        Sut.HasPoints.Should().Be(selectedItem.HasPoints);
        Sut.Points.Should().Be(selectedItem.Points);
    }
    
    #endregion
    
    #region DoActionCommand

    [Fact]
    public void DoActionCommand_ShouldCallPopModalAsync_WhenIsLastItemTrue()
    {
        // Arrange
        Sut.Navigation = Mocker.GetMock<INavigation>().Object;
        Sut.SelectedItem = Sut.Items.Last();

        // Act
        Sut.DoActionCommand.Execute(null);

        // Assert
        Mocker.GetMock<INavigation>().Verify(c => c.PopModalAsync());
    }

    [Fact]
    public void DoActionCommand_ShouldRaiseScrollToRequestedEvent_WhenIsLastItemFalse()
    {
        // Arrange
        var nextSelectedIndex = Sut.Items.IndexOf(Sut.SelectedItem) + 1;
        
        // Act
        var raisedEvent = Assert.Raises<int>(handler => Sut.ScrollToRequested += handler, handler => Sut.ScrollToRequested -= handler,
            () => Sut.DoActionCommand.Execute(null));
    
        // Assert
        raisedEvent.Arguments.Should().Be(nextSelectedIndex);
        raisedEvent.Sender.Should().Be(Sut);
    }
    
    #endregion

    #region SwipedCommand

    [Fact]
    public void SwipedCommand_ShouldSetDetailsFromSelectedItem()
    {
        // Arrange
        var selectedItem = Sut.Items.Last();
        Sut.SelectedItem = selectedItem;
        
        // Act
        Sut.Swiped.Execute(null);

        // Assert
        Sut.SubHeading.Should().Be(selectedItem.SubHeading);
        Sut.Content.Should().Be(selectedItem.Content);
        Sut.ButtonText.Should().Be(selectedItem.ButtonText);
        Sut.HasPoints.Should().Be(selectedItem.HasPoints);
        Sut.Points.Should().Be(selectedItem.Points);
    }

    #endregion

    #region SkipCommand

    [Fact]
    public void SkipCommand_ShouldCallPopModalAsync()
    {
        // Arrange
        Sut.Navigation = Mocker.GetMock<INavigation>().Object;

        // Act
        Sut.Skip.Execute(null);

        // Assert
        Mocker.GetMock<INavigation>().Verify(c => c.PopModalAsync());
    }

    #endregion
}