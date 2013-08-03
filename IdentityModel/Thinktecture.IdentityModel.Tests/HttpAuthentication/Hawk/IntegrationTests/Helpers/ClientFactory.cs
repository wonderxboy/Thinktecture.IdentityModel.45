using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Http.Hawk.Client;
using Thinktecture.IdentityModel.Http.Hawk.Core;
using Thinktecture.IdentityModel.Http.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Http.Hawk.Core.MessageContracts;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers
{
    class ClientFactory
    {
        internal static HawkClient Create(Func<Credential> callback = null,
                                            Func<IRequestMessage, string> normalizationCallback = null,
                                                Func<IRequestMessage, bool> requestPayloadHashabilityCallback = null,
                                                    Func<IResponseMessage, string, bool> verificationCallback = null)
        {
            var options = new ClientOptions()
            {
                LocalTimeOffsetMillis = 0,
                EnableResponseValidation = true,
                EnableAutoCompensationForClockSkew = true,
                CredentialsCallback = callback ?? (() => ServerFactory.DefaultCredential),
                NormalizationCallback = normalizationCallback,
                RequestPayloadHashabilityCallback = requestPayloadHashabilityCallback,
                VerificationCallback = verificationCallback
            };

            return new HawkClient(options);
        }
    }
}
