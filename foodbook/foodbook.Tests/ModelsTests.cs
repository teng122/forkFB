using Xunit;
using foodbook.Models;
using FluentAssertions;

namespace foodbook.Tests
{
    public class ModelsTests
    {
        [Fact]
        public void Recipe_CreatedAt_ShouldBeSet()
        {
            // Arrange & Act
            var recipe = new Recipe
            {
                name = "Test Recipe",
                created_at = DateTime.UtcNow
            };

            // Assert
            recipe.created_at.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Comment_Model_HasRequiredProperties()
        {
            // Arrange & Act
            var comment = new Comment
            {
                comment_id = 1,
                user_id = 1,
                recipe_id = 1,
                body = "Great recipe!",
                created_at = DateTime.UtcNow
            };

            // Assert
            comment.comment_id.Should().Be(1);
            comment.user_id.Should().Be(1);
            comment.recipe_id.Should().Be(1);
            comment.body.Should().Be("Great recipe!");
        }

        [Fact]
        public void Ingredient_Model_HasName()
        {
            // Arrange & Act
            var ingredient = new IngredientMaster
            {
                ingredient_id = 1,
                name = "Tomato",
                created_at = DateTime.UtcNow
            };

            // Assert
            ingredient.ingredient_id.Should().Be(1);
            ingredient.name.Should().Be("Tomato");
        }

        [Fact]
        public void RecipeType_Model_HasContent()
        {
            // Arrange & Act
            var recipeType = new RecipeType
            {
                recipe_type_id = 1,
                content = "Món chính",
                created_at = DateTime.UtcNow
            };

            // Assert
            recipeType.recipe_type_id.Should().Be(1);
            recipeType.content.Should().Be("Món chính");
        }

        [Fact]
        public void RecipeStep_Model_HasInstructionAndOrder()
        {
            // Arrange & Act
            var recipeStep = new RecipeStep
            {
                recipe_id = 1,
                step = 1,
                instruction = "Cut the vegetables"
            };

            // Assert
            recipeStep.recipe_id.Should().Be(1);
            recipeStep.step.Should().Be(1);
            recipeStep.instruction.Should().Be("Cut the vegetables");
        }

        [Fact]
        public void Media_Model_CanBeImageOrVideo()
        {
            // Arrange & Act
            var imageMedia = new Media
            {
                media_id = 1,
                media_img = "https://example.com/image.jpg",
                media_video = null
            };

            var videoMedia = new Media
            {
                media_id = 2,
                media_img = null,
                media_video = "https://example.com/video.mp4"
            };

            // Assert
            imageMedia.media_img.Should().NotBeNullOrEmpty();
            imageMedia.media_video.Should().BeNull();
            
            videoMedia.media_video.Should().NotBeNullOrEmpty();
            videoMedia.media_img.Should().BeNull();
        }

        [Fact]
        public void LikeDislike_Model_TracksUserInteraction()
        {
            // Arrange & Act
            var like = new likeDislike
            {
                ld_id = 1,
                user_id = 1,
                recipe_id = 1,
                body = "",
                created_at = DateTime.UtcNow
            };

            // Assert
            like.ld_id.Should().Be(1);
            like.user_id.Should().Be(1);
            like.recipe_id.Should().Be(1);
        }

        [Fact]
        public void Notebook_Model_SavesUserRecipe()
        {
            // Arrange & Act
            var notebook = new Notebook
            {
                user_id = 1,
                recipe_id = 1,
                created_at = DateTime.UtcNow
            };

            // Assert
            notebook.user_id.Should().Be(1);
            notebook.recipe_id.Should().Be(1);
            notebook.created_at.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Share_Model_TracksSharing()
        {
            // Arrange & Act
            var share = new Share
            {
                user_id = 1,
                recipe_id = 1,
                created_at = DateTime.UtcNow
            };

            // Assert
            share.user_id.Should().Be(1);
            share.recipe_id.Should().Be(1);
            share.created_at.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Report_Model_HasStatusAndBody()
        {
            // Arrange & Act
            var report = new Report
            {
                user_id = 1,
                recipe_id = 1,
                body = "Inappropriate content",
                status = "Đang xử lý",
                created_at = DateTime.UtcNow
            };

            // Assert
            report.user_id.Should().Be(1);
            report.recipe_id.Should().Be(1);
            report.body.Should().Be("Inappropriate content");
            report.status.Should().Be("Đang xử lý");
        }
    }
}

