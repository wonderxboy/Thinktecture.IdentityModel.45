/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license.txt
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Constants;

namespace Thinktecture.IdentityModel.Clients
{
    public class OidcClient
    {
        public static Uri CreateAuthorizeUrl(Uri authorizeEndpoint, Uri redirectUri, string clientId, string scopes, string state, string responseType = "code")
        {
            var queryString = string.Format("?client_id={0}&scope={1}&redirect_uri={2}&state={3}&response_type={4}",
                WebUtility.UrlEncode(clientId),
                WebUtility.UrlEncode(scopes),
                WebUtility.UrlEncode(redirectUri.AbsoluteUri),
                WebUtility.UrlEncode(state),
                responseType);

            return new Uri(authorizeEndpoint.AbsoluteUri + queryString);
        }

        public static OidcAuthorizeResponse ParseAuthorizeResponse(NameValueCollection query)
        {
            var response = new OidcAuthorizeResponse
            {
                Error = query["error"],
                Code = query["code"],
                State = query["state"]
            };

            response.IsError = !string.IsNullOrWhiteSpace(response.Error);
            return response;
        }

        public async static Task<OidcTokenResponse> CallTokenEndpointAsync(Uri tokenEndpoint, Uri redirectUri, string code, string clientId, string clientSecret)
        {
            var client = new HttpClient
            {
                BaseAddress = tokenEndpoint
            };

            client.SetBasicAuthentication(clientId, clientSecret);

            var parameter = new Dictionary<string, string>
                {
                    { OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.AuthorizationCode },
                    { OAuth2Constants.Code, code },
                    { OAuth2Constants.RedirectUri, redirectUri.AbsoluteUri }
                };

            var response = await client.PostAsync("", new FormUrlEncodedContent(parameter));
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            return json.ToObject<OidcTokenResponse>();
        }

        public static OidcTokenResponse RefreshAccessToken(Uri tokenEndpoint, string clientId, string clientSecret, string refreshToken)
        {
            var client = new OAuth2Client(
                tokenEndpoint,
                clientId,
                clientSecret);

            var response = client.RequestAccessTokenRefreshToken(refreshToken);

            return new OidcTokenResponse
            {
                AccessToken = response.AccessToken,
                ExpiresIn = response.ExpiresIn,
                TokenType = response.TokenType,
                RefreshToken = refreshToken
            };
        }

        public static IEnumerable<Claim> ValidateIdentityToken(string token, string issuer, string audience, X509Certificate2 signingCertificate, X509CertificateValidator certificateValidator = null)
        {
            if (certificateValidator == null)
            {
                certificateValidator = X509CertificateValidator.None;
            }

            var configuration = new SecurityTokenHandlerConfiguration
            {
                CertificateValidator = certificateValidator
            };

            var handler = new JwtSecurityTokenHandler
            {
                Configuration = configuration
            };

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                AllowedAudience = audience,
                SigningToken = new X509SecurityToken(signingCertificate)
            };

            return handler.ValidateToken(token, parameters).Claims;
        }

        public async static Task<IEnumerable<Claim>> GetUserInfoClaimsAsync(Uri userInfoEndpoint, string accessToken)
        {
            var client = new HttpClient
            {
                BaseAddress = userInfoEndpoint
            };

            client.SetBearerToken(accessToken);

            var response = await client.GetAsync("");
            response.EnsureSuccessStatusCode();

            var dictionary = await response.Content.ReadAsAsync<Dictionary<string, string>>();

            var claims = new List<Claim>();
            foreach (var pair in dictionary)
            {
                if (pair.Value.Contains(','))
                {
                    foreach (var item in pair.Value.Split(','))
                    {
                        claims.Add(new Claim(pair.Key, item));
                    }
                }
                else
                {
                    claims.Add(new Claim(pair.Key, pair.Value));
                }
            }

            return claims;
        }
    }
}
