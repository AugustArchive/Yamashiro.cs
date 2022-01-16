using System.Threading.Tasks;

namespace Yamashiro
{
    class Program
    {
        async static Task Main(string[] args) => await new Bootstrap().StartAsync();
    }
}