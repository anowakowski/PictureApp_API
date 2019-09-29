using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Providers;

namespace PictureApp.API.Tests.Providers
{
    [TestFixture]
    public class FileFormatInspectorProviderTests : GuardClauseAssertionTests<FileFormatInspectorProvider>
    {
        [Test]
        public void ValidateFileFormat_WhenCalledWithUnrecognizableFileStreamFileFormat_FormatExceptionExpected()
        {
            // ARRANGE
            var fileSteam = new MemoryStream();
            var sut = new FileFormatInspectorProvider(Substitute.For<IConfiguration>());
            Action action = () => sut.ValidateFileFormat(fileSteam);

            // ACT & ASSERT
            action.Should().Throw<FormatException>();
        }

        [Test]
        public void ValidateFileFormat_WhenCalledAndGivenFileStreamIsInAllowedMediaType_TrueExpected()
        {
            // ARRANGE
            var configuration = Substitute.For<IConfiguration>();
            var configurationSection = Substitute.For<IConfigurationSection>();
            configurationSection.Value = "image/jpeg";
            configuration.GetSection(Arg.Any<string>()).Returns(configurationSection);
            var sut = new FileFormatInspectorProvider(configuration);

            // ACT
            var actual = sut.ValidateFileFormat(GetSampleJpegFile());

            // ASSERT
            actual.Should().BeTrue();
        }

        [Test]
        public void ValidateFileFormat_WhenCalledAndGivenFileStreamIsNotInAllowedMediaType_FalseExpected()
        {
            // ARRANGE
            var configuration = Substitute.For<IConfiguration>();
            var configurationSection = Substitute.For<IConfigurationSection>();
            configurationSection.Value.Returns(string.Empty);
            configuration.GetSection(Arg.Any<string>()).Returns(configurationSection);
            var sut = new FileFormatInspectorProvider(configuration);

            // ACT
            var actual = sut.ValidateFileFormat(GetSampleJpegFile());

            // ASSERT
            actual.Should().BeFalse();
        }

        private Stream GetSampleJpegFile()
        {
            var imageAsBase64 =
                @"/9j/4AAQSkZJRgABAQEAYABgAAD/4QAsRXhpZgAATU0AKgAAAAgAAQExAAIAAAAKAAAAGgAAAABHcmVlbnNob3QA/9sAQwAHBQUGBQQHBgUGCAcHCAoRCwoJCQoVDxAMERgVGhkYFRgXGx4nIR
                  sdJR0XGCIuIiUoKSssKxogLzMvKjInKisq/9sAQwEHCAgKCQoUCwsUKhwYHCoqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioq/8AAEQgAAgAEAwEiAAIR
                  AQMRAf/EAB8AAAEFAQEBAQEBAAAAAAAAAAABAgMEBQYHCAkKC//EALUQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqND
                  U2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5
                  +v/EAB8BAAMBAQEBAQEBAQEAAAAAAAABAgMEBQYHCAkKC//EALURAAIBAgQEAwQHBQQEAAECdwABAgMRBAUhMQYSQVEHYXETIjKBCBRCkaGxwQkjM1LwFWJy0QoWJDThJfEXGBkaJicoKSo1Nj
                  c4OTpDREVGR0hJSlNUVVZXWFlaY2RlZmdoaWpzdHV2d3h5eoKDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uLj5OXm5+jp6vLz9PX29/j5+v/
                  aAAwDAQACEQMRAD8A+baKKKAP/9k=";

            var bytes = Convert.FromBase64String(imageAsBase64);
            return new MemoryStream(bytes);
        }
    }
}