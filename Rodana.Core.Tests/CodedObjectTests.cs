using Rodana.Core.Common;

namespace Rodana.Core.Tests
{
    public class CodedObjectTests
    {
        [Fact]
        public void StringEqualCheck()
        {
            CodedObject<string> Test1 = new CodedObject<string>("abc", "LOLOLOL");
            CodedObject<string> Test4 = new CodedObject<string>("abc", "LOLO LOL");

            Assert.Equal(Test1, Test4); // Если отличаются именования, а не коды - всё в порядке.
        }

        [Fact]
        public void StringEmptyCheck()
        {
            CodedObject<string> Test4 = new CodedObject<string>("abc", "LOLO LOL");
            CodedObject<string> Test5 = new CodedObject<string>("", "LOLO LOL");
            CodedObject<string> Test6 = new CodedObject<string>(null, "LOLO LOL");

            Assert.False(Test4.IsEmpty);
            Assert.False(Test5.IsEmpty);
            Assert.True(Test6.IsEmpty);
        }

        [Fact]
        public void StringNonEqualCheck()
        {
            CodedObject<string> Test1 = new CodedObject<string>("abc", "LOLOLOL");
            CodedObject<string> Test3 = new CodedObject<string>("abcd", "LOLOLOL");
            CodedObject<string> Test4 = new CodedObject<string>("abcd", "LOLO LOL");
            CodedObject<string> Test5 = new CodedObject<string>("", "LOLO LOL");
            CodedObject<string> Test6 = new CodedObject<string>(null, "LOLO LOL");

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
            CodedObject<Guid> Test1 = new CodedObject<Guid>(guid1, "LOLOLOL");
            CodedObject<Guid> Test2 = new CodedObject<Guid>(guid1, "LOLO LOL");
            CodedObject<Guid> Test3 = new CodedObject<Guid>(guid2, "LOLOLOL");
            CodedObject<Guid> Test4 = new CodedObject<Guid>(guid2, "LOLO LOL");

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
            CodedObject<Guid> Test1 = new CodedObject<Guid>(guid1, "LOLOLOL");
            CodedObject<Guid> Test2 = new CodedObject<Guid>(Guid.NewGuid(), "LOLOLOL");
            CodedObject<Guid> Test3 = new CodedObject<Guid>(Guid.Empty, "LOLO LOL");

            Assert.False(Test1.IsEmpty);
            Assert.False(Test2.IsEmpty);
            Assert.True(Test3.IsEmpty); // Если отличаются коды - он не должен сработать.
        }

        [Fact]
        public void GuidNonEqualCheck()
        {
            var guid1 = new Guid("1e52df3f-9ce3-4734-9d93-24fc2e652fc9");
            CodedObject<Guid> Test1 = new CodedObject<Guid>(guid1, "LOLOLOL");
            CodedObject<Guid> Test2 = new CodedObject<Guid>(Guid.NewGuid(), "LOLOLOL");
            CodedObject<Guid> Test3 = new CodedObject<Guid>(Guid.Empty, "LOLO LOL");

            Assert.NotEqual(Test1, Test2); // Если отличаются коды - он не должен сработать.
            Assert.NotEqual(Test1, Test3); // Если отличаются коды - он не должен сработать.
        }
    }

}