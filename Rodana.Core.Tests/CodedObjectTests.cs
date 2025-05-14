using Rodana.Core.Common;

namespace Rodana.Core.Tests
{
    public class CodedObjectTests
    {
        [Fact]
        public void StringEqualCheck()
        {
            CodedObject<string, string> Test1 = new CodedObject<string, string>("abc", "LOLOLOL");
            CodedObject<string, string> Test4 = new CodedObject<string, string>("abc", "LOLO LOL");

            Assert.Equal(Test1, Test4); // Если отличаются именования, а не коды - всё в порядке.
        }

        [Fact]
        public void StringEmptyCheck()
        {
            CodedObject<string, string> Test4 = new CodedObject<string, string>("abc", "LOLO LOL");
            CodedObject<string, string> Test5 = new CodedObject<string, string>("", "LOLO LOL");
            CodedObject<string, string> Test6 = new CodedObject<string, string>(null, "LOLO LOL");

            Assert.False(Test4.IsEmpty);
            Assert.False(Test5.IsEmpty);
            Assert.True(Test6.IsEmpty);
        }

        [Fact]
        public void StringNonEqualCheck()
        {
            CodedObject<string, string> Test1 = new CodedObject<string, string>("abc", "LOLOLOL");
            CodedObject<string, string> Test3 = new CodedObject<string, string>("abcd", "LOLOLOL");
            CodedObject<string, string> Test4 = new CodedObject<string, string>("abcd", "LOLO LOL");
            CodedObject<string, string> Test5 = new CodedObject<string, string>("", "LOLO LOL");
            CodedObject<string, string> Test6 = new CodedObject<string, string>(null, "LOLO LOL");

            Assert.NotEqual(Test1, Test3); // Если отличаются коды - он не должен сработать.
            Assert.NotEqual(Test1, Test4); // Если отличаются коды - он не должен сработать.
            Assert.NotEqual(Test1, Test5); // Если отличаются коды - он не должен сработать.
            Assert.NotEqual(Test1, Test6); // Если отличаются коды - он не должен сработать.
        }

        [Fact]
        public void GuidEqualCheck()
        {
            var guid1 = new Guid("1e52df3f-9ce3-4734-9d93-24fc2e652fc9");
            var guid2 = new Guid("1e52df3f-9ce3-4734-9d93-24fc2e652fc9");
            CodedObject<Guid, string> Test1 = new CodedObject<Guid, string>(guid1, "LOLOLOL");
            CodedObject<Guid, string> Test2 = new CodedObject<Guid, string>(guid1, "LOLO LOL");
            CodedObject<Guid, string> Test3 = new CodedObject<Guid, string>(guid2, "LOLOLOL");
            CodedObject<Guid, string> Test4 = new CodedObject<Guid, string>(guid2, "LOLO LOL");

            Assert.Equal(Test1, Test2); // Если отличаются именования, а не коды - всё в порядке.
            Assert.Equal(Test1, Test3); // Если отличаются именования, а не коды - всё в порядке.
            Assert.Equal(Test1, Test4); // Если отличаются именования, а не коды - всё в порядке.
            Assert.Equal(Test2, Test3); // Если отличаются именования, а не коды - всё в порядке.
            Assert.Equal(Test3, Test4); // Если отличаются именования, а не коды - всё в порядке.
            Assert.Equal(Test2, Test4); // Если отличаются именования, а не коды - всё в порядке.
        }

        [Fact]
        public void GuidEmptyCheck()
        {
            var guid1 = new Guid("1e52df3f-9ce3-4734-9d93-24fc2e652fc9");
            CodedObject<Guid, string> Test1 = new CodedObject<Guid, string>(guid1, "LOLOLOL");
            CodedObject<Guid, string> Test2 = new CodedObject<Guid, string>(Guid.NewGuid(), "LOLOLOL");
            CodedObject<Guid, string> Test3 = new CodedObject<Guid, string>(Guid.Empty, "LOLO LOL");

            Assert.False(Test1.IsEmpty);
            Assert.False(Test2.IsEmpty);
            Assert.True(Test3.IsEmpty); // Если отличаются коды - он не должен сработать.
        }

        [Fact]
        public void GuidNonEqualCheck()
        {
            var guid1 = new Guid("1e52df3f-9ce3-4734-9d93-24fc2e652fc9");
            CodedObject<Guid, string> Test1 = new CodedObject<Guid, string>(guid1, "LOLOLOL");
            CodedObject<Guid, string> Test2 = new CodedObject<Guid, string>(Guid.NewGuid(), "LOLOLOL");
            CodedObject<Guid, string> Test3 = new CodedObject<Guid, string>(Guid.Empty, "LOLO LOL");

            Assert.NotEqual(Test1, Test2); // Если отличаются коды - он не должен сработать.
            Assert.NotEqual(Test1, Test3); // Если отличаются коды - он не должен сработать.
        }
    }

}