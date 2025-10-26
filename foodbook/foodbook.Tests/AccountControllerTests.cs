using Xunit;
using foodbook.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace foodbook.Tests
{
    public class AccountControllerTests
    {
        // Test LoginViewModel Validation
        [Fact]
        public void LoginViewModel_WithValidData_ShouldBeValid()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailOrPhone = "test@example.com",
                Password = "password123"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void LoginViewModel_WithEmptyEmailOrPhone_ShouldBeInvalid()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailOrPhone = "",
                Password = "password123"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().NotBeEmpty();
        }

        [Fact]
        public void LoginViewModel_WithEmptyPassword_ShouldBeInvalid()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailOrPhone = "test@example.com",
                Password = ""
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().NotBeEmpty();
        }

        // Test RegisterViewModel Validation
        [Fact]
        public void RegisterViewModel_WithValidData_ShouldBeValid()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void RegisterViewModel_WithMismatchedPasswords_ShouldBeInvalid()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword",
                FullName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void RegisterViewModel_WithInvalidEmail_ShouldBeInvalid()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "Test User"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains("Email"));
        }

        // Test ChangePasswordViewModel
        [Fact]
        public void ChangePasswordViewModel_WithValidData_ShouldBeValid()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void ChangePasswordViewModel_WithMismatchedPasswords_ShouldBeInvalid()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "DifferentPassword"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().NotBeEmpty();
        }

        // Test ForgotPasswordViewModel
        [Fact]
        public void ForgotPasswordViewModel_WithValidEmail_ShouldBeValid()
        {
            // Arrange
            var model = new ForgotPasswordViewModel
            {
                UsernameOrEmail = "test@example.com"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().BeEmpty();
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
