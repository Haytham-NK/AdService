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
                "������.������:/ru",
                "���������� �������:/ru/svrd/revda,/ru/svrd/pervik",
                "������ �������:/ru/svrd"
            });

            service.LoadFromFile(filePath);

            var result1 = service.Search("/ru");
            var result2 = service.Search("/ru/svrd");
            var result3 = service.Search("/ru/svrd/revda");

            Assert.Contains("������.������", result1);
            Assert.Single(result1);

            Assert.Contains("������.������", result2);
            Assert.Contains("������ �������", result2);
            Assert.Equal(2, result2.Count);

            Assert.Contains("������.������", result3);
            Assert.Contains("���������� �������", result3);
            Assert.Contains("������ �������", result3);
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
                "������.������:/ru",
                "������ ��������� ���������:/ru/msk"
            });

            service.LoadFromFile(filePath);

            var result = service.Search("/ru/novosib");

            Assert.Contains("������.������", result);
            Assert.DoesNotContain("������ ��������� ���������", result);
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
                "������ ������ ��� �����",
                " :/ru/msk",
                "������:/ru/msk",
                ""
            });

            service.LoadFromFile(filePath);

            var result = service.Search("/ru/msk");

            Assert.Contains("������", result);
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
                "������.������:/ru",
                "������.������:/ru"
            });

            service.LoadFromFile(filePath);

            var result = service.Search("/ru");

            Assert.Contains("������.������", result);
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