using System.IO;
using Infrastructure.Test;
using Infrastructure.Web;
using Newtonsoft.Json;
using Northwind;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Test.Infrastructure
{
    public class InfrastructureTests
    {
        [Theory, AutoData]
        public void ShoudSerializeAndDeserializeRowVersion(Category category)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JQueryArrayConverter());
            var writer = new StringWriter();
            serializer.Serialize(writer, category);
            var reader = new JsonTextReader(new StringReader(writer.ToString()));
            
            var deserialized = serializer.Deserialize<Category>(reader);

            deserialized.ShouldBeEquivalentTo(category);
        }
    }
}