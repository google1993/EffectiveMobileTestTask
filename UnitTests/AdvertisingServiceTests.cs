using EMTestTask.Configs;
using EMTestTask.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTests
{
    public class AdvertisingServiceTests
    {
        private readonly Mock<ILogger<AdvertisingService>> _loggerMock;
        private readonly Mock<IOptions<AdvertisingSetting>> _optionsMock;
        private readonly AdvertisingService _advertisingService;

        public AdvertisingServiceTests()
        {
            _loggerMock = new Mock<ILogger<AdvertisingService>>();

            var settings = new AdvertisingSetting()
            {
                MapFile = "map.txt"
            };
            _optionsMock = new Mock<IOptions<AdvertisingSetting>>();
            _optionsMock.Setup(o => o.Value).Returns(settings);

            _advertisingService = new AdvertisingService(_loggerMock.Object, _optionsMock.Object);
        }

        [Fact]
        public void LoadFromFile_ShouldParseCorrectly()
        {
            using var fileStream = new FileStream(_optionsMock.Object.Value.MapFile, FileMode.Open, FileAccess.Read);
            var result = _advertisingService.LoadFromFile(fileStream);

            Assert.Equal(0, result.Result);
            Assert.Equal(0, result.LinesWithErrors);
            Assert.Equal(4, result.LinesTotal);            
        }

        [Fact]
        public void FindSites_ShouldReturnAllMatching_1()
        {
            _advertisingService.Reset();

            var sites = _advertisingService.FindSites("/ru/msk");
            Assert.Equal(2, sites.Count());
            Assert.Contains("Газета уральских москвичей", sites);
            Assert.Contains("Яндекс.Директ", sites);
        }

        [Fact]
        public void FindSites_ShouldReturnAllMatching_2()
        {
            _advertisingService.Reset();

            var sites = _advertisingService.FindSites("/ru/svrd");
            Assert.Equal(2, sites.Count());
            Assert.Contains("Крутая реклама", sites);
            Assert.Contains("Яндекс.Директ", sites);
        }

        [Fact]
        public void FindSites_ShouldReturnAllMatching_3()
        {
            _advertisingService.Reset();

            var sites = _advertisingService.FindSites("/ru/svrd/revda");
            Assert.Equal(3, sites.Count());
            Assert.Contains("Крутая реклама", sites);
            Assert.Contains("Яндекс.Директ", sites);
            Assert.Contains("Ревдинский рабочий", sites);
        }

        [Fact]
        public void FindSites_ShouldReturnAllMatching_4()
        {
            _advertisingService.Reset();

            var sites = _advertisingService.FindSites("/ru");
            Assert.Single(sites);
            Assert.Contains("Яндекс.Директ", sites);
        }


    }
}