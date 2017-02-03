using System;
using System.Linq;
using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Services;
using Moq;
using Xunit;
using System.Data.Entity;
using System.Collections.Generic;

namespace Geonorge.Kartografi.Tests
{
    public class CartographyServiceTests
    {
        [Fact]
        public void ShouldAddCartography()
        {
            var mockSet = new Mock<DbSet<CartographyFile>>();

            var mockContext = new Mock<CartographyDbContext>();
            mockContext.Setup(m => m.CartographyFiles).Returns(mockSet.Object);

            var versioning = new Mock<VersioningService>(mockContext.Object);
            var service = new CartographyService(mockContext.Object, versioning.Object);
            var compability = new List<Compatibility>();
            compability.Add(new Compatibility { Id = "WMS", Key = "WMS" });
            service.AddCartography( new CartographyFile { SystemId = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0cef"), Name = "Test", Compatibility = compability, Format = "sld" });

            mockSet.Verify(m => m.Add(It.IsAny<CartographyFile>()), Times.Once());
            mockContext.Verify(m => m.CartographyFiles, Times.Once());
        }

        [Fact]
        public void ShouldReturnCartography()
        {
            var data = new List<CartographyFile>
            {
                new CartographyFile { SystemId = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0fff"),  Name = "BBB" , versioning = new Models.Version { CurrentVersion = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0fff") } },
                new CartographyFile { SystemId = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0bff"),  Name = "ZZZ", versioning = new Models.Version { CurrentVersion = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0bff") } },
                new CartographyFile { SystemId = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0aff"), Name = "AAA", versioning = new Models.Version { CurrentVersion = Guid.Parse("c6056ed8-e040-42ef-b3c8-02f66fbb0aff") } },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<CartographyFile>>();
            mockSet.As<IQueryable<CartographyFile>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CartographyFile>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CartographyFile>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CartographyFile>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<CartographyDbContext>();
            mockContext.Setup(c => c.CartographyFiles).Returns(mockSet.Object);

            var versioning = new Mock<VersioningService>(mockContext.Object);
            var service = new CartographyService(mockContext.Object, versioning.Object);
            var files = service.GetCartography();

            Assert.Equal(3, files.Count);
            Assert.Equal("BBB", files[0].Name);
            Assert.Equal("ZZZ", files[1].Name);
            Assert.Equal("AAA", files[2].Name);
        }
    }
}
