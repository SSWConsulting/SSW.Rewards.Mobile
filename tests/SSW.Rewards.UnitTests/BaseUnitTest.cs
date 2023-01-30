using Moq.AutoMock;

namespace SSW.Rewards.UnitTests;

public class BaseUnitTest<TSut> where TSut : class
{
    public BaseUnitTest()
    {
        Mocker = new AutoMocker();
        Sut = Mocker.CreateInstance<TSut>();
    }

    public AutoMocker Mocker { get; }
    public TSut Sut { get; }
}