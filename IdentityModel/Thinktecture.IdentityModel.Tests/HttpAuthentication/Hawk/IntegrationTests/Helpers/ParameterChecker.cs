using System;
using System.Text.RegularExpressions;

namespace Thinktecture.IdentityModel.Tests.HttpAuthentication.Hawk.IntegrationTests.Helpers
{
    internal class ParameterChecker
    {
        public static bool IsFieldPresent(string parameter, string fieldName)
        {
            return Regex.Match(parameter, String.Format(@"({0})=""([^""\\]*)""\s*(?:,\s*|$)", fieldName)).Success;
        }
    }
}
