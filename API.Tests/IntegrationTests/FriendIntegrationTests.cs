using System.Net.Http.Json;
using API.DTOs.Friendships;
using API.Tests.IntegrationTests.Fixtures;
using API.Tests.IntegrationTests.Helpers;
using FluentAssertions;

namespace API.Tests.IntegrationTests;

[Collection("Integration")]
public class FriendIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _factory;

    public FriendIntegrationTests(IntegrationTestFixture factory) => _factory = factory;

    [Fact]
    public async Task FriendRequest_SendAndAccept_BothUsersSeeEachOtherAsFriends()
    {
        // Arrange: Two authenticated users
        var clientA = _factory.Factory.CreateClient();
        var userA = await IntegrationTestHelpers.RegisterUserAsync(
            clientA, "userA", "userA@test.com", "Password123!"
        );
        var authA = _factory.Factory.CreateAuthenticatedClient(userA);

        var clientB = _factory.Factory.CreateClient();
        var userB = await IntegrationTestHelpers.RegisterUserAsync(
            clientB, "userB", "userB@test.com", "Password123!"
        );
        var authB = _factory.Factory.CreateAuthenticatedClient(userB);

        // Act: A sends request to B, B accepts
        await authA.PostAsync($"/api/friends/request/{userB.Id}", null);

        var pending = await authB.GetAsync("/api/friends/requests");
        var requests = await pending.Content.ReadFromJsonAsync<List<FriendRequestDto>>();
        var friendshipId = requests.First(r => r.RequesterId == userA.Id).FriendshipId;

        await authB.PutAsync($"/api/friends/accept/{friendshipId}", null);

        // Assert: Both see each other as friends
        var friendsA = await authA.GetAsync("/api/friends");
        var listA = await friendsA.Content.ReadFromJsonAsync<List<FriendDto>>();

        listA.Should().Contain(f => f.UserId == userB.Id);

        var friendsB = await authB.GetAsync("/api/friends");
        var listB = await friendsB.Content.ReadFromJsonAsync<List<FriendDto>>();

        listB.Should().Contain(f => f.UserId == userA.Id);
    }
}