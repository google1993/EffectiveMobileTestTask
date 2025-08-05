using EMTestTask.Configs;
using EMTestTask.Logic;
using EMTestTask.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;
using System.Linq;
using System.Text;

namespace UnitTests
{
    public class AdvertisingMapTests
    {
        [Fact]
        public void Fill_1()
        {
            var advMap = new AdvertizingMap();

            advMap.Add("/1/2/3", "1");

            var regions = new string[] { "/1", "/1/2", "/1/2/3", "/1/2/3/4" };
            var agents = new string[][] { [], [], ["1"], ["1"] };

            for (int i = 0; i < regions.Length; i++)
            {
                var factAgents = advMap.FindAgents(regions[i]);
                Assert.Equal(agents[i].Length, factAgents.Count());
                foreach (var agent in agents[i])
                {
                    Assert.Contains(agent, factAgents);
                }
            }
        }

        [Fact]
        public void Fill_2()
        {
            var advMap = new AdvertizingMap();

            advMap.Add("/1/2/3", "1");
            advMap.Add("/1", "2");
            var regions = new string[] { "/1", "/1/2", "/1/2/3", "/1/2/3/4" };
            var agents = new string[][] { ["2"], ["2"], ["1", "2"], ["1", "2"] };

            for (int i = 0; i < regions.Length; i++)
            {
                var factAgents = advMap.FindAgents(regions[i]);
                Assert.Equal(agents[i].Length, factAgents.Count());
                foreach (var agent in agents[i])
                {
                    Assert.Contains(agent, factAgents);
                }
            }
        }

        [Fact]
        public void Fill_3()
        {
            var advMap = new AdvertizingMap();

            advMap.Add("/1", "2");
            advMap.Add("/1/2/3", "1");

            var regions = new string[] { "/1", "/1/2", "/1/2/3", "/1/2/3/4" };
            var agents = new string[][] { ["2"], ["2"], ["1", "2"], ["1", "2"] };

            for (int i = 0; i < regions.Length; i++)
            {
                var factAgents = advMap.FindAgents(regions[i]);
                Assert.Equal(agents[i].Length, factAgents.Count());
                foreach (var agent in agents[i])
                {
                    Assert.Contains(agent, factAgents);
                }
            }
        }

        [Fact]
        public void Fill_4()
        {
            var advMap = new AdvertizingMap();

            advMap.Add("/1", "1");

            var regions = new string[] { "/2" };
            var agents = new string[][] { [] };

            for (int i = 0; i < regions.Length; i++)
            {
                var factAgents = advMap.FindAgents(regions[i]);
                Assert.Equal(agents[i].Length, factAgents.Count());
                foreach (var agent in agents[i])
                {
                    Assert.Contains(agent, factAgents);
                }
            }
        }
    }
}