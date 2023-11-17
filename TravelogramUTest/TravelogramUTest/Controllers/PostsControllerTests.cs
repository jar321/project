using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Travelogram.Controllers;
using Travelogram.Data;
using Travelogram.Models;
using Xunit;
using TravelogramUTest.Extensions;



namespace TravelogramUTest.Controllers
{
    public class PostsControllerTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private PostsController _controller;

        public PostsControllerTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();

            // Mocking SaveChangesAsync to return 1
            _mockContext.Setup(m => m.SaveChangesAsync(default(CancellationToken)))
                        .ReturnsAsync(1);

            // Mocking Posts DbSet
            var mockPosts = new List<Post> { new Post(), new Post() }.AsQueryable().AsDbSetMock();
            _mockContext.Setup(m => m.Posts).Returns(mockPosts.Object);

            // Mocking IWebHostEnvironment
            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            // Mocking UserManager<IdentityUser>
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var mockUserManager = new UserManager<IdentityUser>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new PostsController(_mockContext.Object, mockWebHostEnvironment.Object, mockUserManager);
        }


        /*
        [Fact]
        public async Task Delete_ValidPostId_ReturnsViewWithPost()
        {
            // Arrange
            var postId = 1; // Replace with a valid post ID

            // Create a mock post with the specified ID
            var post = new Post { Id = postId };

            _mockContext.Setup(m => m.Posts.Find(postId)).Returns(post);

            // Act
            var result =  _controller.Delete(postId);
            _mockContext.Verify(m => m.Posts.Find(postId), Times.Once());

            // Assert

            if (result is ViewResult viewResult)
            {
                var model = Assert.IsType<Post>(viewResult.Model);
                Assert.Equal(postId, model.Id);
            }
            else
            {
                Assert.True(false, "Expected a ViewResult, but got " + result.GetType().Name);
            }
        }

        */

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task Index_ReturnsViewWithPosts()
        {
            // Arrange
            // (Setup is already done in the constructor)

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result); // Check if the result is a ViewResult
            var model = Assert.IsAssignableFrom<IEnumerable<Post>>(viewResult.Model); // Check if the returned model is a list of posts
            Assert.Equal(2, model.Count()); // Check if the list contains 2 posts, as we mocked earlier
        }


        [Fact]
        public async Task Dislike_ValidPost_IncreasesDislikeCount()
        {
            // Arrange
            var postId = 1; // Replace with a valid post ID

            // Create a mock post with the specified ID
            var post = new Post { Id = postId, Dislikes = 0 };

            var dbContextMock = new Mock<ApplicationDbContext>();
            dbContextMock.Setup(db => db.Posts.FindAsync(postId)).ReturnsAsync(post);

            var controller = new PostsController(dbContextMock.Object, null, null);

            // Act
            var result = await controller.Dislike(postId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectToActionResult.ActionName);
            Assert.Equal(postId, redirectToActionResult.RouteValues["id"]);

            Assert.Equal(1, post.Dislikes); // Check that Dislikes count was increased
        }

        [Fact]
        public async Task Like_ValidPost_IncreasesLikeCount()
        {
            // Arrange
            var postId = 1; // Replace with a valid post ID

            // Create a mock post with the specified ID
            var post = new Post { Id = postId, Likes = 0 };

            var dbContextMock = new Mock<ApplicationDbContext>();
            dbContextMock.Setup(db => db.Posts.FindAsync(postId)).ReturnsAsync(post);

            var controller = new PostsController(dbContextMock.Object, null, null);

            // Act
            var result = await controller.Like(postId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectToActionResult.ActionName);
            Assert.Equal(postId, redirectToActionResult.RouteValues["id"]);

            Assert.Equal(1, post.Likes); // Check that Likes count was increased
        }
    }
}