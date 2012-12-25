namespace Thinktecture.Samples
{
    public static class Constants
    {
        //
        // change the below constants to match your local system
        //

        public const string WebHostName = "roadie";
        public const string SelfHostName = "roadie";
        
        public const string WebHostAppName = "/webapisecurity/api/";
        public const string SelfHostAppName = "/webapisecurity/api/";

        public const string WebHostBaseAddress = "https://" + WebHostName + WebHostAppName;
        public const string SelfHostBaseAddress = "https://" + SelfHostName + SelfHostAppName;


        //
        // These are the config settings for the sts - don't need to be changed if you use the sample idsrv
        //
        public const string Realm = "urn:webapisecurity";
        public const string Audience = Realm;
        public const string Scope = Realm;

        public const string OAuth2Endpoint = "https://identity.thinktecture.com/sample/issue/oauth2/token";
        public const string Issuer = "http://identityserver.v2.thinktecture.com/samples";
        public const string IdentityServerSigningKey = "fWUU28oBOIcaQuwUKiL01KztD/CsZX83C3I0M1MOYN4=";

        public const string OAuthClientName = "client";
        public const string OAuthClientSecret = "secret";
    }
}
