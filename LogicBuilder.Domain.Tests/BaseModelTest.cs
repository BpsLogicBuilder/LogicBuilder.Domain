using LogicBuilder.Domain.Json;
using System.Text.Json;

namespace LogicBuilder.Domain.Tests
{
    public class BaseModelTest
    {
        [Fact]
        public void CanSerializeAndDeserialize_ClassWithNestedBaseModel()
        {
            // Arrange
            SaveModelRequest saveModelRequest = new()
            {
                Entity = new EnrollmentModel
                {
                    StudentID = 1,
                    StudentName = "John Smith",
                    EntityState = EntityStateType.Modified,
                    CourseID = 2,
                    CourseTitle = "Economics",
                    EnrollmentID = 3
                },
                Operation = "Save"
            };

            // Act
            string json = JsonSerializer.Serialize(saveModelRequest);
            var deserializedRequest = JsonSerializer.Deserialize<SaveModelRequest>(json, Options);

            // Assert
            Assert.NotNull(deserializedRequest);
            var entity = Assert.IsType<EnrollmentModel>(deserializedRequest.Entity, exactMatch: false);
            Assert.Equal("Save", deserializedRequest.Operation);
            Assert.Equal(1, entity.StudentID);
            Assert.Equal("John Smith", entity.StudentName);
            Assert.Equal(EntityStateType.Modified, entity.EntityState);
            Assert.Equal(2, entity.CourseID);
            Assert.Equal("Economics", entity.CourseTitle);
            Assert.Equal(3, entity.EnrollmentID);
        }

        private static JsonSerializerOptions? _options;
        public static JsonSerializerOptions Options
        {
            get
            {
                if (_options != null)
                    return _options;

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new ModelConverter());
                _options = options;
                return _options;
            }
        }

        public class SaveModelRequest
        {
            public BaseModel? Entity { get; set; }
            public string Operation { get; set; } = "";
        }

        public class EnrollmentModel : BaseModel
        {
            public int EnrollmentID { get; set; }

            public int CourseID { get; set; }

            public int StudentID { get; set; }

            public string? CourseTitle { get; set; }

            public string? StudentName { get; set; }
        }
    }
}
