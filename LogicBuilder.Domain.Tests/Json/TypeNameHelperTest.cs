using LogicBuilder.Domain.Json;

namespace LogicBuilder.Domain.Tests.Json
{
    public class TypeNameHelperTest
    {
        private readonly TypeNameHelper helper;
        public TypeNameHelperTest()
        {
            helper = new TypeNameHelper();
        }

        [Theory]
        [InlineData("Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", 0, 39)]
        [InlineData("Enrollment.Domain.Entities.LookUpsModel", 0, -1)]
        [InlineData("System.Linq.IQueryable`1[[Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Linq.Expressions, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", 0, 141)]
        public void IndexOfTopLevelCommaReturnsExpectedResult(string typeName, int startIndex, int expectedIndex)
        {
            //act
            var result = helper.IndexOfTopLevelComma(typeName, startIndex);

            //assert
            Assert.Equal(expectedIndex, result);
        }

        [Theory]
        [InlineData("Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain")]
        [InlineData("Enrollment.Domain.Entities.LookUpsModel", null)]
        [InlineData("Enrollment.Domain.Entities.LookUpsModel,", null)]
        [InlineData(", Enrollment.Domain", null)]
        [InlineData("System.Linq.IQueryable`1[[Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Linq.Expressions, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Linq.IQueryable`1[[Enrollment.Domain.Entities.LookUpsModel, Enrollment.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Linq.Expressions")]
        public void GetKeyReturnsExpectedResult(string typeName, string? expectedResult)
        {
            //act
            var result = helper.GetKey(typeName);

            //assert
            Assert.Equal(expectedResult, result);
        }
    }
}
