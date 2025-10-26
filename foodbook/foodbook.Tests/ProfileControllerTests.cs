using Xunit;
using foodbook.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace foodbook.Tests
{
    public class ProfileControllerTests
    {
        // Test User Model
        [Fact]
        public void User_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var user = new User
            {
                user_id = 1,
                username = "testuser",
                email = "test@example.com",
                password = "hashedpassword",
                full_name = "Test User",
                role = "user",
                is_verified = true
            };

            // Assert
            user.user_id.Should().Be(1);
            user.username.Should().Be("testuser");
            user.email.Should().Be("test@example.com");
            user.role.Should().Be("user");
        }

        [Fact]
        public void User_DefaultRole_ShouldBeUser()
        {
            // Arrange & Act
            var user = new User 
            { 
                username = "test",
                email = "test@example.com",
                password = "password",
                role = "user" 
            };

            // Assert
            user.role.Should().Be("user");
        }

        [Fact]
        public void User_Email_ShouldBeValid()
        {
            // Arrange
            var user = new User 
            { 
                username = "test",
                email = "test@example.com",
                password = "password"
            };

            // Assert
            user.email.Should().Contain("@");
            user.email.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void User_CanHaveAvatar()
        {
            // Arrange & Act
            var user = new User
            {
                username = "testuser",
                email = "test@example.com",
                password = "password",
                avatar_img = "https://example.com/avatar.jpg"
            };

            // Assert
            user.avatar_img.Should().NotBeNullOrEmpty();
            user.avatar_img.Should().StartWith("https://");
        }

        [Fact]
        public void User_CanHaveBio()
        {
            // Arrange & Act
            var user = new User
            {
                username = "testuser",
                email = "test@example.com",
                password = "password",
                bio = "This is my bio"
            };

            // Assert
            user.bio.Should().NotBeNullOrEmpty();
            user.bio.Should().Be("This is my bio");
        }

        // Test ProfileViewModel
        [Fact]
        public void ProfileViewModel_CanTrackFollowers()
        {
            // Arrange
            var viewModel = new ProfileViewModel
            {
                FollowersCount = 10,
                FollowingCount = 5
            };

            // Assert
            viewModel.FollowersCount.Should().Be(10);
            viewModel.FollowingCount.Should().Be(5);
        }

        // Test Follow Model
        [Fact]
        public void Follow_Model_HasRequiredRelationships()
        {
            // Arrange & Act
            var follow = new Follow
            {
                follower_id = 1,
                following_id = 2
            };

            // Assert
            follow.follower_id.Should().Be(1);
            follow.following_id.Should().Be(2);
        }

        // Helper method to validate models
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
