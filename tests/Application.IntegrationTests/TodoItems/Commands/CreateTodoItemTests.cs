// Replace with remaining queries and commands

//using SSW.Rewards.Application.Common.Exceptions;
//using SSW.Rewards.Application.TodoItems.Commands.CreateTodoItem;
//using SSW.Rewards.Application.TodoLists.Commands.CreateTodoList;
//using SSW.Rewards.Domain.Entities;
//using FluentAssertions;
//using NUnit.Framework;

//namespace SSW.Rewards.Application.IntegrationTests.TodoItems.Commands;

//using static Testing;

//public class CreateTodoItemTests : BaseTestFixture
//{
//    [Test]
//    public async Task ShouldRequireMinimumFields()
//    {
//        var command = new CreateTodoItemCommand();

//        await FluentActions.Invoking(() =>
//            SendAsync(command)).Should().ThrowAsync<ValidationException>();
//    }

//    [Test]
//    public async Task ShouldCreateTodoItem()
//    {
//        var userId = await RunAsDefaultUserAsync();

//        var listId = await SendAsync(new CreateTodoListCommand
//        {
//            Title = "New List"
//        });

//        var command = new CreateTodoItemCommand
//        {
//            ListId = listId,
//            Title = "Tasks"
//        };

//        var itemId = await SendAsync(command);

//        var item = await FindAsync<TodoItem>(itemId);

//        item.Should().NotBeNull();
//        item!.ListId.Should().Be(command.ListId);
//        item.Title.Should().Be(command.Title);
//        item.CreatedBy.Should().Be(userId);
//        item.Created.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
//        item.LastModifiedBy.Should().Be(userId);
//        item.LastModified.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
//    }
//}
