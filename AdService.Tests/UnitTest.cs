using AdService.Services;

namespace AdService.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Search_ShouldReturnEmpty_WhenNoDataLoaded()
        {
            var service = new AdIndexService();
            var result = service.Search("/ru/msk");
            Assert.Empty(result);
        }

        [Fact]
        public void LoadFromFile_ShouldLoadPlatformsCorrectly()
        {
            var service = new AdIndexService();
            var filePath = Path.GetTempFileName();

            File.WriteAllLines(filePath, new[]
            {
                "Яндекс.Директ:/ru",
                "Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik",
                "Крутая реклама:/ru/svrd"
            });

            service.LoadFromFile(filePath);

            var result1 = service.Search("/ru");
            var result2 = service.Search("/ru/svrd");
            var result3 = service.Search("/ru/svrd/revda");

            Assert.Contains("Яндекс.Директ", result1);
            Assert.Single(result1);

            Assert.Contains("Яндекс.Директ", result2);
            Assert.Contains("Крутая реклама", result2);
            Assert.Equal(2, result2.Count);

            Assert.Contains("Яндекс.Директ", result3);
            Assert.Contains("Ревдинский рабочий", result3);
            Assert.Contains("Крутая реклама", result3);
            Assert.Equal(3, result3.Count);

            File.Delete(filePath);
        }

        [Fact]
        public void Search_ShouldReturnOnlyGlobal_WhenUnknownLocation()
        {
            var service = new AdIndexService();
            var filePath = Path.GetTempFileName();

            File.WriteAllLines(filePath, new[]
            {
                "Яндекс.Директ:/ru",
                "Газета уральских москвичей:/ru/msk"
            });

            service.LoadFromFile(filePath);

            var result = service.Search("/ru/novosib");

            Assert.Contains("Яндекс.Директ", result);
            Assert.DoesNotContain("Газета уральских москвичей", result);
            Assert.Single(result);

            File.Delete(filePath);
        }

        [Fact]
        public void LoadFromFile_ShouldIgnoreInvalidLines()
        {
            var service = new AdIndexService();
            var filePath = Path.GetTempFileName();

            File.WriteAllLines(filePath, new[]
            {
                "просто строка без всего",
                " :/ru/msk",
                "Газета:/ru/msk",
                ""
            });

            service.LoadFromFile(filePath);

            var result = service.Search("/ru/msk");

            Assert.Contains("Газета", result);
            Assert.Single(result);

            File.Delete(filePath);
        }

        [Fact]
        public void LoadFromFile_ShouldRemoveDuplicates()
        {
            var service = new AdIndexService();
            var filePath = Path.GetTempFileName();

            File.WriteAllLines(filePath, new[]
            {
                "Яндекс.Директ:/ru",
                "Яндекс.Директ:/ru"
            });

            service.LoadFromFile(filePath);

            var result = service.Search("/ru");

            Assert.Contains("Яндекс.Директ", result);
            Assert.Single(result);

            File.Delete(filePath);
        }

        [Fact]
        public void LoadFromFile_ShouldThrow_WhenFileDoesNotExist()
        {
            var svc = new AdIndexService();
            Assert.Throws<FileNotFoundException>(() => svc.LoadFromFile("nofile.txt"));
        }
    }
}