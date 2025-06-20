using Facelift_App.Services;
using System.Threading.Tasks;

namespace Facelift_App.Validators
{
    public class ProducerValidator
    {

        private readonly IPalletProducers IPalletProducers;


        public ProducerValidator(IPalletProducers Producers)
        {
            IPalletProducers = Producers;
        }

        public async Task<string> IsUniqueName(string ProducerName, string id)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            string errMsg = "true";
            MsProducer data = await IPalletProducers.GetDataByProducerNameAsync(ProducerName);
            if (data != null && !data.ProducerName.Equals(id))
            {
                errMsg = "Producer Name already registered.";
            }
            return errMsg;
        }

        public async Task<string> IsExist(string ProducerName)
        {
            //logic explanation, if errMsg is "true" then data pass the validation
            return await IPalletProducers.GetDataByIdAsync(ProducerName) != null ? "true" : "Producer not recognized.";
        }

    }
}