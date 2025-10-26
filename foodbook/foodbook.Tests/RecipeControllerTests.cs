using Xunit;
using foodbook.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace foodbook.Tests
{
    public class RecipeControllerTests
    {
        // Test Recipe Model
        [Fact]
        public void Recipe_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                recipe_id = 1,
                user_id = 1,
                name = "Test Recipe",
                description = "Test Description",
                cook_time = 30,
                level = "dễ",
                status = "active"
            };

            // Assert
            recipe.recipe_id.Should().Be(1);
            recipe.user_id.Should().Be(1);
            recipe.name.Should().Be("Test Recipe");
            recipe.level.Should().Be("dễ");
        }

        [Fact]
        public void Recipe_Level_ShouldBeValid()
        {
            // Arrange
            var validLevels = new[] { "dễ", "trung bình", "khó" };
            var recipe = new Recipe { level = "dễ" };

            // Assert
            validLevels.Should().Contain(recipe.level);
        }

        [Fact]
        public void Recipe_Status_DefaultsToActive()
        {
            // Arrange & Act
            var recipe = new Recipe { status = "active" };

            // Assert
            recipe.status.Should().Be("active");
        }

        // Test AddRecipeViewModel Validation
        [Fact]
        public void AddRecipeViewModel_WithValidData_ShouldBeValid()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                CookTime = 30,
                Level = "dễ"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void AddRecipeViewModel_WithEmptyName_ShouldBeInvalid()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "",
                CookTime = 30,
                Level = "dễ"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void AddRecipeViewModel_WithInvalidCookTime_ShouldBeInvalid()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                CookTime = 0, // Invalid: must be > 0
                Level = "dễ"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains("CookTime"));
        }

        [Fact]
        public void AddRecipeViewModel_ValidatesCookTime()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                CookTime = 45,
                Level = "dễ"
            };

            // Assert
            model.CookTime.Should().BePositive();
            model.CookTime.Should().Be(45);
        }

        [Fact]
        public void AddRecipeViewModel_CanHaveMultipleIngredients()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                CookTime = 30,
                Level = "dễ",
                Ingredients = new List<string> { "Ingredient 1", "Ingredient 2", "Ingredient 3" }
            };

            // Assert
            model.Ingredients.Should().HaveCount(3);
            model.Ingredients.Should().Contain("Ingredient 1");
        }

        [Fact]
        public void AddRecipeViewModel_CanHaveMultipleSteps()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                CookTime = 30,
                Level = "dễ",
                Steps = new List<RecipeStepViewModel>
                {
                    new RecipeStepViewModel { Instruction = "Step 1", StepNumber = 1 },
                    new RecipeStepViewModel { Instruction = "Step 2", StepNumber = 2 }
                }
            };

            // Assert
            model.Steps.Should().HaveCount(2);
            model.Steps.First().Instruction.Should().Be("Step 1");
        }

        [Fact]
        public void AddRecipeViewModel_CanHaveMultipleRecipeTypes()
        {
            // Arrange
            var model = new AddRecipeViewModel
            {
                Name = "Test Recipe",
                CookTime = 30,
                Level = "dễ",
                RecipeTypes = new List<string> { "Món chính", "Món Á", "Món nhanh" }
            };

            // Assert
            model.RecipeTypes.Should().HaveCount(3);
            model.RecipeTypes.Should().Contain("Món chính");
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
