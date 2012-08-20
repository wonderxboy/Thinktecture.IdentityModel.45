using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;
using System.Linq;

namespace Thinktecture.IdentityModel.Tokens.Http
{
    public class ClientCertificateHandler : X509SecurityTokenHandler
    {
        public ClientCertificateHandler(ClientCertificateSecurityMode mode, params string[] values)
        {
            X509CertificateValidator validator;
            HttpClientCertificateIssuerNameRegistry registry;

            // set validator and registry
            if (mode == ClientCertificateSecurityMode.ChainValidation)
            {
                validator = X509CertificateValidator.ChainTrust;
                registry = new HttpClientCertificateIssuerNameRegistry(false, mode);
            }
            if (mode == ClientCertificateSecurityMode.ChainValidationWithIssuerSubjectName ||
                mode == ClientCertificateSecurityMode.ChainValidationWithIssuerThumbprint)
            {
                validator = X509CertificateValidator.ChainTrust;
                registry = new HttpClientCertificateIssuerNameRegistry(true, mode, values);
            }
            else if (mode == ClientCertificateSecurityMode.PeerValidation)
            {
                validator = X509CertificateValidator.PeerTrust;
                registry = new HttpClientCertificateIssuerNameRegistry(false, mode);
            }
            else if (mode == ClientCertificateSecurityMode.IssuerThumbprint)
            {
                validator = X509CertificateValidator.None;
                registry = new HttpClientCertificateIssuerNameRegistry(true, mode, values);
            }
            else
            {
                throw new ArgumentException("mode");
            }

            Configuration = new SecurityTokenHandlerConfiguration
            {
                CertificateValidationMode = X509CertificateValidationMode.Custom,
                CertificateValidator = validator,
                IssuerNameRegistry = registry
            };
        }
    }

    public enum ClientCertificateSecurityMode
    {
        ChainValidation,
        PeerValidation,
        ChainValidationWithIssuerSubjectName,
        ChainValidationWithIssuerThumbprint,
        IssuerThumbprint
    }

    public class HttpClientCertificateIssuerNameRegistry : IssuerNameRegistry
    {
        string[] _values;
        bool _checkIssuer;
        ClientCertificateSecurityMode _mode;

        public HttpClientCertificateIssuerNameRegistry(bool checkIssuer, ClientCertificateSecurityMode mode, params string[] values)
        {
            _checkIssuer = checkIssuer;
            _mode = mode;
            _values = values;
        }

        public override string GetIssuerName(SecurityToken securityToken)
        {
            var token = securityToken as X509SecurityToken;
            if (token == null)
            {
                throw new ArgumentException("securityToken");
            }

            var cert = token.Certificate;
            string hit = null;

            // no check
            if (!_checkIssuer)
            {
                return cert.Subject;
            }

            // check for subject name
            if (_mode == ClientCertificateSecurityMode.ChainValidationWithIssuerSubjectName)
            {
                hit = _values.FirstOrDefault(s => s.Equals(cert.Subject, StringComparison.OrdinalIgnoreCase));
            }

            // check for subject name
            if (_mode == ClientCertificateSecurityMode.ChainValidationWithIssuerThumbprint ||
                _mode == ClientCertificateSecurityMode.IssuerThumbprint)
            {
                hit = _values.FirstOrDefault(s => s.Equals(cert.Thumbprint, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(hit))
            {
                return cert.Subject;
            }
            
            return null;
        }
    }
}
