using System.Net.Http;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models;
#if VQT_HAS_NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace KRT.VRCQuestTools.Services
{
    /// <summary>
    /// VPM operations.
    /// </summary>
    internal class VPMService
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Initializes a new instance of the <see cref="VPMService"/> class.
        /// </summary>
        internal VPMService()
        {
            Client.Timeout = System.TimeSpan.FromSeconds(10);
            Client.DefaultRequestHeaders.Add("User-Agent", VRCQuestTools.Name);
        }

        /// <summary>
        /// Get VPM repository.
        /// </summary>
        /// <param name="url">VPM repository URL.</param>
        /// <returns>Parsed VPM repository.</returns>
        internal async Task<VPMRepository> GetVPMRepository(string url)
        {
#if VQT_HAS_NEWTONSOFT_JSON
            var result = await Client.GetStringAsync(url);
            var repo = JsonConvert.DeserializeObject<VPMRepository>(result);
            return repo;
#else
            throw new System.NotImplementedException("NewtonSoft Json is missing");
#endif
        }
    }
}
